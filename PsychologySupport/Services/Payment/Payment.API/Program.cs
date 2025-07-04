using BuildingBlocks.Behaviors;
using Carter;
using Payment.API;
using Payment.Application;
using Payment.Application.ServiceContracts;
using Payment.Application.Utils;
using Payment.Infrastructure;
using Payment.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseStandardSerilog(builder.Configuration, "Payment");

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddInfrastructureServices(builder.Configuration)
    .AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Apply CORS policy
app.UseCors("CorsPolicy");

app.MapCarter();

app.UseAuthentication();
app.UseAuthorization();


app.Run();