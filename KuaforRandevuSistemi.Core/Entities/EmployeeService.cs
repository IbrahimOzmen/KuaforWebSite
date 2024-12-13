using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class EmployeeService : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }

        public virtual Employee Employee { get; set; }
        public virtual Service Service { get; set; }
    }
}