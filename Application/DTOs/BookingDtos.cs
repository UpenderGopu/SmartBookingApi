namespace SmartBookingApi.Application.DTOs
{
    // Data we RECEIVE from the client when they want to make a booking
    public class CreateBookingDto
    {
        public int RoomId { get; set; }          // Which room to book
        public DateTime StartTime { get; set; }  // When the booking starts
        public DateTime EndTime { get; set; }    // When the booking ends
        // Note: UserId is NOT here - it comes from the JWT token, not the request body
    }
    // Data we SEND back to the client after a booking is created or when listing bookings
    public class BookingDto
    {
        public int Id { get; set; }              // The booking's unique ID
        public int RoomId { get; set; }          // Which room was booked
        public string RoomName { get; set; } = string.Empty; // Room name for display (e.g. "Board Room")
        public int UserId { get; set; }          // Who made the booking
        public DateTime StartTime { get; set; }  // Booking start time
        public DateTime EndTime { get; set; }    // Booking end time
    }
}