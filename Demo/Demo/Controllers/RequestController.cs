using Demo.Dtos.Requests;
using Demo.Services.Request;
using Demo.Utilities.ResponseHelper;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController(IRequestService requestService) : Controller
    {
        private readonly IRequestService _requestService = requestService;

        [HttpGet("Top-3-Buy")]
        public async Task<IActionResult> GetTop3Buy()
        {
            var response = await _requestService.GetTop3BuyRequests();
            return ResponseHelper.GetResponse(response);
        }

        [HttpGet("Top-3-Sell")]
        public async Task<IActionResult> GetTop3Sell()
        {
            var response = await _requestService.GetTop3SellRequests();
            return ResponseHelper.GetResponse(response);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateRequestRequest request)
        {
            var response = await _requestService.CreateRequest(request);
            return ResponseHelper.GetResponse(response);
        }
    }
}
