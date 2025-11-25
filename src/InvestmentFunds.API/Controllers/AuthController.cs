using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Application.Interfaces;
using InvestmentFunds.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet("checkStatus")]
    public async Task<IActionResult> CheckStatus()
    {
        return Ok("OK - Version 1.0.5");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        _logger.LogInformation("Registro de usuario: {Email}", request.Email);
        var result = await _authService.RegisterAsync(request);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Intento de login: {Email}", request.Email);
        var result = await _authService.LoginAsync(request);
        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });
        return Ok(result.Value);
    }
}