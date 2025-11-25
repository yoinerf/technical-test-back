using InvestmentFunds.Domain.Entities;

namespace InvestmentFunds.Application.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string Phone,
    NotificationPreference NotificationPreference
);

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string AccessToken,
    string CustomerId,
    string Email,
    DateTime ExpiresAt
);
