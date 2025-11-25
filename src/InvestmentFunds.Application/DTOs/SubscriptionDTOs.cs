namespace InvestmentFunds.Application.DTOs;

public record SubscribeToFundRequest(string FundId, decimal Amount);

public record SubscriptionResponse(
    string FundId,
    string FundName,
    decimal SubscribedAmount,
    DateTime SubscribedAt,
    string Status
);
