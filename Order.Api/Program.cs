using Order.Api;
using System.Net.Http.Json;
using MassTransit;
using Itm.Shared.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // En un trabajo real, esta URL debe venir de configuración segura (KeyVault / env vars)
        cfg.Host("amqps://fmaauije:iFw3ddUlnEz9tuRvv4azKSc2x4Ln2ntR@shark.rmq.cloudamqp.com/fmaauije");
    });
});

builder.Services.AddHttpClient("InventoryClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5293");
});

builder.Services.AddHealthChecks()
    .AddCheck<CloudAmqpHealthCheck>("CloudAMQP-Broker");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost(
    "/api/orders",
    async (CreateOrderDto order, IPublishEndpoint publisher, HttpContext httpContext, ILogger<Program> logger) =>
    {
        // 1. Extraemos en pasaporte
        var correlationId = httpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? "SIN-ID";

        // 2. Usamos ILogger con BeginScope para sellar TODOS los logs de este bloque
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId } ))
        {
            logger.LogInformation("Iniciando procesamiento de la orden para el producto {ProductId}",order.ProductId);

            var newOrderId = Guid.NewGuid();
            var orderEvent = new OrderCreatedEvent(newOrderId,order.ProductId, "usuario@itm.edu",150000m);
            await publisher.Publish(orderEvent);

            logger.LogInformation("Evento publicado en RabbitMQ. Orden completada.");
            return Results.Ok(new {Status = "Orden procesada", OrderId = newOrderId, CorrelationId = correlationId});
        }

        /*var invClient = factory.CreateClient("InventoryClient");

        // Paso 1: Intentar reservar el stock
        var reduceResponse = await invClient.PostAsJsonAsync("/api/inventory/reduce", order);

        if (!reduceResponse.IsSuccessStatusCode)
        {
            return Results.BadRequest("No se pudo reservar el stock. Transacción abortada.");
        }

        try
        {
            // Paso 2: Procesar el pago (simulado con un random para este ejemplo)
            bool paymentSuccess = new Random().Next(0, 10) > 5;

            if (!paymentSuccess)
            {
                throw new InvalidOperationException("Fondos Insuficientes en la Tarjeta");
            }

            // Supongamos que la venta fue exitosa y ya cobraron.
            var newOrderId = Guid.NewGuid(); // Simulamos el ID generado
            decimal finalTotal = 150000m;    // Simulamos el total de la venta

            // ---------------------------------------------------------
            // EMISIÓN DEL EVENTO (Patrón Fire and Forget)
            // ---------------------------------------------------------
            // Empacamos la caja
            var orderEvent = new OrderCreatedEvent(newOrderId, order.ProductId, "usuario@correo.itm.edu", finalTotal);

            // La tiramos al buzón de RabbitMQ en la nube
            await publisher.Publish(orderEvent);

            Console.WriteLine($"[BROKER] Evento publicado en CloudAMQP para la orden {newOrderId}");

            return Results.Ok(new { Status = "Orden procesada rápido", OrderId = newOrderId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Falló el pago: {ex.Message}. Iniciando compensación de stock...");

            // INCIO DE LA COMPENSACIÓN (SAGA ROLLBACK)
            var compensateResponse = await invClient.PostAsJsonAsync("/api/inventory/release", order);
            if (compensateResponse.IsSuccessStatusCode)
            {
                return Results.Problem("El pago falló. El sttock due devuelto correctamente. Intente de nuevo.");
            }

            Console.WriteLine("[CRITICAL] Falló la compensación. Datos inconsistentes.");
            return Results.Problem("Error crítico del sistema. Contacte soporte.");
        }*/
    });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

internal sealed class CloudAmqpHealthCheck : IHealthCheck
{
    private const string AmqpUrl = "amqps://fmaauije:iFw3ddUlnEz9tuRvv4azKSc2x4Ln2ntR@shark.rmq.cloudamqp.com/fmaauije";

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var uri = new Uri(AmqpUrl);
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(uri.Host, uri.Port > 0 ? uri.Port : 5671, cancellationToken);
            return HealthCheckResult.Healthy("CloudAMQP reachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("CloudAMQP unreachable", ex);
        }
    }
}