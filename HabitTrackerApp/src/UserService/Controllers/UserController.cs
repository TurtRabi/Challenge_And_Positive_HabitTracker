using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Dto.Request.User;
using UserService.Dto.User;
using UserService.Services.ServiceUser;


namespace UserService.Controllers
{
    [AllowAnonymous]
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
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            var userDto = new UserCreateDto
            {
                Email = request.Email,
                Password = request.Password,
                UserName = request.Username,
                Phone = request.Phone,
            };

            var result = await _userService.RegisterUser(userDto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            UserLoginDto loginDto = new UserLoginDto
            {
                UserName = request.UserName,
                Password = request.Password,
            };
            var result = await _userService.Login(loginDto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var result = await _userService.GetUserById(id);
            return Ok(result);
        }

        [HttpGet("getUserByEmail/{email}")]
        public async Task<IActionResult> GetUserByEmail([FromRoute] string email)
        {
            var result = await _userService.GetUserByEmail(email);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserById([FromRoute] Guid id, [FromBody] UserUpdateDto updateDto)
        {
            var result = await _userService.UpdateUserById(id, updateDto);
            return Ok(result);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changeDto)
        {
            var result = await _userService.ChangePassword(changeDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            var result = await _userService.DeleteUser(id);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ManageUserStatus([FromRoute] Guid id, [FromQuery] string status)
        {
            var result = await _userService.ManageUserStatus(id, status);
            return Ok(result);
        }

        [HttpPost("{id}/verify-email/send")]
        public async Task<IActionResult> SendEmailVerification([FromRoute] Guid id)
        {
            var result = await _userService.SendEmailVerificationAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/verify-email")]
        public async Task<IActionResult> VerifyEmail([FromRoute] Guid id, [FromQuery] string code)
        {
            var result = await _userService.VerifyEmailAsync(id, code);
            return Ok(result);
        }

        [HttpPost("{id}/verify-phone/send")]
        public async Task<IActionResult> SendPhoneVerification([FromRoute] Guid id)
        {
            var result = await _userService.SendPhoneVerificationAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/verify-phone")]
        public async Task<IActionResult> VerifyPhone([FromRoute] Guid id, [FromQuery] string code)
        {
            var result = await _userService.VerifyPhoneAsync(id, code);
            return Ok(result);
        }
        [HttpPost("{id}/logout")]
        public async Task<IActionResult> Logout([FromRoute] Guid id)
        {
            var result = await _userService.Logout(id);
            return Ok(result);
        }

        [HttpPost("{id}/refresh-token")]
        public async Task<IActionResult> RefreshToken([FromRoute] Guid id, [FromQuery] string refreshToken)
        {
            var result = await _userService.RefreshToken(id, refreshToken);
            return Ok(result);
        }
        [HttpPost("change-password/{id}/{newPassword}")]
        public async Task<IActionResult> ChangeNewPassword([FromRoute] Guid id,[FromRoute] string newPassword)
        {
            var result = await _userService.ChangeNewPassword(id,newPassword);
            return Ok(result);
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var result = await _userService.GetCurrentUserFromToken(User);
            return Ok(result);
        }
    }
}
