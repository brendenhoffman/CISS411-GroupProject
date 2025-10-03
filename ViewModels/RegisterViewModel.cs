using System.ComponentModel.DataAnnotations;

namespace CISS411_GroupProject.ViewModels
{
    public class RegisterViewModel
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = null!;
        [Required, MaxLength(50)] public string LastName { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required, MaxLength(100)] public string Address { get; set; } = null!;
        [Required, Phone] public string PhoneNumber { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password), Compare(nameof(Password))]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
