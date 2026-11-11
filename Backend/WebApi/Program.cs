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
builder.Services.AddSingleton<Application.Services.ITagRoutingCacheService, Infrastructure.Services.TagRoutingCacheService>();
builder.Services.AddSingleton<Infrastructure.Services.BufferedTelemetryIngestionService>();
builder.Services.AddSingleton<Application.Services.IBufferedTelemetryIngestionService>(sp =>
    sp.GetRequiredService<Infrastructure.Services.BufferedTelemetryIngestionService>());
builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<Infrastructure.Services.BufferedTelemetryIngestionService>());
builder.Services.AddScoped<Application.Services.IPartitionManagementService, Infrastructure.Services.PartitionManagementService>();
builder.Services.AddScoped<Application.Services.IEmailProviderFactory, Infrastructure.Services.EmailProviders.EmailProviderFactory>();
builder.Services.AddScoped<Application.Services.INotificationTemplateService, Infrastructure.Services.Templates.NotificationTemplateService>();
builder.Services.AddScoped<Application.Services.IStatementPdfGenerator, Infrastructure.Services.Templates.StatementPdfGenerator>();
builder.Services.AddScoped<Application.Services.IMeterReplacementService, Infrastructure.Services.MeterReplacementService>();
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
RecurringJob.RemoveIfExists("GroupDataPerMeter");
RecurringJob.RemoveIfExists("ArchiveDataMeasurementPointMonitoring");
RecurringJob.AddOrUpdate<BackgroundJobService>(
    "MaintainTimeSeriesPartitions",
    job => job.MaintainTimeSeriesPartitions(CancellationToken.None),
    "0 1 * * *",
    new RecurringJobOptions()
    {
        TimeZone = TimeZoneInfo.Local
    }
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
  `DienApPhaB` double DEFAULT NULL,
  `DienApPhaC` double DEFAULT NULL,
  `DongDienPhaA` double DEFAULT NULL,
  `DongDienPhaB` double DEFAULT NULL,
  `DongDienPhaC` double DEFAULT NULL,
  `CongSuatTacDungTong3Pha` double DEFAULT NULL,
  `CongSuatPhanKhangTong3Pha` double DEFAULT NULL,
  `HeSoCongSuatPF` double DEFAULT NULL,
  `TanSoHeThong` double DEFAULT NULL,
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
        "CREATE INDEX `IX_MPHalfHourEnergy_PointId_CreatedTime` ON `MeasurementPointHalfHourEnergyReports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPHalfHourEnergy_PointId_CreatedTime` ON `measurementpointhalfhourenergyreports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPDailyEnergy_PointId_CreatedTime` ON `MeasurementPointDailyEnergyReports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPDailyEnergy_PointId_CreatedTime` ON `measurementpointdailyenergyreports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPHalfHourOperating_PointId_CreatedTime` ON `MeasurementPointHalfHourOperatingReports` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPEvents_PointId_CreatedTime` ON `MeasurementPointEvents` (`MeasurementPointId`, `CreatedTime`);",
        "CREATE INDEX `IX_MPAlerts_PointId_CreatedTime` ON `MeasurementPointAlerts` (`MeasurementPointId`, `CreatedTime`);",
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

    // Ensure Phase 1 Wide-Table Snapshot, Partitioned TimeSeries & Schema Additions exist on clean startups
    var phase1StartupDdlSqls = new[]
    {
        @"CREATE TABLE IF NOT EXISTS `MeasurementPointCurrentSnapshots` (
            `Id` bigint NOT NULL,
            `MeasurementPointId` bigint NOT NULL,
            `LastTelemetryAt` datetime(6) DEFAULT NULL,
            `LastStatusAt` datetime(6) DEFAULT NULL,
            `LastDateTimeAt` datetime(6) DEFAULT NULL,
            `UpdatedAt` datetime(6) NOT NULL,
            `DongDienPhaA` decimal(18,4) DEFAULT NULL, `DongDienPhaAUnit` varchar(32) DEFAULT NULL, `DongDienPhaAQuality` tinyint(1) DEFAULT NULL, `DongDienPhaAUpdatedAt` datetime(6) DEFAULT NULL,
            `DongDienPhaB` decimal(18,4) DEFAULT NULL, `DongDienPhaBUnit` varchar(32) DEFAULT NULL, `DongDienPhaBQuality` tinyint(1) DEFAULT NULL, `DongDienPhaBUpdatedAt` datetime(6) DEFAULT NULL,
            `DongDienPhaC` decimal(18,4) DEFAULT NULL, `DongDienPhaCUnit` varchar(32) DEFAULT NULL, `DongDienPhaCQuality` tinyint(1) DEFAULT NULL, `DongDienPhaCUpdatedAt` datetime(6) DEFAULT NULL,
            `DienApPhaA` decimal(18,4) DEFAULT NULL, `DienApPhaAUnit` varchar(32) DEFAULT NULL, `DienApPhaAQuality` tinyint(1) DEFAULT NULL, `DienApPhaAUpdatedAt` datetime(6) DEFAULT NULL,
            `DienApPhaB` decimal(18,4) DEFAULT NULL, `DienApPhaBUnit` varchar(32) DEFAULT NULL, `DienApPhaBQuality` tinyint(1) DEFAULT NULL, `DienApPhaBUpdatedAt` datetime(6) DEFAULT NULL,
            `DienApPhaC` decimal(18,4) DEFAULT NULL, `DienApPhaCUnit` varchar(32) DEFAULT NULL, `DienApPhaCQuality` tinyint(1) DEFAULT NULL, `DienApPhaCUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatTacDungPhaA` decimal(18,4) DEFAULT NULL, `CongSuatTacDungPhaAUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungPhaAQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungPhaAUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatTacDungPhaB` decimal(18,4) DEFAULT NULL, `CongSuatTacDungPhaBUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungPhaBQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungPhaBUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatTacDungPhaC` decimal(18,4) DEFAULT NULL, `CongSuatTacDungPhaCUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungPhaCQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungPhaCUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatTacDungTong3Pha` decimal(18,4) DEFAULT NULL, `CongSuatTacDungTong3PhaUnit` varchar(32) DEFAULT NULL, `CongSuatTacDungTong3PhaQuality` tinyint(1) DEFAULT NULL, `CongSuatTacDungTong3PhaUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatPhanKhangPhaA` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangPhaAUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangPhaAQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangPhaAUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatPhanKhangPhaB` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangPhaBUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangPhaBQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangPhaBUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatPhanKhangPhaC` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangPhaCUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangPhaCQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangPhaCUpdatedAt` datetime(6) DEFAULT NULL,
            `CongSuatPhanKhangTong3Pha` decimal(18,4) DEFAULT NULL, `CongSuatPhanKhangTong3PhaUnit` varchar(32) DEFAULT NULL, `CongSuatPhanKhangTong3PhaQuality` tinyint(1) DEFAULT NULL, `CongSuatPhanKhangTong3PhaUpdatedAt` datetime(6) DEFAULT NULL,
            `HeSoCongSuatPFPhaA` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFPhaAQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFPhaAUpdatedAt` datetime(6) DEFAULT NULL,
            `HeSoCongSuatPFPhaB` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFPhaBQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFPhaBUpdatedAt` datetime(6) DEFAULT NULL,
            `HeSoCongSuatPFPhaC` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFPhaCQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFPhaCUpdatedAt` datetime(6) DEFAULT NULL,
            `HeSoCongSuatPFTrungBinh3Pha` decimal(18,4) DEFAULT NULL, `HeSoCongSuatPFTrungBinh3PhaQuality` tinyint(1) DEFAULT NULL, `HeSoCongSuatPFTrungBinh3PhaUpdatedAt` datetime(6) DEFAULT NULL,
            `TanSoHeThong` decimal(18,4) DEFAULT NULL, `TanSoHeThongUnit` varchar(32) DEFAULT NULL, `TanSoHeThongQuality` tinyint(1) DEFAULT NULL, `TanSoHeThongUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioBT` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioCD` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioTD` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPTong` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangPhanKhangAQTong` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPGioBT` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioBTUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioBTQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioBTUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPGioCD` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioCDUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioCDQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioCDUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPGioTD` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioTDUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioTDQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioTDUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPTong` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPTongUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPTongQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPTongUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAQTong` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAQTongUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAQTongQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAQTongUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPGioBTChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioBTChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioBTChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPGioCDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioCDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioCDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPGioTDChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPGioTDChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPGioTDChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAPTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAPTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAPTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAPTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `ChiSoChotDienNangAQTongChieuNhan` decimal(18,4) DEFAULT NULL, `ChiSoChotDienNangAQTongChieuNhanUnit` varchar(32) DEFAULT NULL, `ChiSoChotDienNangAQTongChieuNhanQuality` tinyint(1) DEFAULT NULL, `ChiSoChotDienNangAQTongChieuNhanUpdatedAt` datetime(6) DEFAULT NULL,
            `PmaxKyHienTai` decimal(18,4) DEFAULT NULL, `PmaxKyHienTaiUnit` varchar(32) DEFAULT NULL, `PmaxKyHienTaiQuality` tinyint(1) DEFAULT NULL, `PmaxKyHienTaiUpdatedAt` datetime(6) DEFAULT NULL,
            `PmaxKyTruoc` decimal(18,4) DEFAULT NULL, `PmaxKyTruocUnit` varchar(32) DEFAULT NULL, `PmaxKyTruocQuality` tinyint(1) DEFAULT NULL, `PmaxKyTruocUpdatedAt` datetime(6) DEFAULT NULL,
            `MatKetNoi` tinyint(1) DEFAULT NULL, `MatKetNoiQuality` tinyint(1) DEFAULT NULL, `MatKetNoiUpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao01` tinyint(1) DEFAULT NULL, `CanhBao01Quality` tinyint(1) DEFAULT NULL, `CanhBao01UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao02` tinyint(1) DEFAULT NULL, `CanhBao02Quality` tinyint(1) DEFAULT NULL, `CanhBao02UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao03` tinyint(1) DEFAULT NULL, `CanhBao03Quality` tinyint(1) DEFAULT NULL, `CanhBao03UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao04` tinyint(1) DEFAULT NULL, `CanhBao04Quality` tinyint(1) DEFAULT NULL, `CanhBao04UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao05` tinyint(1) DEFAULT NULL, `CanhBao05Quality` tinyint(1) DEFAULT NULL, `CanhBao05UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao06` tinyint(1) DEFAULT NULL, `CanhBao06Quality` tinyint(1) DEFAULT NULL, `CanhBao06UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao07` tinyint(1) DEFAULT NULL, `CanhBao07Quality` tinyint(1) DEFAULT NULL, `CanhBao07UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao08` tinyint(1) DEFAULT NULL, `CanhBao08Quality` tinyint(1) DEFAULT NULL, `CanhBao08UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao09` tinyint(1) DEFAULT NULL, `CanhBao09Quality` tinyint(1) DEFAULT NULL, `CanhBao09UpdatedAt` datetime(6) DEFAULT NULL,
            `CanhBao10` tinyint(1) DEFAULT NULL, `CanhBao10Quality` tinyint(1) DEFAULT NULL, `CanhBao10UpdatedAt` datetime(6) DEFAULT NULL,
            `MeterYear` smallint DEFAULT NULL, `MeterYearQuality` tinyint(1) DEFAULT NULL,
            `MeterMonth` smallint DEFAULT NULL, `MeterMonthQuality` tinyint(1) DEFAULT NULL,
            `MeterDay` smallint DEFAULT NULL, `MeterDayQuality` tinyint(1) DEFAULT NULL,
            `MeterHour` smallint DEFAULT NULL, `MeterHourQuality` tinyint(1) DEFAULT NULL,
            `MeterMinute` smallint DEFAULT NULL, `MeterMinuteQuality` tinyint(1) DEFAULT NULL,
            `MeterSecond` smallint DEFAULT NULL, `MeterSecondQuality` tinyint(1) DEFAULT NULL,
            `ClosingYear` smallint DEFAULT NULL, `ClosingYearQuality` tinyint(1) DEFAULT NULL,
            `ClosingMonth` smallint DEFAULT NULL, `ClosingMonthQuality` tinyint(1) DEFAULT NULL,
            `ClosingDay` smallint DEFAULT NULL, `ClosingDayQuality` tinyint(1) DEFAULT NULL,
            `ClosingHour` smallint DEFAULT NULL, `ClosingHourQuality` tinyint(1) DEFAULT NULL,
            `ClosingMinute` smallint DEFAULT NULL, `ClosingMinuteQuality` tinyint(1) DEFAULT NULL,
            `ClosingSecond` smallint DEFAULT NULL, `ClosingSecondQuality` tinyint(1) DEFAULT NULL,
            `PmaxCurrentYear` smallint DEFAULT NULL, `PmaxCurrentYearQuality` tinyint(1) DEFAULT NULL,
            `PmaxCurrentMonth` smallint DEFAULT NULL, `PmaxCurrentMonthQuality` tinyint(1) DEFAULT NULL,
            `PmaxCurrentDay` smallint DEFAULT NULL, `PmaxCurrentDayQuality` tinyint(1) DEFAULT NULL,
            `PmaxCurrentHour` smallint DEFAULT NULL, `PmaxCurrentHourQuality` tinyint(1) DEFAULT NULL,
            `PmaxCurrentMinute` smallint DEFAULT NULL, `PmaxCurrentMinuteQuality` tinyint(1) DEFAULT NULL,
            `PmaxCurrentSecond` smallint DEFAULT NULL, `PmaxCurrentSecondQuality` tinyint(1) DEFAULT NULL,
            `PmaxPrevYear` smallint DEFAULT NULL, `PmaxPrevYearQuality` tinyint(1) DEFAULT NULL,
            `PmaxPrevMonth` smallint DEFAULT NULL, `PmaxPrevMonthQuality` tinyint(1) DEFAULT NULL,
            `PmaxPrevDay` smallint DEFAULT NULL, `PmaxPrevDayQuality` tinyint(1) DEFAULT NULL,
            `PmaxPrevHour` smallint DEFAULT NULL, `PmaxPrevHourQuality` tinyint(1) DEFAULT NULL,
            `PmaxPrevMinute` smallint DEFAULT NULL, `PmaxPrevMinuteQuality` tinyint(1) DEFAULT NULL,
            `PmaxPrevSecond` smallint DEFAULT NULL, `PmaxPrevSecondQuality` tinyint(1) DEFAULT NULL,
            PRIMARY KEY (`Id`),
            UNIQUE KEY `IX_MeasurementPointCurrentSnapshots_MeasurementPointId` (`MeasurementPointId`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;",
        @"CREATE TABLE IF NOT EXISTS `MeasurementPointTimeSeries` (
            `Id` bigint NOT NULL,
            `MeasurementPointId` bigint NOT NULL,
            `PartitionYm` int NOT NULL,
            `RecordedAt` datetime(6) NOT NULL,
            `IngestedAt` datetime(6) NOT NULL,
            `QualityFlag` tinyint(1) NOT NULL DEFAULT '1',
            `SourcePacketType` tinyint unsigned NOT NULL DEFAULT '1',
            `DongDienPhaA` decimal(18,4) DEFAULT NULL,
            `DongDienPhaB` decimal(18,4) DEFAULT NULL,
            `DongDienPhaC` decimal(18,4) DEFAULT NULL,
            `DienApPhaA` decimal(18,4) DEFAULT NULL,
            `DienApPhaB` decimal(18,4) DEFAULT NULL,
            `DienApPhaC` decimal(18,4) DEFAULT NULL,
            `CongSuatTacDungPhaA` decimal(18,4) DEFAULT NULL,
            `CongSuatTacDungPhaB` decimal(18,4) DEFAULT NULL,
            `CongSuatTacDungPhaC` decimal(18,4) DEFAULT NULL,
            `CongSuatTacDungTong3Pha` decimal(18,4) DEFAULT NULL,
            `CongSuatPhanKhangPhaA` decimal(18,4) DEFAULT NULL,
            `CongSuatPhanKhangPhaB` decimal(18,4) DEFAULT NULL,
            `CongSuatPhanKhangPhaC` decimal(18,4) DEFAULT NULL,
            `CongSuatPhanKhangTong3Pha` decimal(18,4) DEFAULT NULL,
            `HeSoCongSuatPFPhaA` decimal(18,4) DEFAULT NULL,
            `HeSoCongSuatPFPhaB` decimal(18,4) DEFAULT NULL,
            `HeSoCongSuatPFPhaC` decimal(18,4) DEFAULT NULL,
            `HeSoCongSuatPFTrungBinh3Pha` decimal(18,4) DEFAULT NULL,
            `TanSoHeThong` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioBT` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioCD` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioTD` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPTong` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangPhanKhangAQTong` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangTacDungAPTongChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPGioBT` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPGioCD` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPGioTD` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPTong` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAQTong` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPGioBTChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPGioCDChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPGioTDChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAPTongChieuNhan` decimal(18,4) DEFAULT NULL,
            `ChiSoChotDienNangAQTongChieuNhan` decimal(18,4) DEFAULT NULL,
            `PmaxKyHienTai` decimal(18,4) DEFAULT NULL,
            `PmaxKyTruoc` decimal(18,4) DEFAULT NULL,
            PRIMARY KEY (`MeasurementPointId`, `PartitionYm`, `RecordedAt`, `Id`),
            KEY `IX_MeasurementPointTimeSeries_MeasurementPointId_RecordedAt` (`MeasurementPointId`, `RecordedAt`),
            KEY `IX_MeasurementPointTimeSeries_RecordedAt` (`RecordedAt`)
        ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4
        PARTITION BY RANGE (`PartitionYm`) (
            PARTITION `p_max` VALUES LESS THAN MAXVALUE
        );",
        "ALTER TABLE `MeasurementPointEvents` ADD COLUMN `ParameterCode` varchar(64) DEFAULT NULL;",
        "ALTER TABLE `MeasurementPointEvents` MODIFY COLUMN `TagNameId` bigint DEFAULT NULL;",
        "ALTER TABLE `MeasurementPointAlerts` ADD COLUMN `ParameterCode` varchar(64) DEFAULT NULL;",
        "ALTER TABLE `MeasurementPointAlerts` MODIFY COLUMN `TagNameId` bigint DEFAULT NULL;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `DienApPhaAUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `DienApPhaBUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `DienApPhaCUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `DongDienPhaAUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `DongDienPhaBUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `DongDienPhaCUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `CongSuatTacDungTong3PhaUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `CongSuatPhanKhangTong3PhaUnit`;",
        "ALTER TABLE `MeasurementPointHalfHourOperatingReports` DROP COLUMN `TanSoHeThongUnit`;"
    };
    foreach (var sql in phase1StartupDdlSqls)
    {
        try { await db.Database.ExecuteSqlRawAsync(sql); } catch { }
    }
    try
    {
        using var partitionScope = app.Services.CreateScope();
        var partitionSvc = partitionScope.ServiceProvider.GetService<Application.Services.IPartitionManagementService>();
        if (partitionSvc != null)
        {
            await partitionSvc.EnsureAndCleanupPartitionsAsync(monthsAhead: 3, retentionMonths: 12);
        }
    }
    catch { }
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
            var removedDupes = await notifRwRepo.DeduplicateTemplatesAsync(CancellationToken.None);
            if (removedDupes > 0)
            {
                app.Logger.LogInformation("Đã tự động dọn {Count} bản ghi mẫu email bị trùng lặp trước khi đồng bộ.", removedDupes);
            }

            var prebuilts = Infrastructure.Services.Templates.NotificationTemplateService.GetPrebuiltDefaultTemplates();
            foreach (var template in prebuilts)
            {
                var existing = await notifRwRepo.GetAnyByCodeAsync(template.Code, CancellationToken.None);
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
