using Microsoft.EntityFrameworkCore;
using SmartBookingApi.Core.Interfaces;
using SmartBookingApi.Infrastructure.Data;
using System.Linq.Expressions;

namespace SmartBookingApi.Infrastructure.Repositories
{
    // This class implements IGenericRepository for ANY entity type T
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly AppDbContext _context; // Our EF Core database context
        private readonly DbSet<T> _dbSet;        // Represents the table for entity T
        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>(); // context.Set<User>() gives us the Users table
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id); // EF Core searches by Primary Key
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync(); // SELECT * FROM table
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync(); // SELECT * FROM table WHERE ...
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity); // Marks entity as "to be inserted" - not saved yet!
        }
        public void Update(T entity)
        {
            _dbSet.Update(entity); // Marks entity as "to be updated" - not saved yet!
        }
        public void Remove(T entity)
        {
            _dbSet.Remove(entity); // Marks entity as "to be deleted" - not saved yet!
        }
    }
}
