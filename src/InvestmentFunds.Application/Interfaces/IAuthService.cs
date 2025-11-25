using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Domain.Common;

namespace InvestmentFunds.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
}
