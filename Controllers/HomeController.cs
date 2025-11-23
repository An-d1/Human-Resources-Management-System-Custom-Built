using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HRManagementSystem.Controllers
{
    
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<HomeController> _logger;

        private readonly ITimeEntryRepository _timeEntryRepository;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, ITimeEntryRepository timeEntryRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _timeEntryRepository = timeEntryRepository;
        }

        public IActionResult Index()
        {
            return View();
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


    }
}
