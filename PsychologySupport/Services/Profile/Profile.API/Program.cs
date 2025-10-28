using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Profile.API.Common.Services;
using Profile.API.Domains.Pii.Services;
using Profile.API.Domains.Public.PatientProfiles.Services;
using Profile.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

builder.Host.UseStandardSerilog(builder.Configuration, "Profile");

var services = builder.Services;

services.AddApplicationServices(builder.Configuration, builder.Environment);

services.AddExceptionHandler<CustomExceptionHandler>();

services.RegisterMapsterConfiguration();

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseExceptionHandler(options => { });

app.UseStaticFiles();

app.MapCarter();

app.UseSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/profile-service/swagger/v1/swagger.json", "Profile API v1"); });
}

//Map gRPC services
app.MapGrpcService<PatientProfileService>();
app.MapGrpcService<PiiService>();
app.MapGrpcService<PersonaOrchestratorService>();
app.MapGrpcReflectionService(); 

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);

// Default route for non-gRPC clients
app.MapGet("/", () =>
    "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.Run();