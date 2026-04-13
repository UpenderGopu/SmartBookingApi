using Microsoft.IdentityModel.Tokens;
using SmartBookingApi.Application.DTOs;
using SmartBookingApi.Core.Entities;
using SmartBookingApi.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartBookingApi.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration; // To read JwtSettings from appsettings.json

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            // Check if email already exists
            IEnumerable<User> existingUsers = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
            if (existingUsers.Any())
                throw new InvalidOperationException("A user with this email already exists.");

            // Create new user - hash the password before saving (NEVER store plain text passwords)
            User user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Secure hashing
                Role = "Admin" // Default role for all new registrations
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return "Registration successful.";
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            // Find user by email
            IEnumerable<User> users = await _unitOfWork.Users.FindAsync(u => u.Email == dto.Email);
            User? user = users.FirstOrDefault();

            // Verify user exists AND password matches the stored hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            // Generate and return a JWT token
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            // Read secret key from appsettings.json
            string secretKey = _configuration["JwtSettings:SecretKey"]!;
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims are pieces of information embedded inside the token
            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject = UserId
                new Claim(JwtRegisteredClaimNames.Email, user.Email),       // User's email
                new Claim(ClaimTypes.Role, user.Role),                      // "User" or "Admin"
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["JwtSettings:ExpiryInMinutes"]!)), // 60 min expiry
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token); // Serialize token to string
        }
    }
}
