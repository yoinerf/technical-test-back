using InvestmentFunds.Application.DTOs;
using InvestmentFunds.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestmentFunds.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FundsController : ControllerBase
{
    private readonly IFundRepository _fundRepository;
    private readonly ILogger<FundsController> _logger;

    public FundsController(IFundRepository fundRepository, ILogger<FundsController> logger)
    {
        _fundRepository = fundRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var funds = await _fundRepository.GetAllAsync();
        
        var response = funds.Select(f => new FundResponse(
            f.FundId,
            f.Name,
            f.MinAmount,
            f.Category
        ));

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var fund = await _fundRepository.GetByIdAsync(id);
        
        if (fund == null)
            return NotFound(new { error = "Fondo no encontrado" });

        var response = new FundResponse(
            fund.FundId,
            fund.Name,
            fund.MinAmount,
            fund.Category
        );

        return Ok(response);
    }
}
