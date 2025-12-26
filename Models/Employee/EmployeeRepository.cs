using HRManagementSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HRManagementSystem.Models
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;
        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Employee> FindEmployeeByIdAsync(string employeeId)
        {
            return await _context.Employees
                .Include(e => e.EmployeeLevel)
                .FirstOrDefaultAsync(e => e.UserId.ToString() == employeeId);
        }
        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteEmployeeAsync(Employee employee)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }


        public async Task<Employee> CreateAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<List<Employee>> ReturnAllEmployeesOfTheCompanyAsync(Guid companyId)
        {
            return await _context.Employees
            .Include(e => e.EmployeeLevel)
            .Where(e => e.User.CompanyId == companyId)
            .ToListAsync();
        }

        public async Task<List<string>> GetDepartmentNamesByUserIdAsync(Guid userId)
        {
            return await _context.EmployeeDepartments
           .Where(ed => ed.UserId == userId)
           .Include(ed => ed.Department) 
           .Select(ed => ed.Department.Name)
           .ToListAsync();
        }

        public async Task SetIsEmployedToTrueFromUserId(Guid userId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employee != null)
            {
                //Update the property
                employee.IsEmployed = true;

                await _context.SaveChangesAsync();
            }
        }

    }
}
