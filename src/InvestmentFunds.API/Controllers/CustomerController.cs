using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvestmentFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(
        ICustomerRepository customerRepository,
        ILogger<CustomerController> logger)
    {
        _customerRepository = customerRepository;
        _logger = logger;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            return NotFound();

        var response = new CustomerBalanceResponse(
            customer.Balance,
            customer.Balance
        );

        return Ok(response);
    }

    [HttpPut("notification-preference")]
    public async Task<IActionResult> UpdateNotificationPreference(
        [FromBody] UpdateNotificationPreferenceRequest request)
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
            return NotFound();

        customer.NotificationPreference = request.Preference;
        await _customerRepository.UpdateAsync(customer);

        return NoContent();
    }
}