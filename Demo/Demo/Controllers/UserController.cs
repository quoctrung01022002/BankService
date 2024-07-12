using Demo.Dtos.Requests;
using Demo.Dtos.Responses;
using Demo.Services.User;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _userService.Validate(request);
            if (response.Success)
            {
                return Ok(response);
            }
            return Unauthorized(response);
        }

        [HttpPost("RenewToken")]
        public async Task<IActionResult> RenewToken(TokenResponse tokenResponse)
        {
            if (tokenResponse == null)
            {
                return BadRequest("Token response cannot be null.");
            }

            var response = await _userService.RenewToken(tokenResponse);
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

    }
}