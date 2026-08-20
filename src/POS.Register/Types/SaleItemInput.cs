namespace POS.Register.Types;

public record SaleItemInput(Guid ItemId, int Quantity, decimal Discount);
