using System.ComponentModel.DataAnnotations;

namespace CISS411_GroupProject.ViewModels
{
    public class UserRoleItem
    {
        public int AppUserId { get; set; }
        public string IdentityUserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string CurrentRole { get; set; } = "Visitor";

        [Display(Name = "New Role")]
        public string? NewRole { get; set; }
    }
}
