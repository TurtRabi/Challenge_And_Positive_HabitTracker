using Microsoft.AspNetCore.Mvc;
using UserService.Services.UserProviderService;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProviderController : ControllerBase
    {
        private readonly IUserProviderService _providerService;

        public UserProviderController(IUserProviderService providerService)
        {
            _providerService = providerService;
        }

        [HttpPost("{userId}/link")]
        public async Task<IActionResult> LinkExternalProvider([FromRoute] Guid userId, [FromQuery] string provider, [FromQuery] string providerUserId)
        {
            var result = await _providerService.LinkExternalProviderAsync(userId, provider, providerUserId);
            return Ok(result);
        }

        [HttpPost("{userId}/unlink")]
        public async Task<IActionResult> UnlinkExternalProvider([FromRoute] Guid userId, [FromQuery] string provider)
        {
            var result = await _providerService.UnlinkExternalProviderAsync(userId, provider);
            return Ok(result);
        }

        [HttpGet("{userId}/linked")]
        public async Task<IActionResult> GetLinkedProviders([FromRoute] Guid userId)
        {
            var result = await _providerService.GetLinkedProvidersAsync(userId);
            return Ok(result);
        }

        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromQuery] string provider, [FromQuery] string accessToken, [FromQuery] string clientType)
        {
            var result = await _providerService.SocialLoginAsync(provider, accessToken, clientType);
            return Ok(result);
        }

        [HttpPost("{userId}/start-mfa")]
        public async Task<IActionResult> StartMfa([FromRoute] Guid userId)
        {
            var result = await _providerService.StartMfaAsync(userId);
            return Ok(result);
        }

        [HttpPost("{userId}/verify-mfa")]
        public async Task<IActionResult> VerifyMfa([FromRoute] Guid userId, [FromQuery] string otpCode)
        {
            var result = await _providerService.VerifyMfaAsync(userId, otpCode);
            return Ok(result);
        }

        [HttpGet("admin/search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _providerService.SearchUsersAsync(keyword, page, pageSize);
            return Ok(result);
        }
    }
}
