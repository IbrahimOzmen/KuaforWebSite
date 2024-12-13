using System;
using KuaforRandevuSistemi.Core.Entities.Common;

namespace KuaforRandevuSistemi.Core.Entities
{
    public class AIStyleSuggestion : BaseEntity
    {
        public int UserId { get; set; }
        public string ImagePath { get; set; }
        public string Suggestions { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual AppUser User { get; set; }
    }
}