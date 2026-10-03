using System.Reflection;
using System.Security;
using System.Text;
using Asp.Versioning;
using Leaderboard.Application.Providers;
using Leaderboard.Domain.Interfaces.Provider;
using Leaderboard.Domain.Interfaces.Service;
using Leaderboard.Domain.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace Leaderboard.Api;

public static class Startup
{
    private const string AppStartupSectionName = "AppStartupSettings";
    private const string AppStartupUrlLogName = "AppStartupUrlLog";
    private const int MinSigningKeyBytes = 32;

    public const string CorsPolicyName = "DefaultCorsPolicy";

    /// <summary>Logs all URLs on which the application is listening when it starts.</summary>
    /// <param name="app">The web application to which the middleware is added.</param>
    public static void LogListeningUrls(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var hosts = app.GetHosts().ToList();

            var appStartupHostLog =
                app.Configuration.GetSection(AppStartupSectionName).GetValue<string>(AppStartupUrlLogName);

            hosts.ForEach(host => Log.Information("{0}{1}", appStartupHostLog, host));
        });
    }


    /// <summary>
    ///     Adds JWT bearer authentication: the access token is signed with <see cref="JwtSettings.SigningKey" />
    ///     and carries the owner id in "sub".
    /// </summary>
    /// <param name="services">The service collection to which authentication services are added.</param>
    /// <param name="configuration">The application configuration with the JwtSettings section.</param>
    public static void AddAuthenticationAndAuthorization(this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>()
                          ?? throw new InvalidOperationException($"{nameof(JwtSettings)} section is missing.");
        if (Encoding.UTF8.GetByteCount(jwtSettings.SigningKey) < MinSigningKeyBytes)
            throw new InvalidOperationException(
                $"{nameof(JwtSettings)}:{nameof(JwtSettings.SigningKey)} must be at least {MinSigningKeyBytes} bytes.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });
        services.AddAuthorization();

        services.AddSingleton<ITokenProvider, JwtTokenProvider>();
    }

    /// <summary>
    ///     Configures Cross-Origin Resource Sharing (CORS) for the application.
    /// </summary>
    /// <param name="services">The service collection to which CORS services are added.</param>
    /// <param name="configuration">The application configuration containing the CORS settings.</param>
    /// <param name="environment">The web hosting environment used to determine development or production configuration.</param>
    public static void AddCors(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration.GetSection(AppStartupSectionName).GetSection("CorsAllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", builder =>
            {
                if (allowedOrigins.Length > 0) builder.WithOrigins(allowedOrigins).AllowCredentials();
                else if (environment.IsDevelopment()) builder.AllowAnyOrigin();
                else
                    throw new SecurityException(
                        "No CORS origins configured. In non-development environment, at least one allowed origin must be specified.");

                builder.AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

    /// <summary>
    ///     Adds Swagger with JWT bearer authorization and XML comments.
    /// </summary>
    /// <param name="services">The service collection to which Swagger services are added.</param>
    public static void AddSwagger(this IServiceCollection services)
    {
        // Routes carry no version, so requests without one must fall back to v1 instead of 400
        services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Leaderboard.Api" });

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please write valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = JwtBearerDefaults.AuthenticationScheme
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        },
                        Name = JwtBearerDefaults.AuthenticationScheme,
                        In = ParameterLocation.Header
                    },
                    []
                }
            });

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
        });
    }

    /// <summary>
    ///     Configures Serilog's per-request logging middleware, escalating the log level based on the response
    ///     status code and any unhandled exception.
    /// </summary>
    /// <param name="app">The web application to which the request logging middleware is added.</param>
    public static void UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, _, ex) => httpContext.Response.StatusCode switch
            {
                _ when ex is not null => LogEventLevel.Error,
                >= 500 => LogEventLevel.Error,
                >= 400 => LogEventLevel.Warning,
                _ => LogEventLevel.Information
            };
        });
    }

    /// <summary>
    ///     Configures the application to use localization with specified supported cultures and default request culture.
    /// </summary>
    /// <param name="app">The web application to which the localization middleware is added.</param>
    public static void UseLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization(options =>
        {
            string[] supportedCultures = ["en", "ru-by"];
            options.SetDefaultCulture(supportedCultures[0]);
            options.AddSupportedCultures(supportedCultures);
            options.AddSupportedUICultures(supportedCultures);
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
    }

    /// <summary>
    ///     Creates the first academic year on an empty database, so scores can be changed right after deploy.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    public static async Task EnsureCurrentAcademicYearAsync(this IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IAcademicYearInitializer>().EnsureCurrentAsync();
    }

    private static IEnumerable<string> GetHosts(this IApplicationBuilder app)
    {
        HashSet<string> hosts = [];

        var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
        serverAddressesFeature?.Addresses.ToList().ForEach(x => hosts.Add(x));

        return hosts;
    }
}