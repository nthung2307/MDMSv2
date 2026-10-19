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
foreach (var (key, value) in builder.Configuration.AsEnumerable())
{
    Console.WriteLine($"-\t\t{key}-{value}");
}

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
                (path.StartsWithSegments("/notification/web")))
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
    options.EnableSensitiveDataLogging();
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
    options.EnableSensitiveDataLogging();
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
app.UseHangfireServer();
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
app.UseMiddleware<ExceptionKillerMiddleware>(true);
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
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Không thể tự động tạo bảng MeasurementPointHalfHourOperatingReports khi khởi động.");
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
