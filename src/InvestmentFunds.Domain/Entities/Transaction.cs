namespace InvestmentFunds.Domain.Entities;

public class Transaction
{
    public string TransactionId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string FundId { get; set; } = string.Empty;
    public string FundName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public TransactionStatus Status { get; set; }
    public string? Description { get; set; }
}

public enum TransactionType
{
    Subscription,
    Cancellation
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed
}