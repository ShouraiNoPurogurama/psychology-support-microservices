using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.BackgroundServices;
using Promotion.Grpc.Data;
using Promotion.Grpc.Extensions;
using Promotion.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddDbContext<PromotionDbContext>(opts =>
{
    opts.UseSqlite(builder.Configuration.GetConnectionString("PromotionDb"));
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

app.Run();