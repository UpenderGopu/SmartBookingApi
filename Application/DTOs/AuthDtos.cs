namespace SmartBookingApi.Application.DTOs
{
    // Data we RECEIVE when a user wants to register
    public class RegisterDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Plain text - we will hash it in AuthService
    }
    // Data we RECEIVE when a user wants to login
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // Plain text - we will verify against hash
    }
}