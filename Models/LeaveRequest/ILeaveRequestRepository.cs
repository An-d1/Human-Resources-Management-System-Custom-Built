using HRManagementSystem.Enums;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Models
{
    public interface ILeaveRequestRepository
    {
        Task<List<LeaveRequest>> GetOutgoingRequestsAsync(Guid userId);
        Task CreateAsync(LeaveRequest leaveRequest);

        Task DeleteAsync(LeaveRequest leaveReq);
        Task<IEnumerable<LeaveRequest>> GetByUserAsync(Guid userId);

        Task<IEnumerable<LeaveRequest>> GetPendingByCompanyAsync(Guid companyId);

        Task<IEnumerable<LeaveRequest>> GetApprovedAndPendingByCompanyAsync(Guid companyId);

        Task<LeaveRequest?> GetByIdAsync(Guid id);

        Task UpdateAsync(LeaveRequest leaveRequest);

        Task<bool> HasOverlappingLeaveAsync(
            Guid userId,
            DateTime? startDate,
            DateTime? endDate);
    }
}
