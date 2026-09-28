using Leaderboard.Api;
using Leaderboard.Api.Middlewares;
using Leaderboard.Application.DependencyInjection;
using Leaderboard.DAL.DependencyInjection;
using Leaderboard.Domain.Settings;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BusinessRules>(builder.Configuration.GetSection(nameof(BusinessRules)));

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();
builder.Services.AddLocalization(options => options.ResourcesPath = nameof(Leaderboard.Application.Resources));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddSerilog(configuration => configuration.ReadFrom.Configuration(builder.Configuration));

builder.Services.AddDataAccessLayer(builder.Configuration);

var app = builder.Build();

app.UseStatusCodePages();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRequestLogging();

app.UseRouting();
app.MapControllers();
app.UseLocalization();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    await app.Services.MigrateDatabaseAsync();
}

app.LogListeningUrls();

await app.RunAsync();
