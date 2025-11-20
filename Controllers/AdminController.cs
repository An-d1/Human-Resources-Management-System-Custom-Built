using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;


        private readonly IEmployeeRepository _employeeRepository;
        public AdminController(UserManager<User> userManager, IUserRepository userRepository, IEmployeeRepository employeeRepository, IDepartmentRepository departmentRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
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
            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(currentUser.Id); // Get his company Id so all employees related to the user can be found

            var employeesList = await _employeeRepository.ReturnAllEmployeesOfTheCompanyAsync(companyId);
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
                    IsEmployed = employee.IsEmployed,
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
                UserId = user.Id.ToString(), // string
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

        // here goes the code for the department

        public async Task<IActionResult> DepartmentsList() // to show all the departments , view
        {
            var currentUser = await _userManager.GetUserAsync(User); // Get the current user
            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(currentUser.Id); // Get his company Id so all departments related to the user can be found

            var departmentsList = await _departmentRepository.GetAllDepartmentsOfTheCompanyAsync(companyId);
            var modelList = new List<DepartmentViewModel>();

            foreach (var department in departmentsList)
            {

                var model = new DepartmentViewModel
                {
                    DepartmentId = department.Id.ToString(),
                    DepartmentName = department.Name,
                };

                modelList.Add(model);
            }

            return View(modelList);
        }

        [HttpGet]
        public IActionResult AddDepartmentView()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddDepartment(AddDepartmentViewModel departmentViewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User); // Get the current user

            if (currentUser == null)
            {
                return Challenge(); // forces them to log in again
            }

            Guid adminsCompanyId = await _userRepository.GetCompanyIdByUserIdAsync(currentUser.Id); // Get his company Id so all employees related to the user can be found

            if (ModelState.IsValid)
            {
                Department department = new Department();
                {
                    department.Id = new Guid();
                    department.Name = departmentViewModel.DepartmentName;
                    department.CompanyId = adminsCompanyId;
                };

                await _departmentRepository.AddDepartmentWithAdminsCompanyIdAsync(department);

                return RedirectToAction("Index");
            }

            return View(departmentViewModel);
        }
    }

}

