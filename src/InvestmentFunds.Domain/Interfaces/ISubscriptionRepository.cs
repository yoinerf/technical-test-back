using InvestmentFunds.Domain.Entities;

namespace InvestmentFunds.Domain.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByCustomerAndFundAsync(string customerId, string fundId);
    Task<List<Subscription>> GetByCustomerIdAsync(string customerId);
    Task CreateAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
}