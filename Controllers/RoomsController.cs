using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBookingApi.Application.DTOs;
using SmartBookingApi.Core.Interfaces;

namespace SmartBookingApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // ALL endpoints in this controller require a valid JWT token by default
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET api/rooms  - Any logged-in user can see all rooms
        [HttpGet]
        public async Task<IActionResult> GetAllRooms()
        {
            IEnumerable<RoomDto> rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        // POST api/rooms  - Only Admin can create rooms
        [HttpPost]
        [Authorize(Roles = "Admin")] // Overrides class-level [Authorize] - restricts to Admin role only
        public async Task<IActionResult> CreateRoom([FromBody] RoomDto dto)
        {
            RoomDto createdRoom = await _roomService.CreateRoomAsync(dto);
            // 201 Created with the URL of the new resource and the created object
            return CreatedAtAction(nameof(GetAllRooms), new { id = createdRoom.Id }, createdRoom);
        }
    }
}