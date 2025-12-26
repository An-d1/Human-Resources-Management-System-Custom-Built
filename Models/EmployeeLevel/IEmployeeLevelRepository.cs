namespace HRManagementSystem.Models
{
    public interface IEmployeeLevelRepository
    {
        Task<IEnumerable<EmployeeLevel>> GetLevelsByCompanyAsync(Guid companyId);
        Task<EmployeeLevel?> GetLevelByIdAsync(Guid levelId);
        Task<EmployeeLevel> AddLevelAsync(EmployeeLevel level);
        Task<bool> DeleteLevelAsync(Guid id);
    }
}
