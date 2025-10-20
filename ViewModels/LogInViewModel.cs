using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class LogInViewModel
    {
        public string? FirstName { get; set; } // Used only to pass the name of the user to the dashboard

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

    }
}
