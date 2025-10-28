
using BuildingBlocks.Behaviors;
using Media.API;
using Media.Application;
using Media.Infrastructure;
using Media.Application.Extensions;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Media");

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration, builder.Environment)
    .RegisterMapsterConfiguration();

var app = builder.Build();

app.UseApiServices();

app.Run(); 