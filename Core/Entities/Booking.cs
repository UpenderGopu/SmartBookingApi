namespace SmartBookingApi.Core.Entities
{
    public class Booking
    {
        public int Id { get; set; }

        // Foreign Key to Room
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        // Foreign Key to User
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
