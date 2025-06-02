using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.BackgroundServices;
using Promotion.Grpc.Data;
using Promotion.Grpc.Extensions;
using Promotion.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.LoadConfiguration(builder.Environment);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var connectionString = builder.Configuration.GetConnectionString("PromotionDb");

builder.Services.AddDbContext<PromotionDbContext>((sp, opt) =>
{
    opt.UseNpgsql(connectionString);
});
//Background service configurations
builder.Services.AddHostedService<UpdatePromotionStatusesBackgroundService>();

builder.Services.RegisterMapsterConfigurations();

builder.Services.AddScoped<ValidatorService>();

var app = builder.Build();

app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
else
{
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Promotion API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Configure the HTTP request pipeline.
app.MapGrpcService<PromotionService>();
app.UseMigration();
app.MapGrpcReflectionService();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();