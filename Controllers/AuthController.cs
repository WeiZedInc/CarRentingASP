using CarRentalSystem.DTOs;
using CarRentalSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register(UserRegistrationDto registrationDto)
        {
            var result = await _authService.Register(registrationDto);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(new { message = "Registration successful" });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto loginDto)
        {
            var result = await _authService.Login(loginDto);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(new { token = result.Token, user = result.User });
        }

        // POST: api/auth/google-login
        [HttpPost("google-login")]
        public async Task<ActionResult> GoogleLogin([FromBody] GoogleLoginDto googleLogin)
        {
            var result = await _authService.GoogleLogin(googleLogin.GoogleId, googleLogin.Email, googleLogin.Name);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok(new { token = result.Token, user = result.User });
        }
    }

    public class GoogleLoginDto
    {
        public string GoogleId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}