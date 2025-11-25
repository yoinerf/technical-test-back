namespace InvestmentFunds.Domain.Entities;

public class Customer
{
    public string CustomerId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public NotificationPreference NotificationPreference { get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Role { get; set; } = "Customer";
}

public enum NotificationPreference
{
    Email,
    SMS,
    Both
}