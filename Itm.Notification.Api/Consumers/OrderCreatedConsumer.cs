using MassTransit;
using Itm.Shared.Events;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Notification.Api.Hubs;

namespace Itm.Notification.Api.Consumers;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedConsumer> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger, IHubContext<NotificationHub> hubcontext)
    {
        _logger = logger;
        _hubContext = _hubContext;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation("Procesando evento de RabbitMQ para orden: {OrderId}", context.Message.OrderId);

        await Task.Delay(3000);

        var message = $"¡Tu boleta para el producto {context.Message.ProductId} ha sido confirmada!";

        await _hubContext.Clients.All.SendAsync("TicketReady", message);

        _logger.LogInformation("Notificación push enviada via SignalR");
        /*var correlationId = context.CorrelationId?.ToString() ?? "SIN-ID";

        using (_logger.BeginScope(new Dictionary<string,object> { ["CorrelationId"] = correlationId }))
        {
            var data = context.Message;
            _logger.LogInformation("Procesando recibo. Enviando a: {Email}", data.UserEmail);
            await Task.Delay(2000);
            _logger.LogInformation("Correo enviado exitosamente.");
        }*/

        /*var data = context.Message;

        Console.WriteLine("\n =====================================");
        Console.WriteLine("$[Enviando recibo de compra]");
        Console.WriteLine("$Para: {data.UserEmail}");
        Console.WriteLine("$Orden: {data.OrderId}");
        Console.WriteLine("$Monto a Cobrar: ${data.TotalAmount}");
        Console.WriteLine("=====================================\n");

        await Task.Delay(4000);

        Console.WriteLine("$ Correo enviado exitosamente a {data.UserEmail}");*/
    }
}