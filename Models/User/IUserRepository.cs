namespace HRManagementSystem.Models;

public interface IUserRepository 
{
    Task<Guid?> GetCompanyIdByUserIdAsync(Guid userId);
}
