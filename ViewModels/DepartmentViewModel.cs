using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class DepartmentViewModel
    {
        public string DepartmentId { get; set; } = default!;
        public string DepartmentName { get; set; } = default!;
    }
}
