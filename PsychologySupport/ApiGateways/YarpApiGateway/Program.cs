using System.Security.Cryptography.X509Certificates;
using YarpApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.ConfigureHttpsDefaults(httpsOptions =>
//     {
//         httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(
//             "/certs/fullchain1.pem",
//             "/certs/privkey1.pem"
//         );
//     });
//
//     serverOptions.ListenAnyIP(80);
//     serverOptions.ListenAnyIP(443, listenOptions =>
//     {
//         listenOptions.UseHttps();
//     });
// });


builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();