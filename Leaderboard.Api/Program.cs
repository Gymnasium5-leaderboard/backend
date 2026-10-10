using Leaderboard.Api;
using Leaderboard.Api.Middlewares;
using Leaderboard.Application.DependencyInjection;
using Leaderboard.BackgroundJobs.DependencyInjection;
using Leaderboard.Cache.DependencyInjection;
using Leaderboard.Cache.Settings;
using Leaderboard.DAL.DependencyInjection;
using Leaderboard.Domain.Settings;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BusinessRules>(builder.Configuration.GetSection(nameof(BusinessRules)));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection(nameof(RedisSettings)));

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddLocalization(options => options.ResourcesPath = nameof(Leaderboard.Application.Resources));

builder.Services.AddAuthenticationAndAuthorization(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddSerilog(configuration => configuration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddCache();
builder.Services.AddApplication();
builder.Services.AddBackgroundJobs();

builder.Services.AddCors(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseStatusCodePages();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRequestLogging();

app.UseRouting();
app.MapControllers();
app.UseLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(Startup.CorsPolicyName);

app.UseMiddleware<ClaimsValidationMiddleware>();

app.UseSwagger();
if (app.Environment.IsDevelopment()) app.UseSwaggerUI();

// EF Core takes a database lock while migrating, so concurrent instances apply migrations only once
await app.Services.MigrateDatabaseAsync();
await app.Services.EnsureCurrentAcademicYearAsync();

app.LogListeningUrls();

await app.RunAsync();