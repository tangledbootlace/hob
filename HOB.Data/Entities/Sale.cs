namespace HOB.Data.Entities;

public class Sale
{
    public Guid SaleId { get; set; }
    public Guid OrderId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public virtual Order Order { get; set; } = null!;
}
