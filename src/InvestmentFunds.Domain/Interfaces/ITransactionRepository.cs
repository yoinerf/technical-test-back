using InvestmentFunds.Domain.Entities;


namespace InvestmentFunds.Domain.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(string transactionId);
    Task<List<Transaction>> GetByCustomerIdAsync(string customerId);
    Task CreateAsync(Transaction transaction);
}