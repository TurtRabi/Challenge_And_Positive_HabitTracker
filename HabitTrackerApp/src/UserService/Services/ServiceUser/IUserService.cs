using UserService.Common;
using UserService.Dto;
using System;
using System.Threading.Tasks;
using UserService.Dto.User;
using System.Security.Claims;


namespace UserService.Services.ServiceUser
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterUser(UserCreateDto userDto);

        Task<ServiceResult> Login(UserLoginDto loginDto);

        Task<ServiceResult> GetUserById(Guid id);

        Task<ServiceResult> UpdateUserById(Guid id, UserUpdateDto updateDto);

        Task<ServiceResult> ChangePassword(ChangePasswordDto changePasswordDto);

        Task<ServiceResult> DeleteUser(Guid id);

        Task<ServiceResult> ManageUserStatus(Guid id, string status);

        Task<ServiceResult> SendEmailVerificationAsync(Guid userId);

        Task<ServiceResult> VerifyEmailAsync(Guid userId, string code);

        Task<ServiceResult> SendPhoneVerificationAsync(Guid userId);

        Task<ServiceResult> VerifyPhoneAsync(Guid userId, string code);
        Task<ServiceResult> Logout(Guid userId);
        Task<ServiceResult> RefreshToken(Guid userId, string refreshToken);
        Task<ServiceResult> GetUserByEmail(string Email);
        Task<ServiceResult> ChangeNewPassword(Guid userId,string newPassword);
        Task<ServiceResult> GetCurrentUserFromToken(ClaimsPrincipal user);

    }
}
