namespace HRManagementSystem.Models;

public interface IEmployeeDepartmentRepository
{
    Task AssignDepartmentAsync(Guid userId, Guid departmentId);
}
