using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace Leaderboard.Api;

public static class Startup
{
    private const string AppStartupSectionName = "AppStartupSettings";
    private const string AppStartupUrlLogName = "AppStartupUrlLog";

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
    ///     Adds Swagger with JWT bearer authorization and XML comments.
    /// </summary>
    /// <param name="services">The service collection to which Swagger services are added.</param>
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddApiVersioning()
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

    private static IEnumerable<string> GetHosts(this IApplicationBuilder app)
    {
        HashSet<string> hosts = [];

        var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
        serverAddressesFeature?.Addresses.ToList().ForEach(x => hosts.Add(x));

        return hosts;
    }
}