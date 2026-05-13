using Microsoft.Extensions.Options;
using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Endpoints;
using Resilient_AI_Gateway.Logging;
using Resilient_AI_Gateway.Services;
using Scalar.AspNetCore;

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

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ResilienceOptions>>().Value);
builder.Services.AddSingleton<LoggingChannel>();
builder.Services.AddSingleton<IRequestLogger, RequestLogger>();
builder.Services.AddSingleton<IGatewayService, GatewayService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseHttpsRedirection();

app.MapInferenceEndpoints();
app.MapHealthEndpoints();

app.Run();