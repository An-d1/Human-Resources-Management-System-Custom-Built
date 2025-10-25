namespace HRManagementSystem.Models
{
    public interface IEmployeeRepository
    {
        Task<Employee> CreateAsync(Employee employee);
        Task<List<Employee>> ReturnAllEmployeesOfTheCompanyAsync(Guid companyId);
        Task<List<string>> GetDepartmentNamesByUserIdAsync(Guid userId);
    }
}
