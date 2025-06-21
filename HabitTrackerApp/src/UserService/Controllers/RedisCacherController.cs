using Microsoft.AspNetCore.Mvc;
using UserService.Services.Redis;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisController : ControllerBase
    {
        private readonly IRedisService _redisService;

        public RedisController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        [HttpPost("set")]
        public async Task<IActionResult> SetAsync([FromQuery] string key, [FromQuery] string value, [FromQuery] int timeMinute)
        {
            await _redisService.SetAsync(key, value, TimeSpan.FromMinutes(timeMinute));
            return Ok(new { message = "Key saved successfully." });
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAsync([FromQuery] string key)
        {
            var value = await _redisService.GetAsync(key);
            if (value == null)
                return NotFound(new { message = "Key not found" });

            return Ok(new { key, value });
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveAsync([FromQuery] string key)
        {
            await _redisService.RemoveAsync(key);
            return Ok(new { message = "Key removed successfully." });
        }
    }
}
