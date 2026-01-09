using HRManagementSystem.Enums;
using HRManagementSystem.Models;
using HRManagementSystem.Repositories;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeDepartmentRepository _employeeDepartmentRepository;
        private readonly IPayRateRepository _payRateRepository;

        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public AdminController(UserManager<User> userManager,
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            IDepartmentRepository departmentRepository,
            IEmployeeDepartmentRepository employeeDepartmentRepository,
            ILeaveRequestRepository leaveRequestRepository,
            IPayRateRepository payRateRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _departmentRepository = departmentRepository;
            _employeeDepartmentRepository = employeeDepartmentRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _payRateRepository = payRateRepository;
        }
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            var customUser = await _userManager.FindByIdAsync(userId);

            var model = new AdminViewModel
            {
                FirstName = customUser.FirstName,
                Email = customUser.Email,
                DepartmentsCount = await _departmentRepository.GetTheTotalNumberOfDepartmentsUnderTheCompanyAsync(customUser.CompanyId),
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

            var currentActivePayRate = await _payRateRepository.GetActivePayRateAsync(currentUser.Id);

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
                    IsEmailConfirmed = user.EmailConfirmed,
                    Phone = user.PhoneNumber,
                    Role = roles.FirstOrDefault(),
                    Departments = await _employeeRepository.GetDepartmentNamesByUserIdAsync(user.Id),
                    HireDate = employee.HireDate,
                    TerminationDate = employee.TerminationDate,
                    IsEmployed = employee.IsEmployed,
                    EmployeeLevel = employee.EmployeeLevel?.Name ?? "N/A",
                    CurrentPayRate = currentActivePayRate?.HourlyRate,
                    PayRateEffectiveFrom = currentActivePayRate?.EffectiveFrom,
                    PayRateEffectiveTo = currentActivePayRate?.EffectiveTo
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

            TempData["SuccessMessage"] = "Employees contract has been terminated.";

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
            var currentActivePayRate = await _payRateRepository.GetActivePayRateAsync(employee.Id);

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
                Departments = await _employeeRepository.GetDepartmentNamesByUserIdAsync(user.Id),
                EmployeeLevel = employee.EmployeeLevel?.Name ?? "N/A",
                CurrentPayRate = currentActivePayRate?.HourlyRate,
                PayRateEffectiveFrom = currentActivePayRate?.EffectiveFrom,
                PayRateEffectiveTo = currentActivePayRate?.EffectiveTo

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
                // Checks that this employee exists first
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
                }
                ;

                await _departmentRepository.AddDepartmentWithAdminsCompanyIdAsync(department);

                TempData["SuccessMessage"] =
               "Department was successfuly created.";

                return RedirectToAction("DepartmentsList");
            }

            return View(departmentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var result = await _departmentRepository.DeleteAsync(id);

            if (!result)
                return NotFound();

            TempData["SuccessMessage"] =
                "Department was successfuly deleted.";

            return RedirectToAction("DepartmentsList");
        }

        // Code to assign employees to department

        [HttpGet]
        public async Task<IActionResult> AssignEmployeeToDepartment(Guid userId)
        {
            var currentUser = await _userManager.GetUserAsync(User); // Get the current user

            var employee = await _userManager.FindByIdAsync(userId.ToString());
            var departments = await _departmentRepository.GetAllDepartmentsOfTheCompanyAsync(currentUser.CompanyId);

            var vm = new AssignDepartmentViewModel
            {
                UserId = userId,
                EmployeeFullName = $"{employee.FirstName} {employee.LastName}",
                Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList()
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> AssignEmployeeToDepartment(AssignDepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _employeeDepartmentRepository.AssignDepartmentAsync(model.UserId, model.SelectedDepartmentId);

            TempData["SuccessMessage"] =
                "Employee was asigned to the chosen department.";

            return RedirectToAction("Employees", new { id = model.UserId });
        }

        // Code for the requested leaves goes down here!

        public async Task<IActionResult> RequestedLeaves()
        {
            // Get the current logged-in Admin user to retrieve their CompanyId
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Fetch both Approved and Pending requests filtered by CompanyId
            // Include(l => l.User) ensures we have name data
            var requests = await _leaveRequestRepository
                .GetApprovedAndPendingByCompanyAsync(user.CompanyId);

            // map to the ViewModel
            var viewModel = requests.Select(l => new LeaveRequestViewModel
            {
                RequestId = l.Id,
                EmployeeFirstName = l.User?.FirstName ?? "Unknown",
                EmployeeLastName = l.User?.LastName ?? "User",
                LeaveType = l.LeaveType,
                LeaveStatus = l.Status,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(Guid requestId)
        {
            var admin = await _userManager.GetUserAsync(User);
            var request = await _leaveRequestRepository.GetByIdAsync(requestId);

            // Security, ensures request exists and belongs to the same company
            if (request == null || request.CompanyId != admin.CompanyId)
            {
                return NotFound();
            }

            request.Status = LeaveStatus.Approved;
            await _leaveRequestRepository.UpdateAsync(request);

            TempData["SuccessMessage"] = "Leave request has been approved.";
            return RedirectToAction(nameof(RequestedLeaves));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(Guid requestId)
        {
            var admin = await _userManager.GetUserAsync(User);

            var request = await _leaveRequestRepository.GetByIdAsync(requestId);

            // Security, ensures request exists and belongs to the same company

            if (request == null || request.CompanyId != admin.CompanyId)
            {
                return NotFound();
            }

            request.Status = LeaveStatus.Rejected;
            await _leaveRequestRepository.UpdateAsync(request);

            TempData["ErrorMessage"] = "Leave request has been rejected.";
            return RedirectToAction(nameof(RequestedLeaves));
        }

        // Code for adding a new payRate goes here

        [HttpGet]
        public async Task<IActionResult> AddPayRate(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return NotFound();

            var model = new AddPayRateViewModel
            {
                UserId = user.Id.ToString(),
                FullName = $"{user.FirstName} {user.LastName}",
                EffectiveDate = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPayRate(AddPayRateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var employee = await _employeeRepository.FindEmployeeByIdAsync(model.UserId);
            if (employee == null) return NotFound();

            // Get the current active rate using repo
            var activePayRate = await _payRateRepository.GetActivePayRateAsync(employee.Id);

            // Logic: If active rate exists, close it
            if (activePayRate != null)
            {
                // Prevent new rate from starting before the old one
                if (activePayRate.EffectiveFrom >= DateOnly.FromDateTime(model.EffectiveDate))
                {
                    ModelState.AddModelError("EffectiveDate", "New rate cannot start before the current active rate.");
                    return View(model);
                }

                // Close the old rate (set End Date to yesterday)
                activePayRate.EffectiveTo = DateOnly.FromDateTime(model.EffectiveDate.AddDays(-1));

                // Save 
                await _payRateRepository.UpdatePayRateAsync(activePayRate);
            }

            // Create the new PayRate instance
            var newPayRate = new PayRate
            {
                EmployeeId = employee.Id,
                HourlyRate = model.HourlyRate,
                EffectiveFrom = DateOnly.FromDateTime(model.EffectiveDate),
                EffectiveTo = null // Null means it is the new active one
            };

            // Add to DB
            await _payRateRepository.AddPayRateAsync(newPayRate);

            TempData["SuccessMessage"] = $"Pay rate updated for {model.FullName}";
            return RedirectToAction("EmployeeDetails", new { id = model.UserId });
        }

    }
}



