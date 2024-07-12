using Demo.Dtos.Responses;

namespace Demo.Services.Stock
{
    public interface IStockService
    {
        Task<ResponseDto<List<Entities.Stock>>> GetAllStock();
    }
}
