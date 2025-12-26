using HRManagementSystem.Data;
using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;


namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]

    public class EmployeeLevelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        private readonly IEmployeeLevelRepository _employeeLevelRepository;
        private readonly ILeaveRuleRepository _leaveRuleRepository;
        public EmployeeLevelController(ApplicationDbContext context,
            UserManager<User> userManager,
            IUserRepository userRepository,
            IEmployeeLevelRepository employeeLevelRepository,
            ILeaveRuleRepository leaveRuleRepository)
        {
            _context = context;
            _userManager = userManager;
            _userRepository = userRepository;
            _employeeLevelRepository = employeeLevelRepository;
            _leaveRuleRepository = leaveRuleRepository;
        }
        [HttpGet]
        public IActionResult CreateEmployeeLevelView()
        {
            var model = new CreateEmployeeLevelViewModel
            {
                WeekDays = Enum.GetValues(typeof(DayOfWeek))
           .Cast<DayOfWeek>()
           .Select(d => new SelectListItem
           {
               Text = d.ToString(),
               Value = d.ToString()
           })
           .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeLevelView(CreateEmployeeLevelViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(currentUser.Id);

            if (ModelState.IsValid)
            {
                //  transaction to ensure atomic behaviour, so if one fails to be created both FAIL
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Create the Level
                    var employeeLevel = new EmployeeLevel
                    {
                        Name = model.Name,
                        CompanyId = companyId,
                    };

                    var level = await _employeeLevelRepository.AddLevelAsync(employeeLevel);

                    // If the repository returns null, something went wrong
                    if (level == null) throw new Exception("Level creation failed");

                    // Create the Leave Rule
                    var leaveRule = new LeaveRule
                    {
                        CompanyId = companyId,
                        EmployeeLevelId = level.Id, 
                        MaxAnnualLeaves = model.MaxAnnualLeave,
                        ExcludedDays = model.ExcludedDays != null ? string.Join(",", model.ExcludedDays) : string.Empty
                    };

                    var rule = await _leaveRuleRepository.AddRuleAsync(leaveRule);

                    if (rule == null) throw new Exception("Leave Rule creation failed");

                    // Commit: If we reach here, both succeeded
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Employee level and leave rules created successfully.";
                    return RedirectToAction("CreateEmployeeLevelView");
                }
                catch (Exception ex)
                {
                    // If any error occurs, the transaction is automatically rolled back 
                    // when the using block ends without a Commit.
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EmployeeLevelList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return NotFound();

            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(currentUser.Id);

            var levels = await _employeeLevelRepository.GetLevelsByCompanyAsync(companyId);

            var model = new List<EmployeeLevelViewModel>();

            foreach (var level in levels)
            {
                var rule = await _leaveRuleRepository.GetRuleByLevelIdAsync(level.Id);

                model.Add(new EmployeeLevelViewModel
                {
                    Id = level.Id,
                    Name = level.Name,
                    MaxAnnualLeave = rule?.MaxAnnualLeaves ?? 0,
                    ExcludedDays = !string.IsNullOrEmpty(rule?.ExcludedDays)
                        ? rule.ExcludedDays.Split(',').ToList()
                        : new List<string>()
                });
            }

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteEmployeeLevel(Guid id)
        {
            // Start the transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Delete the Leave Rule first (to avoid foreign key constraint issues)
                var ruleDeleted = await _leaveRuleRepository.DeleteRuleByLevelIdAsync(id);

                // Delete the Employee Level
                var levelDeleted = await _employeeLevelRepository.DeleteLevelAsync(id);

                if (levelDeleted)
                {
                    // Confirm all changes to the DB
                    await transaction.CommitAsync();
                    TempData["SuccessMessage"] = "Employee Level and associated rules deleted successfully.";
                }
                else
                {
                    throw new Exception("Could not find the Employee Level to delete.");
                }
            }
            catch (Exception ex)
            {
                // If anything fails, nothing is deleted from the DB
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Error during deletion: " + ex.Message;
            }

            return RedirectToAction("EmployeeLevelList");
        }
    }
}

