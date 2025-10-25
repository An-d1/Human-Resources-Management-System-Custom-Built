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
                Email = customUser.Email,
                EmployeeCount = await _userRepository.GetTheTotalNumberOfEmployeesUnderTheCompanyAsync(customUser.CompanyId),
                NewHires = await _userRepository.GetRecentHiresInTheLast30DaysAsync(customUser.CompanyId),
            };

            return View(model);
        }

        // This Action loads and fills the employeeViewModel with the proper data from the database
        public async Task<IActionResult> Employees()
        {
            var currentUser = await _userManager.GetUserAsync(User); // Get the current user
            Guid adminId = currentUser.Id; // Get the current Admin Id
            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(adminId); // Get his Id so all employees related to the user can be found

            var employeesList = await _employeeRepository.ReturnAllEmployeesOfTheCompanyAsync(companyId.Value);
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

        [HttpPost]
        public async Task<IActionResult> TerminateEmployeeContract(string userId)
        {
            var currentEmployee = await _employeeRepository.FindEmployeeByIdAsync(userId);

            if (currentEmployee == null)
            {
                return NotFound();
            }

            currentEmployee.IsEmployed = false;
            currentEmployee.TerminationDate = DateOnly.FromDateTime(DateTime.Now);

            await _employeeRepository.UpdateEmployeeAsync(currentEmployee);

            return RedirectToAction("Employees");
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeDetails(string id)
        {
            // Fetch the employee (with User data included)
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("Associated user not found for this employee.");
            }

            var employee = await _employeeRepository.FindEmployeeByIdAsync(id); // This is the same instance but just the employee table
            // Build the ViewModel
            var model = new EmployeeViewModel
            {
                UserId = user.Id.ToString(), // keep it string in the ViewModel
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
                HireDate = employee.HireDate,
                TerminationDate = employee.TerminationDate,
                IsEmployed = employee.IsEmployed,
                Departments = await _employeeRepository.GetDepartmentNamesByUserIdAsync(user.Id)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployee(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("Missing user id.");
            }

            try
            {
                // Check that this employee exists first
                var employee = await _employeeRepository.FindEmployeeByIdAsync(userId);
                if (employee == null)
                {
                    return NotFound("Employee not found.");
                }

                // Delete the Identity user (this triggers cascade)
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        return BadRequest("Could not delete user account.");
                    }
                }

                TempData["SuccessMessage"] = "Employee has been successfully deleted.";

                return RedirectToAction("Employees", "Admin");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Delete failed: {ex.Message}");
            }
        }


    }

}

