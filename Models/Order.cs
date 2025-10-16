using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CISS411_GroupProject.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        [Required]
        public int CustomerID { get; set; }
        [Required, MaxLength(50)]
        public string Occasion { get; set; } = null!;
        [Required]
        public DateTime DeliveryDate { get; set; }
        public decimal? Budget { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Awaiting Customer Approval, In Process, Ready, Picked Up
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

		// Linda: 10-13-25 For custom design orders
		public string? CustomDescription { get; set; }

        //Timestamp for when order is ready
        public DateTime? ReadyAt { get; set; }


        // Nav
        public User? Customer { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Design> Designs { get; set; } = new List<Design>();
        public ICollection<EmployeeAssignment> EmployeeAssignments { get; set; } = new List<EmployeeAssignment>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>(); // 1-per-customer-per-order enforced via unique index below
    }
}
