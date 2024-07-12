using Demo.Dtos.Key;
using Demo.Dtos.Requests;
using Demo.Dtos.Responses;
using Demo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Demo.Services.User
{
    public class UserService : IUserService
    {
        private readonly TrungTq50demoContext _context;
        private readonly AppSettings _appSetting;
        public UserService(TrungTq50demoContext context, IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _context = context;
            _appSetting = optionsMonitor.CurrentValue;
        }

        public async Task<object> GenerateToken(Demo.Entities.User user)
        {
            var jWtTokenHandler = new JwtSecurityTokenHandler();
            var secretkeyBytes = Encoding.UTF8.GetBytes(_appSetting.SecretKey);
            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretkeyBytes), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = jWtTokenHandler.CreateToken(tokenDescription);
            var accessToken = jWtTokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();
            var refreshTokenEntity = new Refreshtoken
            {
                JwtId = token.Id,
                Token = refreshToken,
                IsAccount = false,
                IsRevoked = false,
                IssueAt = DateTime.Now,
                ExpireAt = DateTime.Now.AddHours(1),
                UserId = user.UserId,
            };
            await _context.AddAsync(refreshTokenEntity);
            await _context.SaveChangesAsync();
            return new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.UserId
            };
        }
        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        public async Task<ApiResponse> RenewToken(TokenResponse tokenModels)
        {
            var jWtTokenHandler = new JwtSecurityTokenHandler();
            var secretkeyBytes = Encoding.UTF8.GetBytes(_appSetting.SecretKey);
            var tokenValidateparam = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretkeyBytes),
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false,
            };
            try
            {
                var tokenInverification = jWtTokenHandler.ValidateToken(tokenModels.AccessToken, tokenValidateparam, out var validatedToken);
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Message = "Invalid Token"
                        };
                    }
                }

                var utcExpireDate = long.Parse(tokenInverification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expireDate = ConvertUnitTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Access token has not yet expired"
                    };
                }

                var storedToken = _context.Refreshtokens.FirstOrDefault(x => x.Token == tokenModels.RefreshToken);
                if (storedToken == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token does not exist"
                    };
                }

                if (storedToken.IsAccount)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token has been used"
                    };
                }

                if (storedToken.IsRevoked)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Refresh token has been revoked"
                    };
                }

                var jti = tokenInverification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Token doesn't not match"
                    };
                }

                storedToken.IsAccount = true;
                storedToken.IsRevoked = true;
                _context.Update(storedToken);
                await _context.SaveChangesAsync();

                var user = await _context.Users.SingleOrDefaultAsync(a => a.Username == storedToken.UserId);
                var token = await GenerateToken(user);
                return new ApiResponse
                {
                    Success = true,
                    Message = "Renew token success",
                    Data = token
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse> Validate(LoginRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Username or password cannot be null or empty"
                    };
                }

                var user = await _context.Users.SingleOrDefaultAsync(p => p.Username == request.Username && p.Password == request.Password);
                if (user == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Message = "Invalid username/password"
                    };
                }

                var token = await GenerateToken(user);
                return new ApiResponse
                {
                    Success = true,
                    Message = "Authentication successful",
                    Data = token
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private DateTime ConvertUnitTimeToDateTime(long utcExpireDate)
        {
            DateTime datetimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return datetimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();
        }
    }
}
