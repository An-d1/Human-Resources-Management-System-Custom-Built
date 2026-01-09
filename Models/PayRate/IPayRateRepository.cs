namespace HRManagementSystem.Models
{
    public interface IPayRateRepository
    {
        Task<PayRate?> GetActivePayRateAsync(Guid userId);
        Task AddPayRateAsync(PayRate payRate);
        Task UpdatePayRateAsync(PayRate payRate);
        Task<PayRate?> GetPayRateForDateAsync(Guid userId, DateOnly date);

    }
}
