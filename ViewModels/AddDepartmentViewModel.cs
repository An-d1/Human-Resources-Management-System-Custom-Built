using System.ComponentModel.DataAnnotations;
namespace HRManagementSystem.ViewModels

{
    public class AddDepartmentViewModel
    {
        [Required]
        public string DepartmentName { get; set; }
    }
}
