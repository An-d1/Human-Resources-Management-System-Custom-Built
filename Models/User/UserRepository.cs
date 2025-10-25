using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace HRManagementSystem.Models;

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetTheTotalNumberOfEmployeesUnderTheCompanyAsync(Guid companyId)
        {
            return await _context.Users
            .CountAsync(u => u.CompanyId == companyId);
        }

        public async Task<int> GetRecentHiresInTheLast30DaysAsync(Guid companyId)
        {
            var dateThreshold = DateTime.UtcNow.AddDays(-30); // returns the date/time 30 days ago

            return await _context.Users
                .CountAsync(u => u.CompanyId == companyId && u.CreatedAt >= dateThreshold);
        }

        public async Task<Guid?> GetCompanyIdByUserIdAsync(Guid userId) // The id of the admin that is creating the user
        {
            var companyId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.CompanyId)
                .FirstOrDefaultAsync();

            return companyId == Guid.Empty ? null : companyId;
        }
    }

