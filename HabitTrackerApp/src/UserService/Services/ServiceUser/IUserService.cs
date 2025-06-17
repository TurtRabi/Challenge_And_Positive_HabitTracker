using UserService.Common;
using UserService.Dto;
using System;
using System.Threading.Tasks;
using UserService.Dto.User;

namespace UserService.Services.ServiceUser
{
    public interface IUserService
    {
        Task<ServiceResult> RegisterUser(UserCreateDto userDto);

        Task<ServiceResult> Login(UserLoginDto loginDto);

        Task<ServiceResult> GetUserById(Guid id);

        Task<ServiceResult> UpdateUserById(Guid id, UserUpdateDto updateDto);

        Task<ServiceResult> ChangePassword(ChangePasswordDto changePasswordDto);
        Task<ServiceResult> VerifyEmailOrPhone(VerifyDto verifyDto);

        Task<ServiceResult> DeleteUser(Guid id);

        Task<ServiceResult> ManageUserStatus(Guid id, string status);
    }
}
