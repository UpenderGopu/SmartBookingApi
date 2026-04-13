namespace SmartBookingApi.Application.DTOs
{
    // Used for BOTH sending room data to client AND receiving room data from client
    public class RoomDto
    {
        public int Id { get; set; }                        // 0 when creating, filled when returning
        public string Name { get; set; } = string.Empty;  // Room name e.g. "Conference Room A"
        public int Capacity { get; set; }                  // How many people fit in this room
    }
}