using System.Text.Json;
using Asp.Versioning;
using DotNetEnv;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Endpoints;
using Resilient_AI_Gateway.Logging;
using Resilient_AI_Gateway.Middleware;
using Resilient_AI_Gateway.Services;
using Scalar.AspNetCore;


// Load environment variables
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(envPath))
    Env.Load(envPath);
else
    Env.Load();


// Create a builder for the web application.
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IHuggingFaceClient, HuggingFaceClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<HuggingFaceOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiToken}");
});

builder.Services.Configure<HuggingFaceOptions>(
    builder.Configuration.GetSection(HuggingFaceOptions.SectionName));

builder.Services.Configure<GatewayOptions>(
    builder.Configuration.GetSection(GatewayOptions.SectionName));

var mongoConn = builder.Configuration["MongoDb:ConnectionString"];
if (!string.IsNullOrEmpty(mongoConn))
{
    builder.Services.AddHostedService<MongoRequestLogger>();
    builder.Services.AddSingleton<GatewayHealthCheck>();
    builder.Services.AddHealthChecks()
        .AddCheck<GatewayHealthCheck>("gateway_health");
}
else
{
    builder.Services.AddHealthChecks();
}

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));

builder.Services.Configure<ResilienceOptions>(
    builder.Configuration.GetSection(ResilienceOptions.SectionName));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ResilienceOptions>>().Value);
builder.Services.AddSingleton<LoggingChannel>();
builder.Services.AddSingleton<IRequestLogger, RequestLogger>();
builder.Services.AddSingleton<IGatewayService, GatewayService>();
builder.Services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web));
builder.Services.AddHttpLogging();
builder.Services.AddSingleton<IModelService, ModelService>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Remove("X-Frame-Options");
    context.Response.Headers.Append("Content-Security-Policy",
        "frame-ancestors 'self' https://*.huggingface.co https://huggingface.co");
    await next();
});

app.UseForwardedHeaders();
app.UseHttpLogging();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

//app.UseHttpsRedirection();

// Middleware for API key authentication
app.UseMiddleware<ApiKeyAuthMiddleware>();

// Middleware for request timing
app.UseMiddleware<RequestTimingMiddleware>();

app.MapInferenceEndpoints();
app.MapHealthEndpoints();
app.MapModelEndpoints();

app.MapGet("/", () => Results.Redirect("/scalar/v1"));

app.Run();