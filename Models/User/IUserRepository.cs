namespace HRManagementSystem.Models;

public interface IUserRepository 
{
    Task<Guid?> GetCompanyIdByUserIdAsync(Guid userId);
<<<<<<< HEAD
    Task<int> GetTheTotalNumberOfEmployeesUnderTheCompanyAsync(Guid companyID);
    Task<int> GetRecentHiresInTheLast30DaysAsync(Guid companyID);
=======
>>>>>>> origin/main
}
