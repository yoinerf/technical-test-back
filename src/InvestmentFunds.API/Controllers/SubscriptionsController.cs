using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvestmentFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(
        ISubscriptionService subscriptionService,
        ILogger<SubscriptionsController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeToFundRequest request)
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        _logger.LogInformation(
            "Cliente {CustomerId} suscribiéndose al fondo {FundId}",
            customerId,
            request.FundId
        );

        var result = await _subscriptionService.SubscribeToFundAsync(
            customerId,
            request.FundId,
            request.Amount
        );

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpDelete("{fundId}")]
    public async Task<IActionResult> Unsubscribe(string fundId)
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        _logger.LogInformation(
            "Cliente {CustomerId} cancelando suscripción al fondo {FundId}",
            customerId,
            fundId
        );

        var result = await _subscriptionService.CancelSubscriptionAsync(customerId, fundId);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetSubscriptions()
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        var result = await _subscriptionService.GetSubscriptionsAsync(customerId);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}