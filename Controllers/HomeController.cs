using HRManagementSystem.Enums;
using HRManagementSystem.Models;
using HRManagementSystem.Repositories;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HRManagementSystem.Controllers
{
    
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, ITimeEntryRepository timeEntryRepository, ILeaveRequestRepository leaveRequestRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _timeEntryRepository = timeEntryRepository;
            _leaveRequestRepository = leaveRequestRepository;
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

        public IActionResult LeaveRequest()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLeave(LeaveRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (model.StartDate > model.EndDate)
            {
                ModelState.AddModelError(nameof(model.EndDate),
                    "End date must be after start date.");
                return View(model);
            }

            var hasOverlap = await _leaveRequestRepository
                .HasOverlappingLeaveAsync(user.Id, model.StartDate, model.EndDate);

            if (hasOverlap)
            {
                ModelState.AddModelError(string.Empty,
                    "You already have a leave request during this period.");
                return View(model);
            }

            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CompanyId = user.CompanyId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                LeaveType = model.LeaveType,
                Status = LeaveStatus.Pending,
                Reason = model.Reason,
                RequestedAt = DateTime.UtcNow
            };

            await _leaveRequestRepository.CreateAsync(leaveRequest);

            TempData["SuccessMessage"] =
                "Your leave request has been submitted for approval.";

            return RedirectToAction("Index", "Home");
        }



    }
}
