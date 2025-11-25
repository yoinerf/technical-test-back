using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Domain.Common;

namespace InvestmentFunds.Application.Interfaces;

public interface ISubscriptionService
{
    Task<Result<SubscriptionResponse>> SubscribeToFundAsync(
        string customerId,
        string fundId,
        decimal amount
    );
    
    Task<Result> CancelSubscriptionAsync(string customerId, string fundId);
    
    Task<Result<List<SubscriptionResponse>>> GetSubscriptionsAsync(string customerId);
}
