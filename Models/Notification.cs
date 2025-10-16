/*Course #: CISS 411
Group 3: Ashley Steward, Linda Daniel, Allan Lopesandovall, Brenden Hoffman, 
Jason Farr, Jerome Whitaker, Jason Farr and Justin Kim.
Date Completed: 10-2-2025
Story Assignee: Ashley Steward 
User Story 8
*/
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace CISS411_GroupProject.Models
{
    //Created for customers to receive "ready message". 
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        public int CustomerID { get; set; }

        [ForeignKey("CustomerID")]
        public User? Customer { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
