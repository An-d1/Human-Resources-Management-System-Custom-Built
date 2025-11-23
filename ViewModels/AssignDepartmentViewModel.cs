using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRManagementSystem.ViewModels;
public class AssignDepartmentViewModel
{
    public Guid UserId { get; set; }
    public string? EmployeeFullName { get; set; }
    public Guid SelectedDepartmentId { get; set; }
    public List<SelectListItem>? Departments { get; set; }
}
