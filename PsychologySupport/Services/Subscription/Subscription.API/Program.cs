using BuildingBlocks.Behaviors;
using Carter;
using Subscription.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseStandardSerilog(builder.Configuration, "Subscription");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

// Apply CORS policy
app.UseCors("CorsPolicy");

app.UseStaticFiles();

app.MapCarter();

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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Subscription API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseRouting();

app.Run();