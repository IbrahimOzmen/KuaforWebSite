using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using KuaforRandevuSistemi.Core.Interfaces;
using KuaforRandevuSistemi.Core.Entities;
using KuaforRandevuSistemi.Infrastructure.Repositories;

namespace KuaforRandevuSistemi.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        private IRepository<Salon> _salons;
        private IRepository<Service> _services;
        private IRepository<Employee> _employees;
        private IRepository<AppUser> _appUsers;
        private IRepository<EmployeeService> _employeeServices;
        private IRepository<WorkingHours> _workingHours;
        private IRepository<Appointment> _appointments;
        private IRepository<AIStyleSuggestion> _aiStyleSuggestions;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<Salon> Salons =>
            _salons ??= new Repository<Salon>(_context);

        public IRepository<Service> Services =>
            _services ??= new Repository<Service>(_context);

        public IRepository<Employee> Employees =>
            _employees ??= new Repository<Employee>(_context);

        public IRepository<AppUser> AppUsers =>
            _appUsers ??= new Repository<AppUser>(_context);

        public IRepository<EmployeeService> EmployeeServices =>
            _employeeServices ??= new Repository<EmployeeService>(_context);

        public IRepository<WorkingHours> WorkingHours =>
            _workingHours ??= new Repository<WorkingHours>(_context);

        public IRepository<Appointment> Appointments =>
            _appointments ??= new Repository<Appointment>(_context);

        public IRepository<AIStyleSuggestion> AIStyleSuggestions =>
            _aiStyleSuggestions ??= new Repository<AIStyleSuggestion>(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                _transaction.Dispose();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction.Dispose();
            }
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}