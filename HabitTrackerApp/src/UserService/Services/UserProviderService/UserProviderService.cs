using Azure.Core;
using Consul;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Common;
using UserService.Common.Enum;
using UserService.Models;
using UserService.Repositories.UOW;
using UserService.Services.Redis;

namespace UserService.Services.UserProviderService
{
    public class UserProviderService : IUserProviderService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        private readonly GoogleAuthSettings _authSettings;
        private readonly JwtSettings _jwtSettings;

        public UserProviderService(IUnitOfWork unitOfWork,IRedisService redisService, IOptions<GoogleAuthSettings> authSettings,IOptions<JwtSettings> jwtOptions)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisService;
            _authSettings = authSettings.Value;
            _jwtSettings = jwtOptions.Value;
        }


        public async Task<ServiceResult> GetLinkedProvidersAsync(Guid userId)
        {
            var providers = (await _unitOfWork.UserProvider.FindAnsyc(u=>u.UserId == userId));
            return new ServiceResult(true, "Linked providers", providers);
        }

       
        public async Task<ServiceResult> LinkExternalProviderAsync(Guid userId, string provider, string providerUserId)
        {
            var exists = (await _unitOfWork.UserProvider.FindAnsyc(p => p.UserId == userId && p.Provider == provider)).Any();
            if (exists)
                return new ServiceResult(false, "Provider already linked");

            var link = new UserProvider
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = provider,
                ProviderUserId = providerUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserProvider.AddAnsync(link);
            await _unitOfWork.CommitAsync();

            return new ServiceResult(true, "Linked external provider", link);
        }

        public async Task<ServiceResult> SearchUsersAsync(string? keyword, int page, int pageSize)
        {
            var query = _unitOfWork.user.Query();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.Username.Contains(keyword) || u.Email.Contains(keyword));
            }

            var total = query.Count();
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new ServiceResult(true,"User search result", new { Total = total, Data = data });
        }

        private async Task<object> GenerateJwtForUser(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            var refreshToken = Guid.NewGuid().ToString();

            var redisKey = $"auth:token:{user.Id}";
            var refreshKey = $"auth:refresh:{user.Id}";
            await _redisService.SetAsync(redisKey, tokenString, TimeSpan.FromMinutes(_jwtSettings.ExpiresInMinutes));
            await _redisService.SetAsync(refreshKey, refreshToken, TimeSpan.FromDays(7));

            return new
            {
                RefreshToken = refreshToken,
                IDUser = user.Id,
                Token = tokenString,
                ExpiresIn = _jwtSettings.ExpiresInMinutes * 60
            };
        }

        public async Task<ServiceResult> SocialLoginAsync(string provider, string accessToken, string clientType)
        {
            string clientId;
            if (clientType == "web" || clientType == "mobile")
            {
                clientId = _authSettings.WebClientId; // Luôn Web Client ID!
            }
            else
            {
                return new ServiceResult(false, "Invalid client type");
            }
            var payload = await GoogleJsonWebSignature.ValidateAsync(accessToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            var existingUserProvider = (await _unitOfWork.UserProvider.FindAnsyc(
                up => up.Provider == provider && up.ProviderUserId == payload.Subject
            )).FirstOrDefault();

            if (existingUserProvider != null)
            {
                var user = (await _unitOfWork.user.FindAnsyc(u => u.Id == existingUserProvider.UserId)).FirstOrDefault();
                if (user == null)
                    return new ServiceResult(false, "User data inconsistency");

                var tokenResult = await GenerateJwtForUser(user);
                return new ServiceResult(true, "Social login success", tokenResult);
            }
            else
            {
                var role = (await _unitOfWork.role.FindAnsyc(c => c.Name.Equals("user"))).FirstOrDefault();
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Email = payload.Email,
                    Username = payload.Email.Split('@')[0],
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Status = UserStatus.Active.ToString(),
                    EmailVerified = true,
                    Roles = {role}
                };

                await _unitOfWork.user.AddAnsync(newUser);

                var userProvider = new UserProvider
                {
                    Id = Guid.NewGuid(),
                    UserId = newUser.Id,
                    Provider = provider,
                    ProviderUserId = payload.Subject,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.UserProvider.AddAnsync(userProvider);
                await _unitOfWork.CommitAsync();

                var newToken = await GenerateJwtForUser(newUser);
                var expireMinutes = Math.Max(1, _jwtSettings.ExpiresInMinutes);
                var refreshToken = Guid.NewGuid().ToString();

                var accessKey = $"auth:token:{userProvider.UserId}";
                var refreshKey = $"auth:refresh:{userProvider.UserId}";

                await _redisService.SetAsync(accessKey, accessToken, TimeSpan.FromMinutes(expireMinutes));
                await _redisService.SetAsync(refreshKey, refreshToken, TimeSpan.FromDays(7));


                return new ServiceResult(true, "Social login success (new user)", new
                {
                    AccessToken = newToken,
                    RefreshToken = refreshToken,
                    IDUser = userProvider.UserId,
                    ExpiresIn = expireMinutes * 60
                });

            }
        }

        public async Task<ServiceResult> StartMfaAsync(Guid userId)
        {
            var code = new Random().Next(100000, 999999).ToString();
            var key = $"mfa:{userId}";
            await _redisService.SetAsync(key, code, TimeSpan.FromMinutes(5));
            return new ServiceResult(true, "OTP code sent (mocked)", code);
        }

        public async Task<ServiceResult> UnlinkExternalProviderAsync(Guid userId, string provider)
        {
            var link = (await _unitOfWork.UserProvider.FindAnsyc(p => p.UserId == userId && p.Provider == provider)).FirstOrDefault();
            if (link == null)
                return new ServiceResult(false, "Provider not linked");

            await _unitOfWork.UserProvider.DeleteAnsync(link);
            await _unitOfWork.CommitAsync();
            return new ServiceResult(true, "Unlinked external provider");
        }

        public async Task<ServiceResult> VerifyMfaAsync(Guid userId, string otpCode)
        {
            var key = $"mfa:{userId}";
            var storedCode = await _redisService.GetAsync(key);
            if (storedCode != otpCode)
                return new ServiceResult(false, "Invalid OTP code");

            await _redisService.RemoveAsync(key);
            return new ServiceResult(true, "MFA verified successfully");
        }
    }
}
