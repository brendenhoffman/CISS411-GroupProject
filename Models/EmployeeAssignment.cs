using System;

namespace CISS411_GroupProject.Models
{
    public class EmployeeAssignment
    {
        public int AssignmentID { get; set; }
        public int OrderID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime AssignedAt { get; set; }

        // Nav
        public Order Order { get; set; } = null!;
        public User Employee { get; set; } = null!;
    }
}
