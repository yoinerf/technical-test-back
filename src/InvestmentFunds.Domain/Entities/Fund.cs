namespace InvestmentFunds.Domain.Entities;

public class Fund
{
    public string FundId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal MinAmount { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}