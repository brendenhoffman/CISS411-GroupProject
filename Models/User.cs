using System;
using System.Collections.Generic;

namespace CISS411_GroupProject.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        // Keeping single Address field per current schema; you can later split into Street/City/State/Zip if desired.
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        // Using string role/status per schema (“Visitor”, “Customer”, “Employee”, “Admin”; “Pending Confirmation”, “Active”, “Inactive”)
        public string Role { get; set; } = "Visitor";
        public string Status { get; set; } = "Pending Confirmation";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Nav
        public ICollection<Order> Orders { get; set; } = new List<Order>();                // as Customer
        public ICollection<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>(); // as Employee
        public ICollection<Design> Designs { get; set; } = new List<Design>();            // as Employee
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();      // as Customer
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();      // as Actor
    }
}
