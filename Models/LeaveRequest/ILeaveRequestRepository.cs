namespace HRManagementSystem.Models
{
    public interface ILeaveRequestRepository
    {
        Task CreateAsync(LeaveRequest leaveRequest);

        Task<IEnumerable<LeaveRequest>> GetByUserAsync(Guid userId);

        Task<IEnumerable<LeaveRequest>> GetPendingByCompanyAsync(Guid companyId);

        Task<LeaveRequest?> GetByIdAsync(Guid id);

        Task UpdateAsync(LeaveRequest leaveRequest);

        Task<bool> HasOverlappingLeaveAsync(
            Guid userId,
            DateTime startDate,
            DateTime endDate);
    }
}
