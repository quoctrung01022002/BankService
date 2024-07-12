using Demo.Services.Stock;
using Demo.Utilities.ResponseHelper;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController(IStockService stockService) : Controller
    {
        private readonly IStockService _stockService = stockService;

        [HttpGet("Get-All")]
        public async Task<IActionResult> GetAll()
        {
            var response = await _stockService.GetAllStock();
            return ResponseHelper.GetResponse(response);
        }
    }
}
