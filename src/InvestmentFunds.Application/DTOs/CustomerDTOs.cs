using InvestmentFunds.Domain.Entities;

namespace InvestmentFunds.Application.DTOs;

public record CustomerBalanceResponse(decimal Balance, decimal AvailableBalance);

public record UpdateNotificationPreferenceRequest(NotificationPreference Preference);
