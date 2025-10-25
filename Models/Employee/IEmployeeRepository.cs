namespace HRManagementSystem.Models
{
    public interface IEmployeeRepository
    {
        Task<Employee> CreateAsync(Employee employee);
    }
}
