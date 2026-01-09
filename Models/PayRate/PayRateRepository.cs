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
        public async Task<PayRate?> GetActivePayRateAsync(Guid employeeId)
        {
            return await _context.PayRates
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.EffectiveTo == null);
        }
        public async Task AddPayRateAsync(PayRate payRate)
        {
            _context.PayRates.Add(payRate);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePayRateAsync(PayRate payRate)
        {
            _context.PayRates.Update(payRate);
            await _context.SaveChangesAsync();
        }
        public async Task<PayRate?> GetPayRateForDateAsync(Guid employeeId, DateOnly date) 
        {
            return await _context.PayRates
                .Where(r =>
                    r.EmployeeId == employeeId &&
                    r.EffectiveFrom <= date &&
                    (r.EffectiveTo == null || r.EffectiveTo >= date)
                )
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefaultAsync();
        }

    }
}
