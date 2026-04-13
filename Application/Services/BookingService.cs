using SmartBookingApi.Application.DTOs;
using SmartBookingApi.Core.Entities;
using SmartBookingApi.Core.Interfaces;
namespace SmartBookingApi.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto, int userId)
        {
            // BUSINESS RULE: Check if the room is already booked for the requested time slot
            var overlapping = await _unitOfWork.Bookings.FindAsync(b =>
                b.RoomId == dto.RoomId &&
                b.StartTime < dto.EndTime &&   // Existing booking starts before new one ends
                b.EndTime > dto.StartTime);    // Existing booking ends after new one starts
            if (overlapping.Any())
                throw new InvalidOperationException("Room is already booked for this time slot.");
            // No overlap found - safe to create the booking
            var booking = new Booking
            {
                RoomId = dto.RoomId,
                UserId = userId,          // From JWT token - not from request body
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };
            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();
            // Fetch the room name to include in the response DTO
            var room = await _unitOfWork.Rooms.GetByIdAsync(booking.RoomId);
            return new BookingDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomName = room?.Name ?? "Unknown",
                UserId = booking.UserId,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime
            };
        }
        public async Task CancelBookingAsync(int bookingId, int userId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found.");
            // BUSINESS RULE: Only the person who made the booking can cancel it
            if (booking.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own bookings.");
            _unitOfWork.Bookings.Remove(booking);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _unitOfWork.Bookings.FindAsync(b => b.UserId == userId);
            return bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                RoomId = b.RoomId,
                RoomName = b.Room?.Name ?? "Unknown",
                UserId = b.UserId,
                StartTime = b.StartTime,
                EndTime = b.EndTime
            });
        }
    }
}