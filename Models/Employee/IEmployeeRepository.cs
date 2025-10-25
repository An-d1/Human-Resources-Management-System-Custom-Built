namespace HRManagementSystem.Models
{
    public interface IEmployeeRepository
    {
        Task<Employee> CreateAsync(Employee employee);
        Task<IEnumerable<Employee>> ReturnAllEmployeesAsync();
        Task<List<string>> GetDepartmentNamesByUserIdAsync(Guid userId);

    }
}
