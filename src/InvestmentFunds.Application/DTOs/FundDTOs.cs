namespace InvestmentFunds.Application.DTOs;

public record FundResponse(
    string FundId,
    string Name,
    decimal MinAmount,
    string Category
);
