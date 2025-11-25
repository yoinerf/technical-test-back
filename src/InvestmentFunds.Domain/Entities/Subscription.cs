namespace InvestmentFunds.Domain.Entities;

public class Subscription
{
    public string CustomerId { get; set; } = string.Empty;
    public string FundId { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public decimal SubscribedAmount { get; set; }
    public DateTime SubscribedAt { get; set; }
    public SubscriptionStatus Status { get; set; }
}

public enum SubscriptionStatus
{
    Active,
    Cancelled
}