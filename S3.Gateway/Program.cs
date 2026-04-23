using Microsoft.EntityFrameworkCore;
using S3.Gateway.Data;
using S3.Gateway.Features.Logs;
using S3.Gateway.Integrations.Base;
using S3.Gateway.Integrations.Napas;
using S3.Gateway.Middleware;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<NapasTokenService>();
builder.Services.AddHttpClient<NapasClient>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };
    });
//.ConfigurePrimaryHttpMessageHandler(sp =>
//{
//    var cert = new X509Certificate2("PaymentSetting/Napas/Key/cfox.pfx", "cfox");
//    var handler = new HttpClientHandler();
//    handler.ClientCertificates.Add(cert);
//    return handler;
//});

builder.Services.Configure<NapasConfig>(
    builder.Configuration.GetSection("NapasConfig"));

builder.Services.AddScoped<BaseApiClient>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddHostedService<LogBackgroundWorker>();

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
