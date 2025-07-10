using BuildingBlocks.Behaviors;
using Test.API;
using Test.Application;
using Test.Application.Extensions;
using Test.Infrastructure;
using Test.Infrastructure.Data.Extensions;


var builder = WebApplication.CreateBuilder(args);

// Load configuration settings
builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseStandardSerilog(builder.Configuration, "Test Service");

//Add services to the container
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration, builder.Environment)
    .RegisterMapsterConfiguration();

var app = builder.Build();

//Configure the HTTP request pipeline
app.UseApiServices();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = string.Empty;
        c.SwaggerEndpoint("/test-service/swagger/v1/swagger.json", "Test API v1");
    });
}

// Apply CORS policy
app.UseCors("CorsPolicy");

app.Run();