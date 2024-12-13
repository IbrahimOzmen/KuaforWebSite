using System.Collections.Generic;
using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class AppUser : BaseEntity
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }

        public virtual ICollection<AIStyleSuggestion> AIStyleSuggestions { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}