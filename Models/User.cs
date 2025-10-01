using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CISS411_GroupProject.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Address { get; set; } = null!;

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, Phone, MaxLength(20)]
        public string PhoneNumber { get; set; } = null!;

        [MaxLength(20)]
        public string Role { get; set; } = "Visitor";

        [MaxLength(30)]
        public string Status { get; set; } = "Pending Confirmation";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navs — keep them initialized to avoid null refs in views
        public ICollection<Order> Orders { get; set; } = new List<Order>();                    // as Customer
        public ICollection<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>(); // as Employee
        public ICollection<Design> Designs { get; set; } = new List<Design>();                // as Employee
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();          // as Customer
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();          // as Actor
    }
}
