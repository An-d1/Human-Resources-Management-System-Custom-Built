using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class AddPayRateViewModel
    {
        [Required]
        public string UserId { get; set; }
        public string FullName { get; set; } // For display purposes only

        [Required(ErrorMessage = "Please enter an hourly rate")]
        [Range(0.01, 10000, ErrorMessage = "Rate must be greater than 0")]
        [Display(Name = "Hourly Rate")]
        public decimal HourlyRate { get; set; }

        [Required(ErrorMessage = "Please select a start date")]
        [DataType(DataType.Date)]
        [Display(Name = "Effective From")]
        public DateTime EffectiveDate { get; set; } = DateTime.Now;
    }
}
