using InvestmentFunds.Domain.Entities;

namespace InvestmentFunds.Domain.Interfaces;

public interface IFundRepository
{
    Task<Fund?> GetByIdAsync(string fundId);
    Task<List<Fund>> GetAllAsync();
    Task CreateAsync(Fund fund);
}
