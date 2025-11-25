using InvestmentFunds.Domain.Entities;

namespace InvestmentFunds.Domain.Interfaces;

public interface INotificationService
{
    Task SendSubscriptionNotificationAsync(
        string customerId,
        string fundName,
        decimal amount,
        NotificationPreference preference
    );
    
    Task SendCancellationNotificationAsync(
        string customerId,
        string fundName,
        decimal refundAmount,
        NotificationPreference preference
    );
}