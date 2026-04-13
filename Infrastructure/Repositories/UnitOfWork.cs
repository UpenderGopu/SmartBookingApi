using SmartBookingApi.Core.Entities;
using SmartBookingApi.Core.Interfaces;
using SmartBookingApi.Infrastructure.Data;

namespace SmartBookingApi.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        // One repository per entity - created once and reused
        public IGenericRepository<User> Users { get; private set; }
        public IGenericRepository<Room> Rooms { get; private set; }
        public IGenericRepository<Booking> Bookings { get; private set; }
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            // All 3 repositories share the SAME DbContext instance
            Users = new GenericRepository<User>(context);
            Rooms = new GenericRepository<Room>(context);
            Bookings = new GenericRepository<Booking>(context);
        }
        public async Task<int> SaveChangesAsync()
        {
            // This is the ONE place where ALL changes are committed to DB
            return await _context.SaveChangesAsync();
        }
        public void Dispose()
        {
            // Properly release the database connection when done
            _context.Dispose();
        }
    }
}
