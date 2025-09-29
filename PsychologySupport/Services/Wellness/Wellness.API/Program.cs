using BuildingBlocks.Behaviors;
using Wellness.API;
using Wellness.Application;
using Wellness.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseCustomSerilog(builder.Configuration, "Wellness");

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration, builder.Environment);
    //.RegisterMapsterConfiguration();

var app = builder.Build();

//app.UseApiServices();

app.Run();