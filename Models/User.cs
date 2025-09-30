using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CISS411_GroupProject.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "Visitor";

        [MaxLength(30)]
        public string Status { get; set; } = "Pending Confirmation";

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
