using HRManagementSystem.Data;
using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetByUserAsync(Guid userId)
        {
            return await _context.LeaveRequests
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetPendingByCompanyAsync(Guid companyId)
        {
            return await _context.LeaveRequests
                .Include(l => l.User)
                .Where(l =>
                    l.CompanyId == companyId &&
                    l.Status == Enums.LeaveStatus.Pending)
                .OrderBy(l => l.StartDate)
                .ToListAsync();
        }

        public async Task<LeaveRequest?> GetByIdAsync(Guid id)
        {
            return await _context.LeaveRequests
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task UpdateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Update(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasOverlappingLeaveAsync(
            Guid userId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.LeaveRequests.AnyAsync(l =>
                l.UserId == userId &&
                l.Status != Enums.LeaveStatus.Rejected &&
                startDate <= l.EndDate &&
                endDate >= l.StartDate);
        }
    }
}
