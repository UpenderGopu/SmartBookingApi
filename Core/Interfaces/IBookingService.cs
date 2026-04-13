using SmartBookingApi.Application.DTOs;
namespace SmartBookingApi.Core.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto, int userId); // Create a booking
        Task CancelBookingAsync(int bookingId, int userId);                    // Cancel a booking
        Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId);        // Get all bookings for a user
    }
}