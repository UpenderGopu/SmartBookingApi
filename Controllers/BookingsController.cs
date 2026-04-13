using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookingApi.Application.DTOs;
using SmartBookingApi.Core.Interfaces;
using System.Security.Claims;

namespace SmartBookingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All booking endpoints require login
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET api/bookings/my  - Get all bookings for the currently logged-in user
        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            int userId = GetCurrentUserId(); // Extract userId from JWT token
            IEnumerable<BookingDto> bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        // POST api/bookings  - Create a new booking
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            int userId = GetCurrentUserId(); // We get userId from token, NOT from request body
            BookingDto booking = await _bookingService.CreateBookingAsync(dto, userId);
            return CreatedAtAction(nameof(GetMyBookings), new { id = booking.Id }, booking);
        }

        // DELETE api/bookings/5  - Cancel a booking by its Id
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            int userId = GetCurrentUserId();
            await _bookingService.CancelBookingAsync(id, userId);
            return NoContent(); // HTTP 204 - success but nothing to return
        }

        // Private helper: extract the UserId from the JWT token claims
        private int GetCurrentUserId()
        {
            // When JWT token is validated, ASP.NET Core extracts its claims into User.Claims
            // "sub" claim holds the UserId we put in during token generation in AuthService
            string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                                  ?? User.FindFirstValue("sub");
            return int.Parse(userIdClaim!);
        }
    }
}
