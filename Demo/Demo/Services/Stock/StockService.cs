using Demo.Dtos.Responses;
using Demo.Entities;

namespace Demo.Services.Stock
{
    public class StockService(TrungTq50demoContext context) : IStockService
    {
        private readonly TrungTq50demoContext _context = context;

        public Task<ResponseDto<List<Entities.Stock>>> GetAllStock()
        {
            var response = new ResponseDto<List<Entities.Stock>>();
            response.StatusCode = 200;
            response.Data = [.. _context.Stocks];
            return Task.FromResult(response);
        }
    }
}
