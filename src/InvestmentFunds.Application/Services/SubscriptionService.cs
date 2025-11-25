using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Application.Interfaces;
using InvestmentFunds.Domain.Common;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;

namespace InvestmentFunds.Application.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IFundRepository _fundRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly INotificationService _notificationService;

    public SubscriptionService(
        ICustomerRepository customerRepository,
        IFundRepository fundRepository,
        ISubscriptionRepository subscriptionRepository,
        ITransactionRepository transactionRepository,
        INotificationService notificationService)
    {
        _customerRepository = customerRepository;
        _fundRepository = fundRepository;
        _subscriptionRepository = subscriptionRepository;
        _transactionRepository = transactionRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<SubscriptionResponse>> SubscribeToFundAsync(
        string customerId,
        string fundId,
        decimal amount)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            return Result.Failure<SubscriptionResponse>("Cliente no encontrado");

        var fund = await _fundRepository.GetByIdAsync(fundId);
        if (fund == null)
            return Result.Failure<SubscriptionResponse>($"El fondo con ID {fundId} no existe");

        if (amount < fund.MinAmount)
            return Result.Failure<SubscriptionResponse>(
                $"El monto mínimo para este fondo es {fund.MinAmount:C} COP");

        if (customer.Balance < amount)
            return Result.Failure<SubscriptionResponse>(
                $"No tiene saldo disponible para vincularse al fondo {fund.Name}");

        var existingSubscription = await _subscriptionRepository
            .GetByCustomerAndFundAsync(customerId, fundId);
            
        if (existingSubscription?.Status == SubscriptionStatus.Active)
            return Result.Failure<SubscriptionResponse>("Ya está suscrito a este fondo");

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            CustomerId = customerId,
            FundId = fundId,
            FundName = fund.Name,
            Type = TransactionType.Subscription,
            Amount = amount,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            Description = $"Suscripción al fondo {fund.Name}"
        };

        await _transactionRepository.CreateAsync(transaction);

        var subscription = new Subscription
        {
            CustomerId = customerId,
            FundId = fundId,
            FundName = fund.Name,
            SubscribedAmount = amount,
            SubscribedAt = DateTime.UtcNow,
            Status = SubscriptionStatus.Active
        };

        if (existingSubscription != null)
            await _subscriptionRepository.UpdateAsync(subscription);
        else
            await _subscriptionRepository.CreateAsync(subscription);

        await _customerRepository.UpdateBalanceAsync(customerId, customer.Balance - amount);

        await _notificationService.SendSubscriptionNotificationAsync(
            customerId,
            fund.Name,
            amount,
            customer.NotificationPreference
        );

        return Result.Success(new SubscriptionResponse(
            fundId,
            fund.Name,
            amount,
            subscription.SubscribedAt,
            subscription.Status.ToString()
        ));
    }

    public async Task<Result> CancelSubscriptionAsync(string customerId, string fundId)
    {
        var subscription = await _subscriptionRepository
            .GetByCustomerAndFundAsync(customerId, fundId);
            
        if (subscription == null || subscription.Status != SubscriptionStatus.Active)
            return Result.Failure("No se encontró una suscripción activa para este fondo");

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            return Result.Failure("Cliente no encontrado");

        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid().ToString(),
            CustomerId = customerId,
            FundId = fundId,
            FundName = subscription.FundName,
            Type = TransactionType.Cancellation,
            Amount = subscription.SubscribedAmount,
            Timestamp = DateTime.UtcNow,
            Status = TransactionStatus.Completed,
            Description = $"Cancelación de suscripción al fondo {subscription.FundName}"
        };

        await _transactionRepository.CreateAsync(transaction);

        subscription.Status = SubscriptionStatus.Cancelled;
        await _subscriptionRepository.UpdateAsync(subscription);

        await _customerRepository.UpdateBalanceAsync(
            customerId,
            customer.Balance + subscription.SubscribedAmount
        );

        await _notificationService.SendCancellationNotificationAsync(
            customerId,
            subscription.FundName,
            subscription.SubscribedAmount,
            customer.NotificationPreference
        );

        return Result.Success();
    }

    public async Task<Result<List<SubscriptionResponse>>> GetSubscriptionsAsync(string customerId)
    {
        var subscriptions = await _subscriptionRepository.GetByCustomerIdAsync(customerId);
        
        var activeSubscriptions = subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Select(s => new SubscriptionResponse(
                s.FundId,
                s.FundName,
                s.SubscribedAmount,
                s.SubscribedAt,
                s.Status.ToString()
            ))
            .ToList();

        return Result.Success(activeSubscriptions);
    }
}
