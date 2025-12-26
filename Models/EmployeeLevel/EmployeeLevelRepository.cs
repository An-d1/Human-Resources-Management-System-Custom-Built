using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Models
{
    public class EmployeeLevelRepository : IEmployeeLevelRepository
    {
        private readonly ApplicationDbContext _context;
        public EmployeeLevelRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<EmployeeLevel>> GetLevelsByCompanyAsync(Guid companyId)
        {
            return await _context.EmployeesLevel
                .Where(l => l.CompanyId == companyId)
                .ToListAsync();
        }

        public async Task<EmployeeLevel?> GetLevelByIdAsync(Guid levelId)
        {
            return await _context.EmployeesLevel
                .FirstOrDefaultAsync(l => l.Id == levelId);
        }

        public async Task<EmployeeLevel> AddLevelAsync(EmployeeLevel level)
        {
            await _context.EmployeesLevel.AddAsync(level);
            await _context.SaveChangesAsync();
            return level;
        }

        public async Task<bool> DeleteLevelAsync(Guid id)
        {
            var level = await _context.EmployeesLevel.FindAsync(id);
            if (level == null) return false;

            _context.EmployeesLevel.Remove(level);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
