using System;
using System.ComponentModel.DataAnnotations;

namespace CISS411_GroupProject.Models
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int OrderID { get; set; }
        public int CustomerID { get; set; }

		// Nullable, 1–5
		public int? Rating { get; set; }

		[Required(ErrorMessage = "Please enter your feedback.")]
		[StringLength(1000, ErrorMessage = "Feedback cannot exceed 1000 characters.")]
		public string FeedbackText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Nav
        public Order Order { get; set; } = null!;
        public User Customer { get; set; } = null!;
    }
}
