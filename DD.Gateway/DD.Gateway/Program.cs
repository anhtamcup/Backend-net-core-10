using DD.Gateway.Administration;
using DD.Gateway.Data;
using DD.Gateway.Middleware;
using DD.Gateway.Modules.Ekyc.NamABank;
using DD.Gateway.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(PartnerAccessKeyDefaults.AccessKeyHeader, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = PartnerAccessKeyDefaults.AccessKeyHeader
    });
    options.AddSecurityDefinition(PartnerAccessKeyDefaults.SecretKeyHeader, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = PartnerAccessKeyDefaults.SecretKeyHeader
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(PartnerAccessKeyDefaults.AccessKeyHeader, document)] = [],
        [new OpenApiSecuritySchemeReference(PartnerAccessKeyDefaults.SecretKeyHeader, document)] = []
    });
});
builder.Services.AddDbContext<GatewayDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<NamABankEkycOptions>(
    builder.Configuration.GetSection(NamABankEkycOptions.SectionName));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<NamABankDeepLinkService>();
builder.Services.AddSingleton<NamABankCallbackVerificationService>();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = PartnerAccessKeyDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = PartnerAccessKeyDefaults.AuthenticationScheme;
    })
    .AddScheme<AuthenticationSchemeOptions, PartnerAccessKeyAuthenticationHandler>(
        PartnerAccessKeyDefaults.AuthenticationScheme, _ => { });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder(PartnerAccessKeyDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();

    foreach (var scope in PartnerScopes.Assignable)
    {
        options.AddPolicy(scope, policy =>
        {
            policy.AddAuthenticationSchemes(PartnerAccessKeyDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(PartnerAccessKeyDefaults.ScopeClaim, scope);
        });
    }
});

var app = builder.Build();

if (PartnerProvisioningCommand.CanHandle(args))
{
    Environment.ExitCode = await PartnerProvisioningCommand.RunAsync(app.Services, args);
    return;
}

app.UseMiddleware<RequestLoggingMiddleware>();

// The request log viewer is a developer tool and should remain opt-in.
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/dev/request-logs", () => Results.Redirect("/dev/request-logs.html"))
    .AllowAnonymous();

app.MapControllers();

app.Run();
