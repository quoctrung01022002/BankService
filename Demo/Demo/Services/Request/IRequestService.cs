using Demo.Dtos.Requests;
using Demo.Dtos.Responses;

namespace Demo.Services.Request
{
    public interface IRequestService
    {
        Task<ResponseDto<List<Top3BuyOrSellRequestsResponse>>> GetTop3BuyRequests();
        Task<ResponseDto<List<Top3BuyOrSellRequestsResponse>>> GetTop3SellRequests();
        Task<ResponseDto> CreateRequest(CreateRequestRequest request);
    }
}
