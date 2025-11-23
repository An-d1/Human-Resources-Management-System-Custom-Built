using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace HRManagementSystem.Models
{
    public class EmployeeDepartmentRepository : IEmployeeDepartmentRepository
    {
        private readonly ApplicationDbContext _context;
        public EmployeeDepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AssignDepartmentAsync(Guid userId, Guid departmentId)
        {
            // Check if link already exists
            var exists = await _context.EmployeeDepartments
                .AnyAsync(ed => ed.UserId == userId && ed.DepartmentId == departmentId);

            if (exists)
                return;

            var link = new EmployeeDepartment
            {
                UserId = userId,
                DepartmentId = departmentId
            };

            _context.EmployeeDepartments.Add(link);
            await _context.SaveChangesAsync();
        }

    }
}
