using MassTransit;
using Itm.Notification.Api.Consumers;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Notification.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddMassTransit(x =>
{
    // Registramos nuestro obrero de eventos (consumidor) para que MassTransit sepa a qué eventos debe "despertar" esta aplicación
    x.AddConsumer<OrderCreatedConsumer>(); // <-- REGISTRAMOS EL CONSUMIDOR
    x.UsingRabbitMq((context, cfg) =>
    {
        // En un trabajo real, esta URL debe venir de configuración segura (KeyVault / env vars)
        cfg.Host("amqps://fmaauije:iFw3ddUlnEz9tuRvv4azKSc2x4Ln2ntR@shark.rmq.cloudamqp.com/fmaauije");

        // Configuramos el nombre de la "fila" donde el obrero va a escuchar
        cfg.ReceiveEndpoint("notificaciones-cola", e =>
        {
            // Le decimos a esta fila que cuando llegue un evento del tipo OrderCreatedEvent, despierte al OrderCreatedConsumer
            e.ConfigureConsumer<OrderCreatedConsumer>(context);
        });
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapGet("/", () => "Notification.Api activa y con soporte de Websockets (Signal R)...");

app.MapHub<NotificationHub>("/hubs/notifications");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();