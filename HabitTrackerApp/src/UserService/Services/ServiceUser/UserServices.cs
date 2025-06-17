using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Common;
using UserService.Common.Enum;
using UserService.Dto.Response;
using UserService.Dto.Role;
using UserService.Dto.User;
using UserService.Models;
using UserService.Repositories.UOW;
using UserService.Services.ServiceRole;

namespace UserService.Services.ServiceUser
{
    public class UserServices : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserServices(IUnitOfWork unitOfWork,IRoleService roleService)
        {
            _unitOfWork = unitOfWork;
        }
        public Task<ServiceResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> DeleteUser(Guid id)
        {
            throw new NotImplementedException();
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


        public Task<ServiceResult> Login(UserLoginDto loginDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> ManageUserStatus(Guid id, string status)
        {
            throw new NotImplementedException();
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





        public Task<ServiceResult> UpdateUserById(Guid id, UserUpdateDto updateDto)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResult> VerifyEmailOrPhone(VerifyDto verifyDto)
        {
            throw new NotImplementedException();
        }
    }
}
