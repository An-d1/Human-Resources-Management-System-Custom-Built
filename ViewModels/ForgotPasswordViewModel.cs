using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; }
    }
}
