using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReceiptSystem.API.DTOs;
using ReceiptSystem.API.Services;

namespace ReceiptSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Treasury")]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Search receipts with multiple filters
    /// </summary>
    [HttpGet("receipts")]
    public async Task<IActionResult> SearchReceipts([FromQuery] SearchReceiptsFilterDto filters)
    {
        var result = await _searchService.SearchReceiptsAsync(filters);
        return Ok(result);
    }

    /// <summary>
    /// Export search results to Excel file
    /// </summary>
    [HttpGet("receipts/export")]
    public async Task<IActionResult> ExportReceipts([FromQuery] SearchReceiptsFilterDto filters)
    {
        var fileBytes = await _searchService.ExportSearchResultsAsync(filters);
        var fileName = $"search_results_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// Get merchant payment history
    /// </summary>
    [HttpGet("merchants/{id}/payment-history")]
    public async Task<IActionResult> GetMerchantHistory(int id,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _searchService.GetMerchantPaymentHistoryAsync(id, from, to);
        if (result == null) return NotFound(new { message = "التاجر غير موجود" });
        return Ok(result);
    }
}
