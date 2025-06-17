using Microsoft.AspNetCore.Mvc;
using UserService.Dto.Request.User;
using UserService.Dto.User;
using UserService.Services.ServiceUser;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterUserRequest request)
        {
            var userDto = new UserCreateDto
            {
                Email = request.Email,
                Password = request.Password,
                UserName = request.Username,
                Phone = request.Phone,
            };

            var rusult = await _userService.RegisterUser(userDto);
            
            return Ok(rusult);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login()
        {
            // TODO: Đăng nhập, trả về token
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var result = await _userService.GetUserById(id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserById([FromRoute] Guid id)
        {
            // TODO: Cập nhật user theo id
            return Ok();
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword()
        {
            // TODO: Đổi mật khẩu
            return Ok();
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmailOrPhone()
        {
            // TODO: Xác thực email hoặc SĐT
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            // TODO: Xoá user theo id
            return Ok();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ManageUserStatus([FromRoute] Guid id)
        {
            // TODO: Quản lý trạng thái user (active, inactive, blocked...)
            return Ok();
        }
    }
}
