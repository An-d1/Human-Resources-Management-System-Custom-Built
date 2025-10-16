using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class RegisterCompanyViewModel
    {
        [Required]
        public string CompanyName { get; set; }
        public string? TaxId { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; } // This is needed only on the inquiry phase so the company can be contacted, then if agreed the email of the Admin for the company will be used

        [Phone] 
        [Required]
        public string PhoneNumber { get; set; } // This is needed only on the inquiry phase so the company can be contaced


    }
}
