using HRManagementSystem.Data;
using HRManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Models
{
    public class LeaveRuleRepository : ILeaveRuleRepository
    {
        private readonly ApplicationDbContext _context;
        public LeaveRuleRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<LeaveRule?> GetRuleByLevelIdAsync(Guid levelId)
        {
            return await _context.LeaveRules
                .FirstOrDefaultAsync(r => r.EmployeeLevelId == levelId);
        }

        public async Task<LeaveRule> AddRuleAsync(LeaveRule rule)
        {
            await _context.LeaveRules.AddAsync(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task UpdateRuleAsync(LeaveRule rule)
        {
            _context.LeaveRules.Update(rule);
            await Task.CompletedTask;
        }
        public async Task<bool> DeleteRuleByLevelIdAsync(Guid levelId)
        {
            var rule = await _context.LeaveRules.FirstOrDefaultAsync(r => r.EmployeeLevelId == levelId);
            if (rule == null) return true;

            _context.LeaveRules.Remove(rule);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<LeaveRule>> GetRulesByCompanyAsync(Guid companyId)
        {
            return await _context.LeaveRules
                .Where(r => r.CompanyId == companyId)
                .ToListAsync();
        }
    }
}
