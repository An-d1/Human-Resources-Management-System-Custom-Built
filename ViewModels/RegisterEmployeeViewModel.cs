using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class RegisterEmployeeViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Required]
        public string PhoneNumber { get; set; } 

        // This field is for the Admin to set the initial role
        [Required]
        [Display(Name = "User Role")]
        public string Role { get; set; }
    }
}