namespace HRManagementSystem.Models
{
    public interface IPayRateRepository
    {
        Task<PayRate?> GetPayRateForDateAsync(Guid userId, DateTime date);
    }
}
