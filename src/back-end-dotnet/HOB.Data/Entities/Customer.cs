namespace HOB.Data.Entities;

public class Customer
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
