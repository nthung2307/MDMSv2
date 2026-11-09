using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MediatR.Extensions.Autofac.DependencyInjection;
using Nobisoft.Core.DI;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.MySql;
using Infrastructure;
using Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using Nobisoft.Core.AspNetCore.Middleware;
using Nobisoft.Core.Configuration;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Infrastructure.Swagger;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using VineHunter.Infrastructure.Common;
using WebApi;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment.EnvironmentName;

Console.WriteLine($"==> ⚙️⚙️⚙️⚙️⚙️ Current Environment info: {env}");

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Call ConfigureContainer on the Host sub property
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Auto-register modules
    containerBuilder.AutoRegisterModules<WebApiModule>();

    // Register MediatR
    containerBuilder.RegisterMediatR(typeof(InfrastructureModule).Assembly);
});

// Add services to the container.
builder.Services.AddAutoMapper(typeof(InfrastructureModule).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<Application.Services.IEmailProviderFactory, Infrastructure.Services.EmailProviders.EmailProviderFactory>();
builder.Services.AddScoped<Application.Services.INotificationTemplateService, Infrastructure.Services.Templates.NotificationTemplateService>();
builder.Services.AddScoped<Application.Services.IStatementPdfGenerator, Infrastructure.Services.Templates.StatementPdfGenerator>();
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddControllers()
    .AddJsonOptions(options => { options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });
builder.Services.AddFluentValidationAutoValidation().AddValidatorsFromAssembly(typeof(InfrastructureModule).Assembly);

#region Swagger

builder.Services.AddSwaggerGen(c =>
{
    c.ExampleFilters();
    c.EnableAnnotations();
    c.SchemaFilter<SchemaFilter>();

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Manual Swagger configuration with versioning
    var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        c.SwaggerDoc(description.GroupName, new OpenApiInfo
        {
            Title = $"Your API {description.ApiVersion}",
            Version = description.ApiVersion.ToString()
        });
    }
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

#endregion

#region Api Versioning

builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
    config.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"));
    config.ErrorResponses = new ApiVersioningErrorResponseProvider();
});

builder.Services.AddVersionedApiExplorer(config =>
{
    config.GroupNameFormat = "'v'VVV";
    config.SubstituteApiVersionInUrl = true; // To show api version param in swagger
});

#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// DB
builder.Services.Configure<AppDbConfiguration>(builder.Configuration.GetSection("AppDb"));
var appDb = new AppDbConfiguration();
builder.Configuration.GetSection("AppDb").Bind(appDb);

// JWT
builder.Services.Configure<Nobisoft.Core.Authorization.Commons.JwtConfiguration>(builder.Configuration.GetSection("JwtConfigurations"));

// Email
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("Email"));
var appEmail = new EmailConfiguration();
builder.Configuration.GetSection("Email").Bind(appEmail);
builder.Services.AddHealthChecks();

builder.Services.AddHttpContextAccessor();
builder.Services.AddFeatureManagement();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(config =>
    {
        config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
//.AddCookie(config => config.SlidingExpiration = true)
.AddJwtBearer(config =>
{
    config.RequireHttpsMetadata = false;
    config.SaveToken = true;
    config.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = builder.Configuration["JwtConfigurations:Issuer"] ?? "api-amr.nobisoft.vn",
        ValidAudience = builder.Configuration["JwtConfigurations:Audience"] ?? "api-amr.nobisoft.vn",
        IssuerSigningKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtConfigurations:SymmetricSecurityKey"] ?? string.Empty)),
    };
    config.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/notification/web") ||
                 path.StartsWithSegments("/listnotificationhub") ||
                 path.StartsWithSegments("/detailnotificationhub") ||
                 path.StartsWithSegments("/detailnotificationdatetimehub") ||
                 path.StartsWithSegments("/notificationeventhub")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var expClaim = context.Principal?.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Exp)?.Value;
            if (expClaim != null && long.TryParse(expClaim, out var exp))
            {
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                if (expirationTime <= DateTime.UtcNow)
                {
                    context.Fail("Token has expired.");
                    return Task.CompletedTask;
                }
            }

            var userIdClaim =
                context.Principal?.Claims.FirstOrDefault(claim => claim.Type == "id");
            if (userIdClaim == null)
            {
                context.Fail("JWT Token does not contain User Id Claim.");
            }

            Console.WriteLine(@"Token Validated OK");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            if (string.IsNullOrEmpty(context.Error))
                context.Error = "invalid_token";
            if (string.IsNullOrEmpty(context.ErrorDescription))
            {
                if (context.AuthenticateFailure != null &&
                    context.AuthenticateFailure.Message.Length > 0)
                {
                    context.ErrorDescription = context.AuthenticateFailure.Message;
                }
                else
                {
                    context.ErrorDescription =
                        "This request requires a valid JWT access token to be provided.";
                }
            }

            if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() ==
                typeof(SecurityTokenExpiredException))
            {
                var authenticationException =
                    context.AuthenticateFailure as SecurityTokenExpiredException;
                context.Response.Headers.Append("WWW-Authenticate", "Bearer");
                context.ErrorDescription = $"The token expired on {authenticationException?.Expires:o}";
            }

            var responsePayload = new
            {
                status = 401,
                error = context.Error,
                errorDescription = context.ErrorDescription
            };

            return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload));
        }
    };
});

builder.Services.AddScoped<EmailSenderService>();
builder.Services.AddScoped<EmailSenderSingleService>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 8; // Set the required password length to 8 characters
    options.Password.RequireDigit = true; // Require a digit
    options.Password.RequireLowercase = true; // Require lowercase
    options.Password.RequireUppercase = true; // Require uppercase
    options.Password.RequireNonAlphanumeric = true; // Require special character
    options.Password.RequiredUniqueChars = 4; // Require at least 4 unique characters
});


builder.Services.AddPooledDbContextFactory<AppReadWriteDbContext>(options =>
{
    var connectionString = $"Server={appDb.Server};Port={appDb.Port};Database={appDb.Database};User={appDb.UserName};Password={appDb.Password};Default Command Timeout=180;Connection Timeout=180;Allow User Variables=true;AllowPublicKeyRetrieval=true;";

    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), opts =>
    {
        opts.MigrationsAssembly(typeof(WebApiModule).Assembly.FullName);
        opts.CommandTimeout(180);
    });
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
    options.UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddSerilog();
    }));
}, poolSize: 128);

builder.Services.AddPooledDbContextFactory<AppReadOnlyDbContext>(options =>
{
    var connectionString = $"Server={appDb.Server};Port={appDb.Port};Database={appDb.Database};User={appDb.UserName};Password={appDb.Password};Default Command Timeout=180;Connection Timeout=180;Allow User Variables=true;AllowPublicKeyRetrieval=true;";     // Optional, but often needed for long-running operations

    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)), opts =>
    {
        opts.MigrationsAssembly(typeof(WebApiModule).Assembly.FullName);
        opts.CommandTimeout(180);
    });
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
    options.UseLoggerFactory(LoggerFactory.Create(loggingBuilder =>
    {
        loggingBuilder.AddConsole();
        loggingBuilder.AddSerilog();
    }));
}, poolSize: 128);
builder.Services.AddHostedService<NotificationService>();
builder.Services.AddSignalR();
builder.Services.AddHangfire(configuration =>
{
    configuration.UseStorage(
        new MySqlStorage(
            $"server={appDb.Server};uid={appDb.UserName};pwd={appDb.Password};database={appDb.Database};port={appDb.Port};Allow User Variables=True;AllowPublicKeyRetrieval=True",
            new MySqlStorageOptions
            {
                TablesPrefix = "Hangfire",
                QueuePollInterval = TimeSpan.FromSeconds(15), // Adjust polling interval to reduce load
                JobExpirationCheckInterval = TimeSpan.FromHours(1), // Reduce frequency of job expiration checks
            }
        )
    );
});

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount; // Adjust worker count
    options.Queues = new[] { "default" };
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15); // Poll every 15 seconds
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllDomains",
        corsPolicyBuilder => corsPolicyBuilder
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(option =>
{
    option.EnableFilter();
    // To get list of available versions
    var apiVersionProvider = app.Services.GetService<IApiVersionDescriptionProvider>();
    if (apiVersionProvider != null)
    {
        foreach (var description in apiVersionProvider.ApiVersionDescriptions)
        {
            option.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName);
        }
    }
});

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure health check endpoint
app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonConvert.SerializeObject(
            new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    exception = entry.Value.Exception?.Message,
                    duration = entry.Value.Duration.ToString()
                }).ToDictionary(c => c.name, c => new { c.status, c.exception, c.duration })
            }, Formatting.Indented);
        await context.Response.WriteAsync(result);
    }
});

app.UseHttpsRedirection();
app.UseHangfireDashboard("/dashboard");
RecurringJob.AddOrUpdate<BackgroundJobService>(
    "GroupDataPerMeter",
    job => job.GroupDataPerMeter(CancellationToken.None),
    "10 * * * *"
);
RecurringJob.AddOrUpdate<BackgroundJobService>(
    "GenerateEnergyReportDaily",
    job => job.GenerateEnergyReportDaily(CancellationToken.None),
    "5 0 * * *",
    new RecurringJobOptions()
    {
        TimeZone = TimeZoneInfo.Local
    }
);
RecurringJob.AddOrUpdate<BackgroundJobService>(
    "GenerateEnergyReportHalfHour",
    job => job.GenerateEnergyReportHalfHour(CancellationToken.None),
    "5,35 * * * *"
);
RecurringJob.AddOrUpdate<BackgroundJobService>(
    "GenerateOperatingReportHalfHour",
    job => job.GenerateOperatingReportHalfHour(CancellationToken.None),
    "5,35 * * * *"
);
RecurringJob.AddOrUpdate<BackgroundJobService>(
    "CleanupOldEvents",
    job => job.CleanupOldEvents(CancellationToken.None),
    "0 2 * * *"
);
//
// RecurringJob.AddOrUpdate<BackgroundJobArchiveDataService>(
//     "ArchiveDataMeasurementPointMonitoring",
//     job => job.ArchiveDataMeasurementPointMonitoring(CancellationToken.None),
//     "30 0 * * *"
// );

app.UseCors("AllowAllDomains");
app.UseMiddleware<HeaderTrackingMiddleware>();
app.UseMiddleware<ExceptionKillerMiddleware>(app.Environment.IsDevelopment());
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => $"Welcome to AMR!");
app.MapHub<ListNotificationHub>("/listnotificationhub");
app.MapHub<DetailNotificationHub>("/detailnotificationhub");
app.MapHub<DetailNotificationDateTimeHub>("/detailnotificationdatetimehub");
app.MapHub<NotificationEventHub>("/notificationeventhub");

try
{
    var dbFactory = app.Services.GetRequiredService<IDbContextFactory<AppReadWriteDbContext>>();
    using var db = dbFactory.CreateDbContext();
    await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS `MeasurementPointHalfHourOperatingReports` (
  `Id` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `CreatedTime` datetime(6) DEFAULT NULL,
  `DienApPhaA` double DEFAULT NULL,
  `DienApPhaAUnit` longtext,
  `DienApPhaB` double DEFAULT NULL,
  `DienApPhaBUnit` longtext,
  `DienApPhaC` double DEFAULT NULL,
  `DienApPhaCUnit` longtext,
  `DongDienPhaA` double DEFAULT NULL,
  `DongDienPhaAUnit` longtext,
  `DongDienPhaB` double DEFAULT NULL,
  `DongDienPhaBUnit` longtext,
  `DongDienPhaC` double DEFAULT NULL,
  `DongDienPhaCUnit` longtext,
  `CongSuatTacDungTong3Pha` double DEFAULT NULL,
  `CongSuatTacDungTong3PhaUnit` longtext,
  `CongSuatPhanKhangTong3Pha` double DEFAULT NULL,
  `CongSuatPhanKhangTong3PhaUnit` longtext,
  `HeSoCongSuatPF` double DEFAULT NULL,
  `TanSoHeThong` double DEFAULT NULL,
  `TanSoHeThongUnit` longtext,
  PRIMARY KEY (`Id`),
  KEY `IX_MeasurementPointHalfHourOperatingReports_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeasurementPointHalfHourOperatingReports_CreatedTime` (`CreatedTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
    app.Logger.LogInformation("Đã kiểm tra và đảm bảo bảng MeasurementPointHalfHourOperatingReports tồn tại.");

    // Tự động đảm bảo các bảng Email Config và Metering mới tồn tại
    await db.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS `SystemEmailConfigs` (
  `Id` bigint NOT NULL,
  `ProviderType` varchar(50) NOT NULL,
  `SenderEmail` varchar(255) NOT NULL,
  `SenderDisplayName` varchar(255) DEFAULT NULL,
  `TenantId` varchar(100) DEFAULT NULL,
  `ClientId` varchar(100) DEFAULT NULL,
  `EncryptedClientSecret` varchar(1000) DEFAULT NULL,
  `SmtpHost` varchar(255) DEFAULT NULL,
  `SmtpPort` int DEFAULT NULL,
  `SecurityMode` varchar(50) DEFAULT NULL,
  `RequireAuth` tinyint(1) NOT NULL DEFAULT '1',
  `SmtpUsername` varchar(255) DEFAULT NULL,
  `EncryptedSmtpPassword` varchar(1000) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `NotificationTemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `SubjectTemplate` varchar(500) NOT NULL,
  `BodyHtmlTemplate` longtext NOT NULL,
  `Description` varchar(1000) DEFAULT NULL,
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_NotificationTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `StatementPdfTemplates` (
  `Id` bigint NOT NULL,
  `Code` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `HeaderTitle` varchar(255) DEFAULT NULL,
  `SubHeaderTitle` varchar(255) DEFAULT NULL,
  `CompanyName` varchar(255) DEFAULT NULL,
  `CompanyAddress` varchar(500) DEFAULT NULL,
  `ContactPhone` varchar(255) DEFAULT NULL,
  `LogoBase64` longtext,
  `PaperSize` varchar(50) DEFAULT NULL,
  `EngineType` varchar(50) DEFAULT 'EXCEL_LIBREOFFICE',
  `ExcelTemplateBase64` longtext,
  `ExcelTemplateFileName` varchar(255) DEFAULT NULL,
  `DisplayColumnsJson` varchar(1000) DEFAULT NULL,
  `FooterNotes` varchar(2000) DEFAULT NULL,
  `RequireDualSignatures` tinyint(1) NOT NULL DEFAULT '1',
  `IsDefault` tinyint(1) NOT NULL DEFAULT '0',
  `IsActive` tinyint(1) NOT NULL DEFAULT '1',
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_StatementPdfTemplates_Code` (`Code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS `MeterReplacements` (
  `Id` bigint NOT NULL,
  `MeasurementPointId` bigint NOT NULL,
  `ReplacementDate` datetime(6) NOT NULL,
  `OldMeterSerial` varchar(100) NOT NULL,
  `NewMeterSerial` varchar(100) NOT NULL,
  `OldMeterFinalReading` double NOT NULL DEFAULT '0',
  `NewMeterInitialReading` double NOT NULL DEFAULT '0',
  `Reason` varchar(500) DEFAULT NULL,
  `MinutesFromCutoff` int NOT NULL DEFAULT '0',
  `OldMeterRatio` double NOT NULL DEFAULT '0',
  `NewMeterRatio` double NOT NULL DEFAULT '0',
  `Status` varchar(50) NOT NULL DEFAULT 'COMPLETED',
  `Notes` varchar(1000) DEFAULT NULL,
  `CreatedTime` datetime(6) NOT NULL,
  `ModifiedTime` datetime(6) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MeterReplacements_MeasurementPointId` (`MeasurementPointId`),
  KEY `IX_MeterReplacements_ReplacementDate` (`ReplacementDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `StatementPdfTemplates` ADD COLUMN `EngineType` varchar(50) DEFAULT 'EXCEL_LIBREOFFICE';");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `StatementPdfTemplates` ADD COLUMN `ExcelTemplateBase64` longtext;");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `StatementPdfTemplates` ADD COLUMN `ExcelTemplateFileName` varchar(255) DEFAULT NULL;");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `GcsTemporary` ADD COLUMN `NewPreviousReading` double DEFAULT NULL;");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `gcstemporary` ADD COLUMN `NewPreviousReading` double DEFAULT NULL;");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `GcsTemporary` ADD COLUMN `IsSuspicious` tinyint(1) NOT NULL DEFAULT '0';");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `gcstemporary` ADD COLUMN `IsSuspicious` tinyint(1) NOT NULL DEFAULT '0';");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `GcsTemporary` ADD COLUMN `SuspiciousReason` varchar(500) DEFAULT NULL;");
    }
    catch { }
    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE `gcstemporary` ADD COLUMN `SuspiciousReason` varchar(500) DEFAULT NULL;");
    }
    catch { }
    var extraAlterSqls = new[]
    {
        "ALTER TABLE `GcsTemporary` ADD COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` ADD COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` ADD COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `gcsmonthlydetail` ADD COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` ADD COLUMN `ReadingTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `gcsmonthlydetail` ADD COLUMN `ReadingTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `gcsmonthlydetail` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `periodicenergystatement` ADD COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `periodicenergystatement` ADD COLUMN `PmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `PreviousPmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `periodicenergystatement` ADD COLUMN `PreviousPmaxTime` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `PeriodicEnergyStatement` ADD COLUMN `BillingTimestamp` datetime(6) DEFAULT NULL;",
        "ALTER TABLE `periodicenergystatement` ADD COLUMN `BillingTimestamp` datetime(6) DEFAULT NULL;"
    };
    foreach (var sql in extraAlterSqls)
    {
        try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
    }

    // Phase 3: Chuẩn hóa kiểu số liệu đo đếm sang DECIMAL(18, 4) và DECIMAL(5, 2)
    var decimalMigrationSqls = new[]
    {
        // GcsTemporary
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `PreviousReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `CurrentReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `NewCurrentReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `NewPreviousReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `MultiplierFactor` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `Consumption` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcstemporary` MODIFY COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `PreviousReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `CurrentReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `NewCurrentReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `NewPreviousReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `MultiplierFactor` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `Consumption` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsTemporary` MODIFY COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",

        // GcsMonthlyDetail
        "ALTER TABLE `gcsmonthlydetail` MODIFY COLUMN `PreviousReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcsmonthlydetail` MODIFY COLUMN `CurrentReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcsmonthlydetail` MODIFY COLUMN `MultiplierFactor` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `gcsmonthlydetail` MODIFY COLUMN `Consumption` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `gcsmonthlydetail` MODIFY COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` MODIFY COLUMN `PreviousReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` MODIFY COLUMN `CurrentReading` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` MODIFY COLUMN `MultiplierFactor` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `GcsMonthlyDetail` MODIFY COLUMN `Consumption` decimal(18, 4) DEFAULT NULL;",
        "ALTER TABLE `GcsMonthlyDetail` MODIFY COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",

        // PeriodicEnergyStatement
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Bt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Cd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Td` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Sg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Vc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Pmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `PreviousBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `PreviousCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `PreviousTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `PreviousSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `PreviousVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `PreviousPmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `ConsumtionBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `ConsumtionCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `ConsumtionTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `ConsumtionSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `ConsumtionVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `Multiplier` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `periodicenergystatement` MODIFY COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Bt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Cd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Td` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Sg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Vc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Pmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `PreviousBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `PreviousCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `PreviousTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `PreviousSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `PreviousVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `PreviousPmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `ConsumtionBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `ConsumtionCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `ConsumtionTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `ConsumtionSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `ConsumtionVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `Multiplier` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `PeriodicEnergyStatement` MODIFY COLUMN `CosPhi` decimal(5, 2) DEFAULT NULL;",

        // MeasurementPoints
        "ALTER TABLE `measurementpoints` MODIFY COLUMN `MultiplierFactor` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `MeasurementPoints` MODIFY COLUMN `MultiplierFactor` decimal(18, 4) NOT NULL DEFAULT '1.0000';",

        // MeterReplacements
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldMeterMultiplier` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `OldPmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewMeterMultiplier` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `meterreplacements` MODIFY COLUMN `NewPmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldMeterMultiplier` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `OldPmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewMeterMultiplier` decimal(18, 4) NOT NULL DEFAULT '1.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewBt` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewCd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewTd` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewSg` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewVc` decimal(18, 4) NOT NULL DEFAULT '0.0000';",
        "ALTER TABLE `MeterReplacements` MODIFY COLUMN `NewPmax` decimal(18, 4) NOT NULL DEFAULT '0.0000';"
    };
    foreach (var sql in decimalMigrationSqls)
    {
        try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
    }

    var compositeIndexSqls = new[]
    {
        "CREATE INDEX `IX_MPMonitoring_PointId_CreatedTime` ON `MeasurementPointMonitoring` (`MeasurementPointsId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoring_PointId_CreatedTime` ON `measurementpointmonitoring` (`MeasurementPointsId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoring_PointId_Tag_CreatedTime` ON `MeasurementPointMonitoring` (`MeasurementPointsId`, `TagNameId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoring_PointId_Tag_CreatedTime` ON `measurementpointmonitoring` (`MeasurementPointsId`, `TagNameId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoringDT_PointId_CreatedTime` ON `MeasurementPointMonitoringByDateTime` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoringDT_PointId_CreatedTime` ON `measurementpointmonitoringbydatetime` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoringHourly_PointId_CreatedTime` ON `MeasurementPointMonitoringHourly` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPMonitoringHourly_PointId_CreatedTime` ON `measurementpointmonitoringhourly` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPHalfHourEnergy_PointId_CreatedTime` ON `MeasurementPointHalfHourEnergyReports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPHalfHourEnergy_PointId_CreatedTime` ON `measurementpointhalfhourenergyreports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPDailyEnergy_PointId_CreatedTime` ON `MeasurementPointDailyEnergyReports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPDailyEnergy_PointId_CreatedTime` ON `measurementpointdailyenergyreports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_GcsTemporary_Gcs_Cycle` ON `GcsTemporary` (`GcsId`, `Year`, `Month`, `PeriodNumber`);",
        "CREATE INDEX `IX_GcsTemporary_Gcs_Cycle` ON `gcstemporary` (`GcsId`, `Year`, `Month`, `PeriodNumber`);",
        "CREATE INDEX `IX_GcsMonthlyDetail_Cycle_Point` ON `GcsMonthlyDetail` (`GcsCutoffCycleId`, `MeasurementPointId`);",
        "CREATE INDEX `IX_GcsMonthlyDetail_Cycle_Point` ON `gcsmonthlydetail` (`GcsCutoffCycleId`, `MeasurementPointId`);",
        "CREATE INDEX `IX_PeriodicStatement_Gcs_Cycle` ON `PeriodicEnergyStatement` (`GcsId`, `Year`, `Month`, `PeriodNumber`);",
        "CREATE INDEX `IX_PeriodicStatement_Gcs_Cycle` ON `periodicenergystatement` (`GcsId`, `Year`, `Month`, `PeriodNumber`);",
        "CREATE INDEX `IX_GcsCutoffCycle_Gcs_Cycle` ON `GcsCutoffCycle` (`GcsId`, `Year`, `Month`, `PeriodNumber`);",
        "CREATE INDEX `IX_GcsCutoffCycle_Gcs_Cycle` ON `gcscutoffcycle` (`GcsId`, `Year`, `Month`, `PeriodNumber`);"
    };
    foreach (var indexSql in compositeIndexSqls)
    {
        try { await db.Database.ExecuteSqlRawAsync(indexSql); } catch { }
    }
    app.Logger.LogInformation("Đã kiểm tra và đảm bảo các bảng Email Config, Metering mới và Composite Indexes tồn tại.");

    var fixEncodingSqls = new[]
    {
        "SET NAMES utf8mb4;",
        @"UPDATE `StatementPdfTemplates` SET 
            `Name` = 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng',
            `HeaderTitle` = 'MONTHLY POWER METER READING',
            `SubHeaderTitle` = 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
            `CompanyName` = 'VSIP HAI PHONG CO., LTD',
            `CompanyAddress` = 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',
            `ContactPhone` = 'Tel: 84-225-3959868   Fax: 84-225-3959886',
            `PaperSize` = 'A5_LANDSCAPE',
            `EngineType` = 'QUEST_PDF',
            `DisplayColumnsJson` = '{\""columns\"":[\""BT\"",\""CD\"",\""TD\"",\""SG\"",\""VC\"",\""COSPHI\"",\""REMARKS\"",\""PMAX\"",\""MULTIPLIER\""],\""documentCode\"":\""M&E-1/01 PR04\"",\""copyLabel\"":\""(M&E Copy – Liên cho Phòng Cơ Điện)\"",\""partyAName\"":\""Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)\"",\""partyBName\"":\""Party B (Tenant)\\nBên B (Khách hàng)\"",\""headerLeftMode\"":\""LOGO\"",\""headerCenterMode\"":\""LOGO\"",\""headerRightMode\"":\""DECOR\"",\""footerLeftMode\"":\""LOGO\"",\""footerCenterMode\"":\""LOGO\"",\""footerRightMode\"":\""LOGO\""}',
            `RequireDualSignatures` = 1,
            `FooterNotes` = 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.'
          WHERE `Code` = 'A5_VSIP_STANDARD' OR `Code` = 'STATEMENT_PDF_A5_STANDARD' OR `Id` = 1;",
        @"UPDATE `statementpdftemplates` SET 
            `Name` = 'Mẫu phiếu báo A5 Ngang - Chuẩn VSIP Hải Phòng',
            `HeaderTitle` = 'MONTHLY POWER METER READING',
            `SubHeaderTitle` = 'PHIẾU GHI CHỈ SỐ ĐIỆN HÀNG THÁNG',
            `CompanyName` = 'VSIP HAI PHONG CO., LTD',
            `CompanyAddress` = 'VSIP Hai Phong Administration Office, VSIP Hai Phong Township, Industrial & Service Park\nHoa Binh Ward, Hai Phong City, Vietnam',
            `ContactPhone` = 'Tel: 84-225-3959868   Fax: 84-225-3959886',
            `PaperSize` = 'A5_LANDSCAPE',
            `EngineType` = 'QUEST_PDF',
            `DisplayColumnsJson` = '{\""columns\"":[\""BT\"",\""CD\"",\""TD\"",\""SG\"",\""VC\"",\""COSPHI\"",\""REMARKS\"",\""PMAX\"",\""MULTIPLIER\""],\""documentCode\"":\""M&E-1/01 PR04\"",\""copyLabel\"":\""(M&E Copy – Liên cho Phòng Cơ Điện)\"",\""partyAName\"":\""Party A (VSIP Hai Phong Co., Ltd)\\nBên A (Công ty TNHH VSIP Hải Phòng)\"",\""partyBName\"":\""Party B (Tenant)\\nBên B (Khách hàng)\"",\""headerLeftMode\"":\""LOGO\"",\""headerCenterMode\"":\""LOGO\"",\""headerRightMode\"":\""DECOR\"",\""footerLeftMode\"":\""LOGO\"",\""footerCenterMode\"":\""LOGO\"",\""footerRightMode\"":\""LOGO\""}',
            `RequireDualSignatures` = 1,
            `FooterNotes` = 'Quý khách vui lòng kiểm tra và phản hồi số liệu trong vòng 03 ngày làm việc kể từ ngày nhận phiếu báo.'
          WHERE `Code` = 'A5_VSIP_STANDARD' OR `Code` = 'STATEMENT_PDF_A5_STANDARD' OR `Id` = 1;",
        @"UPDATE `SystemEmailConfigs` SET `SenderDisplayName` = 'VSIP Hải Phòng - Phòng Cơ Điện' WHERE `Id` = 1;",
        @"UPDATE `systememailconfigs` SET `SenderDisplayName` = 'VSIP Hải Phòng - Phòng Cơ Điện' WHERE `Id` = 1;"
    };
    foreach (var sql in fixEncodingSqls)
    {
        try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
    }

    try
    {
        using var notifScope = app.Services.CreateScope();
        var notifSp = notifScope.ServiceProvider;
        var notifRwRepo = notifSp.GetService<Application.Repositories.ReadWrite.INotificationTemplateReadWriteRepository>();
        var snowflake = notifSp.GetService<Nobisoft.Core.Interfaces.ISnowflakeIdService>();
        if (notifRwRepo != null)
        {
            var prebuilts = Infrastructure.Services.Templates.NotificationTemplateService.GetPrebuiltDefaultTemplates();
            foreach (var template in prebuilts)
            {
                var existing = await notifRwRepo.GetDefaultByCodeAsync(template.Code, CancellationToken.None);
                if (existing != null)
                {
                    existing.Name = template.Name;
                    existing.SubjectTemplate = template.SubjectTemplate;
                    existing.BodyHtmlTemplate = template.BodyHtmlTemplate;
                    existing.Description = template.Description;
                    existing.ModifiedTime = DateTimeOffset.UtcNow;
                    await notifRwRepo.UpdateAsync(existing, CancellationToken.None);
                }
                else
                {
                    if (snowflake != null)
                    {
                        template.Id = await snowflake.GenerateId(CancellationToken.None);
                    }
                    template.CreatedTime = DateTimeOffset.UtcNow;
                    if (template.IsDefault)
                    {
                        await notifRwRepo.SetDefaultAsync(template.Id, template.Code, CancellationToken.None);
                    }
                    await notifRwRepo.AddAsync(template, CancellationToken.None);
                }
            }
            await notifRwRepo.SaveChangesAsync(CancellationToken.None);
            app.Logger.LogInformation("Đã đồng bộ 3 mẫu email chuẩn mặc định vào CSDL thành công.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Không thể tự động đồng bộ mẫu email chuẩn khi khởi động: {Message}", ex.Message);
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Không thể tự động tạo các bảng CSDL mới khi khởi động.");
}

try
{
    app.Logger.LogInformation("Starting web host");
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
    app.Logger.LogCritical(ex, "Host terminated unexpectedly");
}
