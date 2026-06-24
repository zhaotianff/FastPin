using FastPin.Api.Auth;
using FastPin.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FastPin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApiAuthOptions _options;
    private readonly TokenStore _tokenStore;

    public AuthController(IOptions<ApiAuthOptions> options, TokenStore tokenStore)
    {
        _options = options.Value;
        _tokenStore = tokenStore;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!string.Equals(request.Username, _options.Username, StringComparison.Ordinal) ||
            !string.Equals(request.Password, _options.Password, StringComparison.Ordinal))
        {
            return Unauthorized();
        }

        var token = _tokenStore.CreateToken();
        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresInMinutes = _options.TokenLifetimeMinutes
        });
    }
}
