using System;

namespace CISS411_GroupProject.Models
{
    public class Design
    {
        public int DesignID { get; set; }
        public int OrderID { get; set; }
        public int EmployeeID { get; set; }
        public string ImagePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public int ProposedQuantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? DesignNotes { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedByCustomerID { get; set; }

        // Nav
        public Order Order { get; set; } = null!;
        public User Employee { get; set; } = null!;
    }
}