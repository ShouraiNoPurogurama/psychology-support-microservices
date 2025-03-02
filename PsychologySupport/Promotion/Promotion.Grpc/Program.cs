using Microsoft.EntityFrameworkCore;
using Promotion.Grpc.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddDbContext<PromotionDbContext>(opts =>
{
    opts.UseSqlite(builder.Configuration.GetConnectionString("PromotionDb"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigration();
app.MapGrpcReflectionService();

app.Run();