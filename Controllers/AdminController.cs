using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IUserRepository _userRepository;

        private readonly IEmployeeRepository _employeeRepository;
        public AdminController(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager, IUserRepository userRepository, IEmployeeRepository employeeRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
        }
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            var customUser = await _userManager.FindByIdAsync(userId);
            
            var model = new AdminViewModel
            {
                FirstName = customUser.FirstName,
                EmployeeCount = await _userRepository.GetTheTotalNumberOfEmployeesUnderTheCompanyAsync(customUser.CompanyId),
                NewHires = await _userRepository.GetRecentHiresInTheLast30DaysAsync(customUser.CompanyId),
            };

            return View(model);
        }

        public async Task<IActionResult> Employees()
        {
            var employeesList = await _employeeRepository.ReturnAllEmployeesAsync();
            var modelList = new List<EmployeeViewModel>();

            foreach (var employee in employeesList)
            {
                var user = await _userManager.FindByIdAsync(employee.UserId.ToString());
                if (user == null)
                    continue;

                var roles = await _userManager.GetRolesAsync(user);

                var model = new EmployeeViewModel
                {
                    UserId = user.Id.ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Role = roles.FirstOrDefault(),
                    Departments = await _employeeRepository.GetDepartmentNamesByUserIdAsync(user.Id),
                    HireDate = employee.HireDate,
                    TerminationDate = employee.TerminationDate,
                    IsEmployed = employee.IsEmployed
                };

                modelList.Add(model);
            }
            return View(modelList);
        }

    }
}
