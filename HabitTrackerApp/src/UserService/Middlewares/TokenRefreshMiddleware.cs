using System.IdentityModel.Tokens.Jwt;
using UserService.Dto.User;
using UserService.Services.ServiceUser;

namespace UserService.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var accessToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var refreshToken = context.Request.Headers["X-Refresh-Token"].ToString();

            if (!string.IsNullOrEmpty(accessToken))
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var token = jwtHandler.ReadJwtToken(accessToken);

                var exp = token.ValidTo;
                if (DateTime.UtcNow > exp)
                {
                    var userId = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                    if (Guid.TryParse(userId, out Guid id))
                    {
                        var result = await userService.RefreshToken(id, refreshToken);
                        if (result.Success)
                        {
                            if (result.Data is RefreshTokenResponseDto tokenData)
                            {
                                context.Response.Headers["X-New-Access-Token"] = tokenData.Token;
                                context.Response.Headers["X-New-Expires-In"] = tokenData.ExpiresIn.ToString();
                            }
                        }
                        else
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Token expired or invalid.");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }

}
