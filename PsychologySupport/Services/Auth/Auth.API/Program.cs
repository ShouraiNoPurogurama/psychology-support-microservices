using Auth.API.EventHandlers;
using Auth.API.Extensions;
using BuildingBlocks.Exceptions.Handler;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var services = builder.Services;

//Cross-cutting concerns
services.AddExceptionHandler<CustomExceptionHandler>();

services.AddApplicationServices(builder.Configuration);
services.AddIdentityServices(builder.Configuration);

//RabbitMQ
services.AddMassTransit(x =>
{
    x.AddConsumer<DoctorProfileCreatedEventHandler>();
    x.AddConsumer<DoctorProfileUpdatedEventHandler>();
    x.AddConsumer<PatientProfileCreatedEventHandler>();
    x.AddConsumer<PatientProfileUpdatedEventHandler>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ReceiveEndpoint("doctor-profile-created-queue", e =>
        {
            e.PrefetchCount = 10;
            e.ConfigureConsumer<DoctorProfileCreatedEventHandler>(context);
        });

        cfg.ReceiveEndpoint("doctor-profile-updated-queue", e =>
        {
            e.PrefetchCount = 10;
            e.ConfigureConsumer<DoctorProfileUpdatedEventHandler>(context);
        });

        cfg.ReceiveEndpoint("patient-profile-created-queue", e =>
        {
            e.PrefetchCount = 10;
            e.ConfigureConsumer<PatientProfileCreatedEventHandler>(context);
        });

        cfg.ReceiveEndpoint("patient-profile-updated-queue", e =>
        {
            e.PrefetchCount = 10;
            e.ConfigureConsumer<PatientProfileUpdatedEventHandler>(context);
        });
    });
});

// Configure the HTTP request pipeline
var app = builder.Build();

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();    
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply CORS policy
app.UseCors(config =>
{
    config.AllowAnyHeader();
    config.AllowAnyMethod();
    config.AllowAnyOrigin();
});

app.MapControllers();

app.Run();