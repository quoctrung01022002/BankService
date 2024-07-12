using Demo.Dtos.Requests;
using Demo.Dtos.Responses;
using System.Security.Principal;

namespace Demo.Services.User
{
    public interface IUserService
    {
        Task<ApiResponse> Validate(LoginRequest request);
        Task<object> GenerateToken(Entities.User user);
        Task<ApiResponse> RenewToken(TokenResponse tokenModels);
    }
}
