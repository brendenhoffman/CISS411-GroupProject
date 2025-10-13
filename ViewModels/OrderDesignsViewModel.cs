using CISS411_GroupProject.Models;

namespace CISS411_GroupProject.ViewModels
{
    public class OrderDesignsViewModel
    {
        public Order Order { get; set; } = null!;
        public List<DesignWithDetails> Designs { get; set; } = new();
        public bool CanApprove { get; set; }
        public bool CanUpload { get; set; }
    }

    public class DesignWithDetails
    {
        public int DesignID { get; set; }
        public string ImagePath { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public string EmployeeName { get; set; } = null!;
        public int ProposedQuantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public string? DesignNotes { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedByCustomerID { get; set; }
        public string? ApprovedByCustomerName { get; set; }
    }
}