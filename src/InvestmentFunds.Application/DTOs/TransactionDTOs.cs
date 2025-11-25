namespace InvestmentFunds.Application.DTOs;

public record TransactionResponse(
    string TransactionId,
    string FundId,
    string FundName,
    string Type,
    decimal Amount,
    DateTime Timestamp,
    string Status,
    string? Description
);
