namespace HRManagementSystem.Models
{
    public interface IEmployeeRepository
    {
        Task<Employee> FindEmployeeByIdAsync(string employeeId);
        Task UpdateEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(Employee employee);
        Task<Employee> CreateAsync(Employee employee);
        Task<List<Employee>> ReturnAllEmployeesOfTheCompanyAsync(Guid companyId);
        Task<List<string>> GetDepartmentNamesByUserIdAsync(Guid userId);
    }
}
