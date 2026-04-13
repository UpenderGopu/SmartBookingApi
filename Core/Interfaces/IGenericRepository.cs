using System.Linq.Expressions;

namespace SmartBookingApi.Core.Interfaces
{
    // T is a generic type - this one interface works for User, Room, AND Booking
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);          // Get one record by its Id
        Task<IEnumerable<T>> GetAllAsync();     // Get all records
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate); // Get records matching a condition
        Task AddAsync(T entity);                // Insert a new record
        void Update(T entity);                  // Update an existing record
        void Remove(T entity);                  // Delete a record
    }
}
