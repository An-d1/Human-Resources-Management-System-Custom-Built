using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "You must confirm the password.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match. Please re-enter the password")]
        public string ConfirmNewPassword { get; set; }
    }
}
