using System;

namespace CISS411_GroupProject.Models
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public int? Rating { get; set; } // 1–5, nullable per schema
        public string FeedbackText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        // Nav
        public Order Order { get; set; } = null!;
        public User Customer { get; set; } = null!;
    }
}
