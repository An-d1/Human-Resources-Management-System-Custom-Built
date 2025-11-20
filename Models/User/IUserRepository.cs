namespace HRManagementSystem.Models;

public interface IUserRepository 
{
    Task<Guid> GetCompanyIdByUserIdAsync(Guid userId);
    Task<int> GetTheTotalNumberOfEmployeesUnderTheCompanyAsync(Guid companyID);
    Task<int> GetRecentHiresInTheLast30DaysAsync(Guid companyID);

}
