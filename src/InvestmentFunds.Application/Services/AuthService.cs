using BCrypt.Net;
using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Application.Interfaces;
using InvestmentFunds.Domain.Common;
using InvestmentFunds.Domain.Entities;
using InvestmentFunds.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace InvestmentFunds.Application.Services;

public class AuthService : IAuthService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;
    private const decimal InitialBalance = 500000m;

    public AuthService(
        ICustomerRepository customerRepository,
        IConfiguration configuration)
    {
        _customerRepository = customerRepository;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email);
        if (existingCustomer != null)
            return Result.Failure<AuthResponse>("El correo electrónico ya está registrado");

        var customer = new Customer
        {
            CustomerId = Guid.NewGuid().ToString(),
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            NotificationPreference = request.NotificationPreference,
            Balance = InitialBalance,
            CreatedAt = DateTime.UtcNow,
            Role = "Customer"
        };

        await _customerRepository.CreateAsync(customer);

        var authResponse = await GenerateTokensAsync(customer);
        return Result.Success(authResponse);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var customer = await _customerRepository.GetByEmailAsync(request.Email);
        if (customer == null)
            return Result.Failure<AuthResponse>("Credenciales inválidas");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, customer.PasswordHash))
            return Result.Failure<AuthResponse>("Credenciales inválidas");

        var authResponse = await GenerateTokensAsync(customer);
        return Result.Success(authResponse);
    }


    private async Task<AuthResponse> GenerateTokensAsync(Customer customer)
    {
        var accessToken = GenerateAccessToken(customer);
        var expiresAt = DateTime.UtcNow.AddHours(1);
        
        return new AuthResponse(
            accessToken,
            customer.CustomerId,
            customer.Email,
            expiresAt
        );
    }

    private string GenerateAccessToken(Customer customer)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT key not configured"))
        );
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, customer.CustomerId),
            new Claim(JwtRegisteredClaimNames.Email, customer.Email),
            new Claim(ClaimTypes.Role, customer.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
}
