using BuildingBlocks.Exceptions.Handler;
using Carter;
using Profile.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

var services = builder.Services;

services.AddApplicationServices(builder.Configuration);

services.AddExceptionHandler<CustomExceptionHandler>();

services.RegisterMapsterConfiguration();

// Configure the HTTP request pipeline
var app = builder.Build();


app.UseExceptionHandler(options => { });

app.UseStaticFiles();

app.MapCarter();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.RoutePrefix = string.Empty;
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Profile API v1");
});

app.UseCors("CorsPolicy");

app.Run();