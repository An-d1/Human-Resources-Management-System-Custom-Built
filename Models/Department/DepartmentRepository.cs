using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace HRManagementSystem.Models
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;
        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Department>> GetAllDepartmentsOfTheCompanyAsync(Guid companyId)
        {
            return await _context.Departments
                .Where(d => d.CompanyId == companyId) 
                .ToListAsync();
        }
        public async Task<int> GetTheTotalNumberOfDepartmentsUnderTheCompanyAsync(Guid companyId)
        {
            return await _context.Departments
            .CountAsync(d => d.CompanyId == companyId);
        }

        public async Task<Department> GetByIdAsync(Guid guid)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == guid);
        }
        public async Task AddDepartmentWithAdminsCompanyIdAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return false;

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
