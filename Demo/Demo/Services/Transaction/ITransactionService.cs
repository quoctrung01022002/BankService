using Demo.Dtos.Requests;
using Demo.Dtos.Responses;

namespace Demo.Services.Transaction
{
    public interface ITransactionService
    {
        Task<ResponseDto> BuyStock(BuyOrSellStock request);
        Task<ResponseDto> SellStock(BuyOrSellStock request);
    }
}
