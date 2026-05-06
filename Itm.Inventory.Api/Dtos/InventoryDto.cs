namespace Itm.Inventory.Api.Dtos
{
    //Usamos record para inmutabilidad y concisión en la definición de la clase DTO
    public record InventoryItemDto(int ProductId, int Stock, string Sku);

    //1. public record
    //2. Parámetros
}

