using Microsoft.Extensions.DependencyInjection;
using Itm.Gateway.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.AddHealthCheckEndpoint("Inventory API", "http://localhost:5293/health");
    setup.AddHealthCheckEndpoint("Orders API (con CloudAMQP)", "http://localhost:5246/health");
    setup.AddHealthCheckEndpoint("Prices API", "http://localhost:5280/health");
    setup.AddHealthCheckEndpoint("Notifications API", "http://localhost:5277/health");
    setup.AddHealthCheckEndpoint("Product API", "http://localhost:5113/health");

})
    .AddInMemoryStorage();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.MapReverseProxy();

app.MapHealthChecksUI(options => options.UIPath = "/monitor");

app.Run();