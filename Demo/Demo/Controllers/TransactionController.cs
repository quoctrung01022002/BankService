using Demo.Dtos.Requests;
using Demo.Services.Transaction;
using Demo.Utilities.ResponseHelper;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController(ITransactionService transactionService) : Controller
    {
        private ITransactionService _transactionService = transactionService;

        [HttpPost("Buy-Stock")]
        public async Task<IActionResult> BuyStock([FromBody] BuyOrSellStock request)
        {
            var response = await _transactionService.BuyStock(request);
            return ResponseHelper.GetResponse(response);
        }

        [HttpPost("Sell-Stock")]
        public async Task<IActionResult> SellStock([FromBody] BuyOrSellStock request)
        {
            var response = await _transactionService.SellStock(request);
            return ResponseHelper.GetResponse(response);
        }
    }
}
