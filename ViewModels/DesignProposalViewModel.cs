using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CISS411_GroupProject.ViewModels
{
    public class DesignProposalViewModel
    {
        public int OrderID { get; set; }

        [Required(ErrorMessage = "Please upload at least one design image")]
        [Display(Name = "Design Image")]
        public IFormFile? ImageFile { get; set; }

        [Required]
        [Display(Name = "Proposed Quantity")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int ProposedQuantity { get; set; }

        [Display(Name = "Design Notes")]
        [StringLength(500)]
        public string? DesignNotes { get; set; }

        [Display(Name = "Estimated Cost")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost must be greater than 0")]
        public decimal EstimatedCost { get; set; }

        // For display purposes
        public string? CustomerName { get; set; }
        public string? Occasion { get; set; }
        public decimal CustomerBudget { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? OrderItemsDescription { get; set; }
    }
}