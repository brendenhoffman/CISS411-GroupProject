using System;

namespace CISS411_GroupProject.Models
{
    public class AuditLog
    {
        public int LogID { get; set; }
        public int UserID { get; set; }          // actor making the change
        public string TableAffected { get; set; } = null!;
        public int RecordID { get; set; }
        public string Action { get; set; } = null!; // e.g. RoleUpdated, StatusChanged
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }

        // Nav
        public User User { get; set; } = null!;
    }
}
