using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Promotion.Grpc.BackgroundServices;
using Promotion.Grpc.Data;
using Promotion.Grpc.Extensions;
using Promotion.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

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

// Configure the HTTP request pipeline.
app.MapGrpcService<PromotionService>();
app.UseMigration();
app.MapGrpcReflectionService();

app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();