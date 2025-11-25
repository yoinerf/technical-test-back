using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvestmentFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionRepository transactionRepository,
        ILogger<TransactionsController> logger)
    {
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        var transactions = await _transactionRepository.GetByCustomerIdAsync(customerId);
        
        var response = transactions.Select(t => new TransactionResponse(
            t.TransactionId,
            t.FundId,
            t.FundName,
            t.Type.ToString(),
            t.Amount,
            t.Timestamp,
            t.Status.ToString(),
            t.Description
        ));

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(customerId))
            return Unauthorized();

        var transaction = await _transactionRepository.GetByIdAsync(id);
        
        if (transaction == null || transaction.CustomerId != customerId)
            return NotFound(new { error = "Transacción no encontrada" });

        var response = new TransactionResponse(
            transaction.TransactionId,
            transaction.FundId,
            transaction.FundName,
            transaction.Type.ToString(),
            transaction.Amount,
            transaction.Timestamp,
            transaction.Status.ToString(),
            transaction.Description
        );

        return Ok(response);
    }
}