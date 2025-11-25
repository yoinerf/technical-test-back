using InvestmentFunds.Domain.Entities;

namespace InvestmentFunds.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string customerId);
    Task<Customer?> GetByEmailAsync(string email);
    Task CreateAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task<decimal> GetBalanceAsync(string customerId);
    Task UpdateBalanceAsync(string customerId, decimal newBalance);
}