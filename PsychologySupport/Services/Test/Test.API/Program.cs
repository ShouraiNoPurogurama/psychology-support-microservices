using Test.API;
using Test.Application;
using Test.Infrastructure;
using Test.Infrastructure.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Add services to the container
builder.Services
    .AddApplicationServices(builder.Configuration) //Use case services
    .AddInfrastructureServices(builder.Configuration) //Database and model configuration services
    .AddApiServices(builder.Configuration); //Routing-related services

var app = builder.Build();

//Configure the HTTP request pipeline
app.UseApiServices();

if (app.Environment.IsDevelopment())
{
    app.InitializeDatabaseAsync();
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Test API v1"); });
}


app.Run();