using Wallet.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

//builder.Configuration.LoadConfiguration(builder.Environment);

//builder.Host.UseCustomSerilog(builder.Configuration, "Wallet");

// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services
//    //.AddApplicationServices(builder.Configuration)
//    //.AddInfrastructureServices(builder.Configuration)
//    .AddApiServices(builder.Configuration, builder.Environment);

var app = builder.Build();

//app.UseApiServices();

app.Run();