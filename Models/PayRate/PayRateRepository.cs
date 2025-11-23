using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Models
{
    public class PayRateRepository : IPayRateRepository 
    {
        private readonly ApplicationDbContext _context;
        public PayRateRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PayRate?> GetPayRateForDateAsync(Guid userId, DateTime date)
        {
            return await _context.PayRates
                .Where(r =>
                    r.UserId == userId &&
                    r.EffectiveFrom <= DateOnly.FromDateTime(date) &&
                    r.EffectiveTo >= date
                )
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

    }
}
