using Itm.Price.Api.Dtos;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

//1. Agregar los servicios al contenedor
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

//2. Configurar el pipeline de la aplicación
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//3. Simular las bases de datos
var priceDb = new List<PriceDto>
{
    new (1,999.99m,"USD"),
    new (2,49.99m,"USD"),
    new (3,199.99m,"USD")
};

//4. Definir los endpoints de la API
app.MapGet("/api/prices/{id}", (int id) =>
{
    var price = priceDb.FirstOrDefault(p => p.ProductId == id);
    return price is not null ? Results.Ok(price) : Results.NotFound();
})
.WithName("GetPriceById")
.WithOpenApi();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
