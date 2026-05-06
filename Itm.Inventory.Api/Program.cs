using Itm.Inventory.Api.Dtos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//1. Agregar servicios al contenedor
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var JwtSettings = builder.Configuration.GetSection("JwtSettings");
var SecretKey = Encoding.UTF8.GetBytes(JwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = JwtSettings["Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(SecretKey)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

var app = builder.Build();

//2. Configurar el pipeline de la aplicación
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//3. Simulación de base de datos en memoria para los items de inventario
var inventoryDB = new List<InventoryItemDto>
{ new (1,50,"laptop-123"),
    new (2,0,"mouse-456"),
new (3,100,"camera-789")};

//4. Definir los endpoints de la API
//Endpoint para obtener el inventario completo
app.MapGet("/api/inventory/{id}", (int id) =>
{
    //Buscamos en la lista simulada el item de inventario por su ID
    var item = inventoryDB.FirstOrDefault(p => p.ProductId == id);

    //Retornamos 200 OK con el item encontrado o 404 Not Found si no existe
    return item is not null ? Results.Ok(item) : Results.NotFound();
})
.WithName("GetInventory")
.WithOpenApi()
.RequireAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
