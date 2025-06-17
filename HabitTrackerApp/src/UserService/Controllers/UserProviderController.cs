using Microsoft.AspNetCore.Mvc;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserProviderController : ControllerBase
    {
       
        [HttpPost("link")]
        public async Task<IActionResult> LinkExternalProvider()
        {
            
            return Ok();
        }


        [HttpPost("unlink")]
        public async Task<IActionResult> UnlinkExternalProvider()
        {
            
            return Ok();
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetLinkedProviders()
        {

            return Ok();
        }

   
        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin()
        {
           
            return Ok();
        }

    
        [HttpPost("start-mfa")]
        public async Task<IActionResult> StartMfa()
        {
            
            return Ok();
        }

        [HttpPost("verify-mfa")]
        public async Task<IActionResult> VerifyMfa()
        {
            
            return Ok();
        }

        
        [HttpGet("login-history")]
        public async Task<IActionResult> GetLoginHistory()
        {
           
            return Ok();
        }

        
        [HttpGet("admin/search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return Ok();
        }

    }
}
