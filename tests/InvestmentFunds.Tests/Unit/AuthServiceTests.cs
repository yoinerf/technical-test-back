using FluentAssertions;
using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Application.Services;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace InvestmentFunds.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _configurationMock = new Mock<IConfiguration>();

        _configurationMock.Setup(x => x["Jwt:SecretKey"])
            .Returns("test-secret-key-minimum-32-characters-long");
        _configurationMock.Setup(x => x["Jwt:Issuer"])
            .Returns("test-issuer");
        _configurationMock.Setup(x => x["Jwt:Audience"])
            .Returns("test-audience");

        _sut = new AuthService(
            _customerRepositoryMock.Object,
            _configurationMock.Object
        );
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldReturnFailure()
    {
        var request = new RegisterRequest(
            "test@gmail.com",
            "password123",
            "+573509914446",
            NotificationPreference.Email
        );

        var existingCustomer = new Customer { Email = request.Email };

        _customerRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(existingCustomer);

        var result = await _sut.RegisterAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ya está registrado");
    }

    [Fact]
    public async Task RegisterAsync_WhenValidRequest_ShouldReturnSuccess()
    {
        var request = new RegisterRequest(
            "test@gmail.com",
            "password123",
            "+573509914446",
            NotificationPreference.Email
        );

        _customerRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((Customer?)null);

        var result = await _sut.RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be(request.Email);
        result.Value.AccessToken.Should().NotBeNullOrEmpty();

        _customerRepositoryMock.Verify(
            x => x.CreateAsync(It.Is<Customer>(c => 
                c.Email == request.Email && 
                c.Balance == 500000m
            )),
            Times.Once
        );
       
    }

    [Fact]
    public async Task LoginAsync_WhenInvalidCredentials_ShouldReturnFailure()
    {
        var request = new LoginRequest("test@gmail.com", "wrongpassword");

        _customerRepositoryMock
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((Customer?)null);

        var result = await _sut.LoginAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Credenciales inválidas");
    }
}
