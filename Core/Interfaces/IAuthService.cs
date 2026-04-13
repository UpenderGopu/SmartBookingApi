using SmartBookingApi.Application.DTOs;
namespace SmartBookingApi.Core.Interfaces
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDto dto);  // Returns success message
        Task<string> LoginAsync(LoginDto dto);         // Returns JWT token string
    }
}