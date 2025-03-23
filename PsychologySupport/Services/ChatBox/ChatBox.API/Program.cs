using System.Text;
using ChatBox.API.Extensions;
using ChatBox.API.Hubs;
using ChatBox.API.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddApplicationServices(builder.Configuration);
services.AddSignalR();
services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4c7e29927561b2d1129ca95d6d4591565056ae1cbadb1f943b5c7fb06eb59fc0f3f8ea7c9b3cb86a014f81ec1a4207fcef8e5ae59138bbc2317825fa8988f16c")),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();



app.UseCors("CorsPolicy"); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/chatHub");

app.Run();