using Leaderboard.Api;
using Leaderboard.DAL.DependencyInjection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(configuration => configuration.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddDataAccessLayer(builder.Configuration);

var app = builder.Build();

app.LogListeningUrls();

await app.RunAsync();
