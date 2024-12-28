using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Core.Entities.Common;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KuaforRandevuSistemi.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public async Task<IEnumerable<T>> FindWithIncludeAsync(
    Expression<Func<T, bool>> predicate,
    params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                query = includes.Aggregate(query,
                    (current, include) => current.Include(include));
            }

            return await query.Where(predicate).ToListAsync();
        }

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate);
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Infrastructure/Repositories/Repository.cs
        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>> includes = null)
        {
            IQueryable<T> query = _dbSet;

            if (includes != null)
            {
                query = includes(query);
            }

            return await query.Where(predicate).ToListAsync();
        }
        public async Task<IEnumerable<T>> FindWithNavigationPropertiesAsync(Expression<Func<T, bool>> predicate)
        {
            // Entity Framework'ün ChangeTracker'ı kullanarak ilişkili verileri otomatik yüklemesi için
            _context.ChangeTracker.LazyLoadingEnabled = false;

            var query = _dbSet.AsQueryable();

            // T tipi Appointment ise
            if (typeof(T) == typeof(Appointment))
            {
                query = query
                    .Include("Service")
                    .Include("Employee")
                    .Include("Customer");
            }

            return await query.Where(predicate).ToListAsync();
        }
    }
}