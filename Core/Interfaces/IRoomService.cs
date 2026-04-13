using SmartBookingApi.Application.DTOs;
namespace SmartBookingApi.Core.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllRoomsAsync();   // Get list of all rooms
        Task<RoomDto> CreateRoomAsync(RoomDto dto);      // Admin creates a new room
    }
}