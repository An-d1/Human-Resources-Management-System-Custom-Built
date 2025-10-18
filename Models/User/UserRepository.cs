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
        public async Task<Guid?> GetCompanyIdByUserIdAsync(Guid userId) // The id of the admin that is creating the user
        {
            var companyId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.CompanyId)
                .FirstOrDefaultAsync();

            return companyId == Guid.Empty ? null : companyId;
        }
    }

