using Microsoft.AspNetCore.Mvc;
using SmartBookingApi.Application.DTOs;
using SmartBookingApi.Core.Interfaces;

namespace SmartBookingApi.Controllers
{
    [ApiController]               // Marks this as an API controller (enables automatic model validation)
    [Route("api/[controller]")]   // Route = api/auth (controller name without "Controller" suffix)
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            string message = await _authService.RegisterAsync(dto);
            return Ok(new { message }); // Returns: { "message": "Registration successful." }
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            string token = await _authService.LoginAsync(dto);
            return Ok(new { token }); // Returns: { "token": "eyJhbGciOiJ..." }
        }
    }
}