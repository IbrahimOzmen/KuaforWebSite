using System;
using System.Threading.Tasks;
using KuaforRandevuSistemi.Core.Entities;

namespace KuaforRandevuSistemi.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Salon> Salons { get; }
        IRepository<Service> Services { get; }
        IRepository<Employee> Employees { get; }
        IRepository<AppUser> AppUsers { get; }
        IRepository<EmployeeService> EmployeeServices { get; }
        IRepository<WorkingHours> WorkingHours { get; }
        IRepository<Appointment> Appointments { get; }
        IRepository<AIStyleSuggestion> AIStyleSuggestions { get; }

        Task<int> CompleteAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}