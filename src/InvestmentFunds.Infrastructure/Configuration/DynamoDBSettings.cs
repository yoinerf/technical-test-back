namespace InvestmentFunds.Infrastructure.Configuration;

public class DynamoDBSettings
{
    public string CustomersTable { get; set; } = "InvestmentFunds-Customers";
    public string FundsTable { get; set; } = "InvestmentFunds-Funds";
    public string TransactionsTable { get; set; } = "InvestmentFunds-Transactions";
    public string SubscriptionsTable { get; set; } = "InvestmentFunds-Subscriptions";
    public string RefreshTokensTable { get; set; } = "InvestmentFunds-RefreshTokens";
}
