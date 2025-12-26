using HRManagementSystem.Enums;
using HRManagementSystem.Models;
using HRManagementSystem.Repositories;
using HRManagementSystem.Services;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HRManagementSystem.Controllers
{

    [Authorize(Roles = "Employee")]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        private readonly ILeaveRuleRepository _leaveRuleRepository;
        private readonly IEmployeeLevelRepository _employeeLevelRepository;

        private readonly IEmployeeRepository _employeeRepository;

        private readonly CalculateLeaveDays _leaveCalculator;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager,
            ITimeEntryRepository timeEntryRepository, 
            ILeaveRequestRepository leaveRequestRepository, 
            ILeaveRuleRepository leaveRuleRepository, 
            IEmployeeLevelRepository employeeLevelRepository, 
            IEmployeeRepository employeeRepository,
            CalculateLeaveDays leaveCalculator)
        {
            _logger = logger;
            _userManager = userManager;
            _timeEntryRepository = timeEntryRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveRuleRepository = leaveRuleRepository;
            _employeeLevelRepository = employeeLevelRepository;
            _employeeRepository = employeeRepository;
            _leaveCalculator = leaveCalculator;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var employee = new EmployeeViewModel
            {
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                Email = user.Email
            };

            return View(employee);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CheckInOut()
        {
            var user = await _userManager.GetUserAsync(User);

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var todayEntries = await _timeEntryRepository.GetEntriesForTodayAsync(user.Id);

            bool isCheckedIn = todayEntries.Any(t => t.ClockOutUtc == null);

            var model = new CheckInViewModel
            {
                IsCheckedIn = isCheckedIn,
                TodayEntries = todayEntries
            };

            return View(model);
        }

        public async Task<IActionResult> CheckInEmployee()
        {
            var user = await _userManager.GetUserAsync(User);

            bool success = await _timeEntryRepository.CheckInEmployeeAsync(user.Id);

            if (!success) // checks wether it went well
                return BadRequest("You are already checked in.");

            return RedirectToAction("CheckInOut");
        }

        public async Task<IActionResult> CheckOutEmployee()
        {
            var user = await _userManager.GetUserAsync(User);

            bool success = await _timeEntryRepository.CheckOutEmployeeAsync(user.Id);

            if (!success) // checks wether it went well
                return BadRequest("You are not checked in.");

            return RedirectToAction("CheckInOut");
        }

        //Method to populate the max laves and annual leaves used for the employee view
        async Task PopulateLeaveStatsAsync(LeaveRequestViewModel model, User user)
        {
            var employee = await _employeeRepository
                .FindEmployeeByIdAsync(user.Id.ToString());

            int year = model.SelectedYear ?? DateTime.Now.Year;

            if (employee?.EmployeeLevelId != null)
            {
                var rule = await _leaveRuleRepository
                    .GetRuleByLevelIdAsync(employee.EmployeeLevelId.Value);

                if (rule != null)
                {
                    model.MaxAnnualLeaves = rule.MaxAnnualLeaves;
                    model.UsedAnnualLeaves = await _leaveRequestRepository
                        .GetTotalDaysUsedInYearAsync(user.Id, year);
                }
            }

            model.OutgoingRequests =
                await _leaveRequestRepository.GetOutgoingRequestsAsync(user.Id);
        }

        [HttpGet]
        public async Task<IActionResult> LeaveRequest(int? selectedYear) //get from query string
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var employee = await _employeeRepository.FindEmployeeByIdAsync(user.Id.ToString());

            int usedDays = 0;
            int maxDays = 0;

            int year = selectedYear ?? DateTime.Now.Year;

            if (employee?.EmployeeLevelId != null)
            {
                var rule = await _leaveRuleRepository
                    .GetRuleByLevelIdAsync(employee.EmployeeLevelId.Value);

                if (rule != null)
                {
                    maxDays = rule.MaxAnnualLeaves;
                    usedDays = await _leaveRequestRepository
                        .GetTotalDaysUsedInYearAsync(user.Id, year);
                }
            }

            var model = new LeaveRequestViewModel
            {
                OutgoingRequests = await _leaveRequestRepository
                    .GetOutgoingRequestsAsync(user.Id),
                SelectedYear = year,
                UsedAnnualLeaves = usedDays,
                MaxAnnualLeaves = maxDays
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveRequest(LeaveRequestViewModel model)
        {
           
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }
            var employee = await _employeeRepository.FindEmployeeByIdAsync(user.Id.ToString());

            if (model.StartDate < DateTime.Now) // to be commented out in case of testing
            {
                ModelState.AddModelError(nameof(model.StartDate),
                    "Start date can't be on the past.");
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            if (!ModelState.IsValid)
            {
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError(nameof(model.EndDate),
                    "End date must be after start date.");
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            var hasOverlap = await _leaveRequestRepository
                .HasOverlappingLeaveAsync(user.Id, model.StartDate, model.EndDate);

            if (hasOverlap)
            {
                ModelState.AddModelError(nameof(model.Reason),
                    "You already have a leave request during this period.");
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            // 1 Get the Rule for the employee level
            if (employee?.EmployeeLevelId == null)
            {
                ModelState.AddModelError("", "Your employee level is not configured.");
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            var rule = await _leaveRuleRepository.GetRuleByLevelIdAsync(employee.EmployeeLevelId.Value);

            if (rule == null)
            {
                ModelState.AddModelError("", "No leave policy found for your role.");
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            int requestedDays = _leaveCalculator.CalculateDaysInYear(
                model.StartDate!.Value,
                model.EndDate!.Value,
                model.StartDate.Value.Year,
                rule.ExcludedDays
            ); // this is used to calculate only the days that fall within that year of the request

            int totalTripDays = _leaveCalculator.CalculateActualLeaveDays(
            model.StartDate.Value,
            model.EndDate.Value,
            rule.ExcludedDays
            ); //this is used to save the actual amount of days used per year

            // 2 Verify balance (Summing the CalculatedDays column)
            int yearUsage = await _leaveRequestRepository.GetTotalDaysUsedInYearAsync(user.Id, DateTime.Now.Year);

            if (yearUsage + requestedDays > rule.MaxAnnualLeaves)
            {
                ModelState.AddModelError("", $"Limit exceeded. Remaining balance: {rule.MaxAnnualLeaves - yearUsage} days.");
                await PopulateLeaveStatsAsync(model, user);
                return View("LeaveRequest", model);
            }

            // 3 Create the request
            var request = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyId = user.CompanyId,
                StartDate = model.StartDate.Value,
                EndDate = model.EndDate.Value,
                LeaveType = model.LeaveType!.Value,
                Status = LeaveStatus.Pending,
                CalculatedDays = totalTripDays, // Precalculated for performance
                Reason = model.Reason,
                RequestedAt = DateTime.UtcNow
            };

            await _leaveRequestRepository.CreateAsync(request);

            TempData["SuccessMessage"] =
                "Your leave request has been submitted for approval.";

            return RedirectToAction("Index", "Home");
        }
    }
}
