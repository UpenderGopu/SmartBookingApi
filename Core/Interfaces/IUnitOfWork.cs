namespace SmartBookingApi.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // One repository per entity - accessed through Unit of Work
        IGenericRepository<Entities.User> Users { get; }
        IGenericRepository<Entities.Room> Rooms { get; }
        IGenericRepository<Entities.Booking> Bookings { get; }
        // Single save point - commits ALL pending changes to the database at once
        Task<int> SaveChangesAsync();
    }
}
