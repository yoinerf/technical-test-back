using FluentAssertions;
using InvestmentFunds.Application.Services;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using Moq;
using Xunit;

namespace InvestmentFunds.Tests.Unit;

public class SubscriptionServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IFundRepository> _fundRepositoryMock;
    private readonly Mock<ISubscriptionRepository> _subscriptionRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly SubscriptionService _sut;

    public SubscriptionServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _fundRepositoryMock = new Mock<IFundRepository>();
        _subscriptionRepositoryMock = new Mock<ISubscriptionRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _sut = new SubscriptionService(
            _customerRepositoryMock.Object,
            _fundRepositoryMock.Object,
            _subscriptionRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task SubscribeToFundAsync_WhenCustomerNotFound_ShouldReturnFailure()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";
        var amount = 100000m;

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync((Customer?)null);

        var result = await _sut.SubscribeToFundAsync(customerId, fundId, amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cliente no encontrado");
    }

    [Fact]
    public async Task SubscribeToFundAsync_WhenFundNotFound_ShouldReturnFailure()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";
        var amount = 100000m;

        var customer = new Customer
        {
            CustomerId = customerId,
            Balance = 500000m,
            NotificationPreference = NotificationPreference.Email,
            Email = "test@gmail.com"
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync((Fund?)null);

        var result = await _sut.SubscribeToFundAsync(customerId, fundId, amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no existe");
    }

    [Fact]
    public async Task SubscribeToFundAsync_WhenAmountBelowMinimum_ShouldReturnFailure()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";
        var amount = 40000m;

        var customer = new Customer
        {
            CustomerId = customerId,
            Balance = 500000m,
            NotificationPreference = NotificationPreference.Email,
            Email = "test@gmail.com"
        };

        var fund = new Fund
        {
            FundId = fundId,
            Name = "Test Fund",
            MinAmount = 50000m,
            Category = "FPV"
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        var result = await _sut.SubscribeToFundAsync(customerId, fundId, amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("monto mínimo");
    }

    [Fact]
    public async Task SubscribeToFundAsync_WhenInsufficientBalance_ShouldReturnFailure()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";
        var amount = 600000m;

        var customer = new Customer
        {
            CustomerId = customerId,
            Balance = 500000m,
            NotificationPreference = NotificationPreference.Email,
            Email = "test@gmail.com"
        };

        var fund = new Fund
        {
            FundId = fundId,
            Name = "Test Fund",
            MinAmount = 50000m,
            Category = "FPV"
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        var result = await _sut.SubscribeToFundAsync(customerId, fundId, amount);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No tiene saldo disponible");
    }

    [Fact]
    public async Task SubscribeToFundAsync_WhenValidRequest_ShouldReturnSuccess()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";
        var amount = 100000m;

        var customer = new Customer
        {
            CustomerId = customerId,
            Balance = 500000m,
            NotificationPreference = NotificationPreference.Email,
            Email = "test@gmail.com"
        };

        var fund = new Fund
        {
            FundId = fundId,
            Name = "Test Fund",
            MinAmount = 50000m,
            Category = "FPV"
        };

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        _fundRepositoryMock
            .Setup(x => x.GetByIdAsync(fundId))
            .ReturnsAsync(fund);

        _subscriptionRepositoryMock
            .Setup(x => x.GetByCustomerAndFundAsync(customerId, fundId))
            .ReturnsAsync((Subscription?)null);

        var result = await _sut.SubscribeToFundAsync(customerId, fundId, amount);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.FundId.Should().Be(fundId);
        result.Value.FundName.Should().Be(fund.Name);
        result.Value.SubscribedAmount.Should().Be(amount);

        _transactionRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Transaction>()),
            Times.Once
        );

        _subscriptionRepositoryMock.Verify(
            x => x.CreateAsync(It.IsAny<Subscription>()),
            Times.Once
        );

        _customerRepositoryMock.Verify(
            x => x.UpdateBalanceAsync(customerId, 400000m),
            Times.Once
        );

        _notificationServiceMock.Verify(
            x => x.SendSubscriptionNotificationAsync(
                customerId,
                fund.Name,
                amount,
                NotificationPreference.Email
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task CancelSubscriptionAsync_WhenSubscriptionNotFound_ShouldReturnFailure()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";

        _subscriptionRepositoryMock
            .Setup(x => x.GetByCustomerAndFundAsync(customerId, fundId))
            .ReturnsAsync((Subscription?)null);

        var result = await _sut.CancelSubscriptionAsync(customerId, fundId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No se encontró una suscripción activa");
    }

    [Fact]
    public async Task CancelSubscriptionAsync_WhenValidRequest_ShouldReturnSuccess()
    {
        var customerId = "customer-1";
        var fundId = "fund-1";
        var subscribedAmount = 100000m;

        var subscription = new Subscription
        {
            CustomerId = customerId,
            FundId = fundId,
            FundName = "Test Fund",
            SubscribedAmount = subscribedAmount,
            Status = SubscriptionStatus.Active
        };

        var customer = new Customer
        {
            CustomerId = customerId,
            Balance = 400000m,
            NotificationPreference = NotificationPreference.Email,
            Email = "test@gmail.com"
        };

        _subscriptionRepositoryMock
            .Setup(x => x.GetByCustomerAndFundAsync(customerId, fundId))
            .ReturnsAsync(subscription);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId))
            .ReturnsAsync(customer);

        var result = await _sut.CancelSubscriptionAsync(customerId, fundId);

        result.IsSuccess.Should().BeTrue();

        _transactionRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Transaction>(t => t.Type == TransactionType.Cancellation)),
            Times.Once
        );

        _subscriptionRepositoryMock.Verify(
            x => x.UpdateAsync(It.Is<Subscription>(s => s.Status == SubscriptionStatus.Cancelled)),
            Times.Once
        );

        _customerRepositoryMock.Verify(
            x => x.UpdateBalanceAsync(customerId, 500000m),
            Times.Once
        );

        _notificationServiceMock.Verify(
            x => x.SendCancellationNotificationAsync(
                customerId,
                subscription.FundName,
                subscribedAmount,
                NotificationPreference.Email
            ),
            Times.Once
        );
    }
}