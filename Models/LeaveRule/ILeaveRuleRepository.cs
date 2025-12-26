namespace HRManagementSystem.Models
{
    public interface ILeaveRuleRepository
    {
        Task<LeaveRule?> GetRuleByLevelIdAsync(Guid levelId);
        Task<LeaveRule> AddRuleAsync(LeaveRule rule);
        Task UpdateRuleAsync(LeaveRule rule);
        Task<bool> DeleteRuleByLevelIdAsync(Guid levelId);

        Task<List<LeaveRule>> GetRulesByCompanyAsync(Guid companyId);
    }
}
