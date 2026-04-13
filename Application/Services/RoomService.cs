using SmartBookingApi.Application.DTOs;
using SmartBookingApi.Core.Entities;
using SmartBookingApi.Core.Interfaces;
namespace SmartBookingApi.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        // IUnitOfWork is injected - RoomService never touches DbContext directly
        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<RoomDto>> GetAllRoomsAsync()
        {
            var rooms = await _unitOfWork.Rooms.GetAllAsync(); // Fetch all Room entities from DB
            // Map each Room entity to a RoomDto - never return raw entities to the caller
            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Capacity = r.Capacity
            });
        }
        public async Task<RoomDto> CreateRoomAsync(RoomDto dto)
        {
            // Map the incoming DTO to a Room entity
            var room = new Room
            {
                Name = dto.Name,
                Capacity = dto.Capacity
            };
            await _unitOfWork.Rooms.AddAsync(room); // Stage the insert (not saved yet)
            await _unitOfWork.SaveChangesAsync();    // NOW save to database - Id gets generated here
            dto.Id = room.Id; // EF Core fills room.Id after save - put it back in the DTO
            return dto;       // Return the DTO with the new Id
        }
    }
}