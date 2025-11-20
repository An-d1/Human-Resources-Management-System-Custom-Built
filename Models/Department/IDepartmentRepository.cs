namespace HRManagementSystem.Models
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllDepartmentsOfTheCompanyAsync(Guid companyId);
        Task<Department> GetByIdAsync(Guid id);
        Task AddDepartmentWithAdminsCompanyIdAsync(Department department); // The companies Id will be passed on the controller
    }
}
