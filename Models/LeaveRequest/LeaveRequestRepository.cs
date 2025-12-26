using HRManagementSystem.Data;
using HRManagementSystem.Enums;
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

        public async Task<List<LeaveRequest>> GetOutgoingRequestsAsync(Guid userId)
        {
            return await _context.LeaveRequests
                .Where(l =>
                    l.UserId == userId &&
                    l.EndDate >= DateTime.Today)
                .OrderBy(l => l.StartDate)
                .ToListAsync();
        }


        public async Task CreateAsync(LeaveRequest leaveRequest)
        {
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LeaveRequest leaveReq)
        {
            _context.LeaveRequests.Remove(leaveReq);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LeaveRequest>> GetByUserAsync(Guid userId)
        {
            return await _context.LeaveRequests
                .Include(l => l.User)
                .Where(l => l.UserId == userId &&
                            (l.Status == LeaveStatus.Pending ||
                             l.Status == LeaveStatus.Approved))
                .OrderBy(l => l.StartDate)
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
        public async Task<IEnumerable<LeaveRequest>> GetApprovedAndPendingByCompanyAsync(Guid companyId)
        {
            return await _context.LeaveRequests
                .Include(l => l.User)
                .Where(l =>
                    l.CompanyId == companyId &&
                    (l.Status == LeaveStatus.Approved ||
                     l.Status == LeaveStatus.Pending))
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
            DateTime? startDate,
            DateTime? endDate)
        {
            return await _context.LeaveRequests.AnyAsync(l =>
                l.UserId == userId &&
                l.Status != Enums.LeaveStatus.Rejected &&
                startDate <= l.EndDate &&
                endDate >= l.StartDate);
        }
        public async Task<int> GetTotalDaysUsedInYearAsync(Guid userId, int year)
        {
            // sums up all used leaves on a year where leavetype == annual 
            return await _context.LeaveRequests
                .Where(l =>
                    l.UserId == userId &&
                    l.LeaveType == LeaveType.Annual &&
                    l.StartDate.Year == year &&
                    (l.Status == LeaveStatus.Approved || l.Status == LeaveStatus.Pending))
                .SumAsync(l => l.CalculatedDays);
        }

        public async Task<List<LeaveRequest>> GetApprovedRequestsOverlappingPeriodAsync(Guid userId, DateTime periodStart, DateTime periodEnd)
        {
            return await _context.LeaveRequests
                .Include(l => l.User) 
                .Where(l =>
                    l.UserId == userId &&
                    l.Status == Enums.LeaveStatus.Approved &&
                    // the overlap logic:
                    // Request starts before the period ends
                    l.StartDate <= periodEnd &&
                    // and request ends after the period starts
                    l.EndDate >= periodStart
                )
                .ToListAsync();
        }
    }
}
