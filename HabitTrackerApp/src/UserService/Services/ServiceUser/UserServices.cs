using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Common;
using UserService.Common.Enum;
using UserService.Dto.Response;
using UserService.Dto.Role;
using UserService.Dto.User;
using UserService.Models;
using UserService.Repositories.UOW;
using UserService.Services.Redis;
using UserService.Services.ServiceRole;
using UserService.Services.ServiceSendEmail;

namespace UserService.Services.ServiceUser
{
    public class UserServices : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisService _redisService;
        private readonly JwtSettings _jwtSettings;
        private readonly IEmailService _emailService;

        public UserServices(IUnitOfWork unitOfWork, IRedisService redisService, IOptions<JwtSettings> jwtOptions,IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _redisService = redisService;
            _jwtSettings = jwtOptions.Value;
            _emailService = emailService;
        }

        public async Task<ServiceResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var result = new ServiceResult();
            var user = (await _unitOfWork.user.FindAnsyc(U=>U.Id ==changePasswordDto.UserId)).FirstOrDefault();
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not found.";
                return result;
            }

            var hasher = new PasswordHasher<object>();

            var verificationResult = hasher.VerifyHashedPassword(null, user.PasswordHash,changePasswordDto.OldPassword);

            if(verificationResult == PasswordVerificationResult.Failed)
            {
                result.Success = false;
                result.Message = "Old password is incorrect.";
                return result;
            }

            user.PasswordHash = hasher.HashPassword(null, changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            result.Success = true;
            result.Message = "Password changed successfully.";
            return result;
        }

        public async Task<ServiceResult> DeleteUser(Guid id)
        {
            var result = new ServiceResult();
            var user = (await _unitOfWork.user.FindAnsyc(u=>u.Id == id)).FirstOrDefault();
            if(user == null)
            {
                result.Success = false;
                result.Message = "User not found.";
                return result;
            }

            user.Status = UserStatus.Deleted.ToString();
            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            result.Success = true;
            result.Message = "User deleted successfully.";
            return result;
        }

        public async Task<ServiceResult> GetUserById(Guid id)
        {
            var result = new ServiceResult();

            var getUserById = _unitOfWork.user.Query()
                .Include(x => x.Roles)
                .FirstOrDefault(u => u.Id == id);

            if (getUserById == null)
            {
                result.Success = false;
                result.Message = "User not exist";
                return result;
            }

            var userDto = new UserWithRolesRequest
            {
                Id = getUserById.Id,
                Email = getUserById.Email,
                Username = getUserById.Username,
                Phone = getUserById.PhoneNumber,
                Roles = getUserById.Roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };

            result.Success = true;
            result.Message = "Get User";
            result.Data = userDto;
            return result;
        }


        public async Task<ServiceResult> Login(UserLoginDto loginDto)
        {
            var result = new ServiceResult();

            var user = (await _unitOfWork.user.FindAnsyc(u => u.Username == loginDto.UserName)).FirstOrDefault();
            if (user == null)
            {
                result.Success = false;
                result.Message = "Invalid username or password.";
                return result;
            }

            var hasher = new PasswordHasher<object>();
            var verifyResult = hasher.VerifyHashedPassword(null, user.PasswordHash, loginDto.Password);
            if (verifyResult == PasswordVerificationResult.Failed)
            {
                result.Success = false;
                result.Message = "Invalid username or password.";
                return result;
            }
            var getUserById = _unitOfWork.user.Query()
                .Include(x => x.Roles)
                .FirstOrDefault(u => u.Id == user.Id);
            

            // Create Access Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };
            claims.AddRange(getUserById.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name)));

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
            var accessToken = tokenHandler.WriteToken(token);

            var refreshToken = Guid.NewGuid().ToString();
            var accessKey = $"auth:token:{user.Id}";
            var refreshKey = $"auth:refresh:{user.Id}";

            var expireMinutes = Math.Max(1, _jwtSettings.ExpiresInMinutes);
            await _redisService.SetAsync(accessKey, accessToken, TimeSpan.FromMinutes(expireMinutes));
            await _redisService.SetAsync(refreshKey, refreshToken, TimeSpan.FromDays(7));

            result.Success = true;
            result.Message = "Login successful.";
            result.Data = new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IDUser = user.Id,
                ExpiresIn = expireMinutes * 60
            };

            return result;
        }



        public async Task<ServiceResult> ManageUserStatus(Guid id, string status)
        {
            var result = new ServiceResult();
            var user = (await _unitOfWork.user.FindAnsyc(u=>u.Id == id)).FirstOrDefault();  

            if(user == null)
            {
                result.Success = false;
                result.Message = "User not found.";
                return result;
            }

            if (!Enum.TryParse<UserStatus>(status, true, out var newStatus))
            {
                result.Success = false;
                result.Message = "Invalid status value.";
                return result;
            }

            user.Status = newStatus.ToString();
            user.UpdatedAt = DateTime.Now;

            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            result.Success = true;
            result.Message = "User status updated.";
            return result;
        }

        public async Task<ServiceResult> RegisterUser(UserCreateDto userDto)
        {
            var result = new ServiceResult();

            if (string.IsNullOrWhiteSpace(userDto.Email) ||
                string.IsNullOrWhiteSpace(userDto.UserName) ||
                string.IsNullOrWhiteSpace(userDto.Password))
            {
                result.Success = false;
                result.Message = "Email, Username, and Password are required.";
                return result;
            }

            var existingUserByEmail = (await _unitOfWork.user.FindAnsyc(u => u.Email == userDto.Email)).FirstOrDefault();
            if (existingUserByEmail != null)
            {
                result.Success = false;
                result.Message = "A user with this email already exists.";
                return result;
            }

            var existingUserByUsername = (await _unitOfWork.user.FindAnsyc(u => u.Username == userDto.UserName)).FirstOrDefault();
            if (existingUserByUsername != null)
            {
                result.Success = false;
                result.Message = "A user with this username already exists.";
                return result;
            }

            var hasher = new PasswordHasher<object>();
            var hashedPassword = hasher.HashPassword(null, userDto.Password);

            var roleResult = (await _unitOfWork.role.FindAnsyc(r => r.Name.ToLower() == "user")).FirstOrDefault();
            if (roleResult == null)
            {
                result.Success = false;
                result.Message = "Role 'user' not found.";
                return result;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = userDto.Email,
                Username = userDto.UserName,
                PhoneNumber = userDto.Phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = UserStatus.Active.ToString(),
                PasswordHash = hashedPassword,
                Roles = new List<Role> { roleResult },
                EmailVerified = false,
                PhoneVerified = false
            };


            await _unitOfWork.user.AddAnsync(user);
            await _unitOfWork.CommitAsync();
            var userResponse = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Phone = user.PhoneNumber
            };

            result.Success = true;
            result.Message = "User registered successfully.";
            result.Data = userResponse;

            return result;
        }





        public async Task<ServiceResult> UpdateUserById(Guid id, UserUpdateDto updateDto)
        {
            var result = new ServiceResult();

            var user = (await _unitOfWork.user.FindAnsyc(u => u.Id == id)).FirstOrDefault();
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not found.";
                return result;
            }

            user.FullName = updateDto.FullName;
            user.AvatarUrl = updateDto.AvatarUrl;
            user.PhoneNumber = updateDto.PhoneNumber;
            user.Gender = updateDto.Gender;
            user.DateOfBirth = updateDto.DateOfBirth;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            var updatedUser = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Phone = user.PhoneNumber
            };

            result.Success = true;
            result.Message = "User updated successfully.";
            result.Data = updatedUser;

            return result;
        }


        public async Task<ServiceResult> SendEmailVerificationAsync(Guid userId)
        {
            var user = (await _unitOfWork.user.FindAnsyc(u => u.Id == userId)).FirstOrDefault();
            if (user == null)
                return new ServiceResult(false, "User not found");

            var code = Generate6DigitCode();
            var key = $"verify:email:{user.Email}";

            await _redisService.SetAsync(key, code, TimeSpan.FromMinutes(10));
            await _emailService.SendEmailAsync(user.Email, "Email Verification Code", $"Your code is: {code}");

            return new ServiceResult(true, "Verification code sent to email.");
        }

        public async Task<ServiceResult> VerifyEmailAsync(Guid userId, string code)
        {
            var user = (await _unitOfWork.user.FindAnsyc(u => u.Id == userId)).FirstOrDefault();
            if (user == null)
                return new ServiceResult(false, "User not found");

            var key = $"verify:email:{user.Email}";
            var storedCode = await _redisService.GetAsync(key);

            if (storedCode == null || storedCode != code)
                return new ServiceResult(false, "Invalid or expired code");

            user.EmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            await _redisService.RemoveAsync(key);

            return new ServiceResult(true, "Email verified successfully.");
        }

        public async Task<ServiceResult> SendPhoneVerificationAsync(Guid userId)
        {
            var user = (await _unitOfWork.user.FindAnsyc(u=>u.Id==userId)).FirstOrDefault();
            if (user == null)
                return new ServiceResult(false, "User not found");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                return new ServiceResult(false, "User does not have a phone number.");

            var code = Generate6DigitCode();
            var key = $"verify:phone:{user.PhoneNumber}";

            await _redisService.SetAsync(key, code, TimeSpan.FromMinutes(10));

            // Gửi SMS (ở đây bạn có thể thay thế bằng tích hợp thực tế như Twilio)
            await SendSmsAsync(user.PhoneNumber, $"Your verification code is: {code}");

            return new ServiceResult(true, "Verification code sent to phone.");
        }

        public async Task<ServiceResult> VerifyPhoneAsync(Guid userId, string code)
        {
            var user = (await _unitOfWork.user.FindAnsyc(u => u.Id == userId)).FirstOrDefault();
            if (user == null)
                return new ServiceResult(false, "User not found");

            var key = $"verify:phone:{user.PhoneNumber}";
            var storedCode = await _redisService.GetAsync(key);

            if (storedCode == null || storedCode != code)
                return new ServiceResult(false, "Invalid or expired code");

            user.PhoneVerified = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            await _redisService.RemoveAsync(key);

            return new ServiceResult(true, "Phone verified successfully.");
        }

        private string Generate6DigitCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }



        private Task SendSmsAsync(string phoneNumber, string message)
        {
            Console.WriteLine("------ MOCK SMS ------");
            Console.WriteLine($"To: {phoneNumber}");
            Console.WriteLine($"Message: {message}");
            Console.WriteLine("----------------------");
            return Task.CompletedTask;
        }

        public async Task<ServiceResult> Logout(Guid userId)
        {
            var result = new ServiceResult();
            var accessKey = $"auth:token:{userId}";
            var refreshKey = $"auth:refresh:{userId}";

            await _redisService.RemoveAsync(accessKey);
            await _redisService.RemoveAsync(refreshKey);

            result.Success = true;
            result.Message = "Logout successful.";
            return result;
        }

        public async Task<ServiceResult> RefreshToken(Guid userId, string refreshToken)
        {
            var result = new ServiceResult();
            var redisRefreshKey = $"auth:refresh:{userId}";
            var storedRefreshToken = await _redisService.GetAsync(redisRefreshKey);
            if (storedRefreshToken == null || storedRefreshToken != refreshToken)
            {
                result.Success = false;
                result.Message = "Invalid or expired refresh token.";
                return result;
            }

            var user = (await _unitOfWork.user.FindAnsyc(u => u.Id == userId)).FirstOrDefault();

            if(user == null)
            {
                result.Success = false;
                result.Message = "User not found.";
                return result;
            }
            var getUserById = _unitOfWork.user.Query()
                .Include(x => x.Roles)
                .FirstOrDefault(u => u.Id == user.Id);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username)
            };
            claims.AddRange(getUserById.Roles.Select(r => new Claim(ClaimTypes.Role, r.Name)));
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
            var newAccessToken = tokenHandler.WriteToken(token);
            var redisAccessKey = $"auth:token:{user.Id}";
            var expireMinutes = Math.Max(1, _jwtSettings.ExpiresInMinutes);
            await _redisService.SetAsync(redisAccessKey, newAccessToken, TimeSpan.FromMinutes(expireMinutes));


            result.Success = true;
            result.Message = "Token refreshed successfully.";
            result.Data = new
            {
                Token = newAccessToken,
                ExpiresIn = expireMinutes * 60
            };

            return result;
        }

        public async Task<ServiceResult> GetUserByEmail(string Email)
        {
            var result = new ServiceResult();
            var getUserByEmail = (await _unitOfWork.user.FindAnsyc(u => u.Email.Equals(Email))).FirstOrDefault();
            if (getUserByEmail == null)
            {
                result.Success = true;
                result.Message="User not found";

                return result;
            }
            var userDto = new UserWithRolesRequest
            {
                Id = getUserByEmail.Id,
                Email = getUserByEmail.Email,
                Username = getUserByEmail.Username,
                Phone = getUserByEmail.PhoneNumber,
                Roles = getUserByEmail.Roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name
                }).ToList()
            };


            result.Success= true;
            result.Data = userDto;

            return result;
        }

        public async Task<ServiceResult> ChangeNewPassword(Guid id,string newPassword)
        {
            var result = new ServiceResult();
            var user = (await _unitOfWork.user.FindAnsyc(U => U.Id == id)).FirstOrDefault();
            if (user == null)
            {
                result.Success = false;
                result.Message = "User not found.";
                return result;
            }

            var hasher = new PasswordHasher<object>();


            user.PasswordHash = hasher.HashPassword(null, newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.user.UpdateAnsync(user);
            await _unitOfWork.CommitAsync();

            result.Success = true;
            result.Message = "Password changed successfully.";
            return result;
        }

        public async Task<ServiceResult> GetCurrentUserFromToken(ClaimsPrincipal user)
        {
            try
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return new ServiceResult(false, "User ID not found in token");

                var userId = Guid.Parse(userIdClaim.Value);

                var userData = _unitOfWork.user.Query()
                .Include(x => x.Roles)
                .FirstOrDefault(u => u.Id == userId);



                if (userData == null)
                    return new ServiceResult(false, "User not found");

                var userDto = new UserDto
                {
                    Id = userData.Id,
                    Email = userData.Email,
                    UserName = userData.Username,
                    PhoneNumber = userData.PhoneNumber,
                    AvatarUrl = userData.AvatarUrl,
                    FullName = userData.FullName,
                    Gender = userData.Gender,
                    DateOfBirth = userData.DateOfBirth,
                    Status = userData.Status,
                    EmailVerified = userData.EmailVerified,
                    PhoneVerified = userData.PhoneVerified,
                    Roles = userData.Roles.Select(ur => new RoleDto
                    {
                        Id = ur.Id,
                        Name = ur.Name
                    }).ToList()
                };

                return new ServiceResult(true, "User information", userDto);
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Error: " + ex.Message);
            }
        }

    }
}
