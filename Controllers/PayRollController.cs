using HRManagementSystem.Models;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class PayRollController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IPayRateRepository _payRateRepository;
        private readonly IHolidayCalendarRepository _holidayCalendarRepository;
        private readonly IEmployeeRepository _employeeRepository;


        public PayRollController(UserManager<User> userManager, IUserRepository userRepository,ITimeEntryRepository timeEntryRepository, IPayRateRepository payRateRepository, IHolidayCalendarRepository holidayCalendarRepository, IEmployeeRepository employeeRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _timeEntryRepository = timeEntryRepository;
            _payRateRepository = payRateRepository;
            _holidayCalendarRepository = holidayCalendarRepository;
            _employeeRepository = employeeRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PayRollEmployees()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var companyId = await _userRepository.GetCompanyIdByUserIdAsync(currentUser.Id);

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
                    IsEmailConfirmed = user.EmailConfirmed,
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
        public async Task<IActionResult> Calculate(Guid userId)
        {
            var admin = await _userManager.GetUserAsync(User);
            var companyId = admin.CompanyId;

            var from = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var to = from.AddMonths(1);

            // Calculate the payroll using your existing logic
            decimal total = await CalculatePayrollAsync(userId, companyId, from, to);

            // Prepare modal
            ViewBag.ShowPayrollModal = true;
            ViewBag.Total = total.ToString("F2");

            // RELOAD ALL EMPLOYEES for the same page
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
                    Role = roles.FirstOrDefault(),
                    Departments = await _employeeRepository.GetDepartmentNamesByUserIdAsync(user.Id),
                    IsEmployed = employee.IsEmployed
                };

                modelList.Add(model);
            }

            return View("PayRollEmployees", modelList);
        }

        public List<(DateTime Start, DateTime End)> SplitEntryByDay(DateTime start, DateTime end)
        {
            var segments = new List<(DateTime Start, DateTime End)>();

            var current = start;

            while (current.Date < end.Date)
            {
                // End of the current day (midnight)
                var endOfDay = current.Date.AddDays(1);

                // Add segment till midnight
                segments.Add((current, endOfDay));

                // Move to the start of next day
                current = endOfDay;
            }

            // Final segment (remaining time on the last day)
            segments.Add((current, end));

            return segments;
        }
        public async Task<decimal> CalculatePayrollAsync(Guid userId, Guid companyId, DateTime from, DateTime to)
        {
            var entries = await _timeEntryRepository.GetEntriesWithinPeriodAsync(userId, from, to);

            decimal totalPay = 0m;

            foreach (var entry in entries)
            {
                var segments = SplitEntryByDay(entry.ClockInUtc, entry.ClockOutUtc!.Value);

                foreach (var (segmentStart, segmentEnd) in segments)
                {
                    var hours = (decimal)(segmentEnd - segmentStart).TotalHours;

                    // rate for this day
                    var rate = await _payRateRepository.GetPayRateForDateAsync(userId, segmentStart);
                    if (rate == null)
                        throw new Exception($"Missing pay rate for {segmentStart}");

                    // multiplier
                    decimal multiplier = 1.0m;

                    if (await _holidayCalendarRepository.IsHolidayAsync(companyId, segmentStart))
                        multiplier = 1.5m; // the multipliers can also be taken from the company
                    else if (_holidayCalendarRepository.IsSunday(segmentStart))
                        multiplier = 1.75m;

                    totalPay += hours * rate.HourlyRate * multiplier;
                }
            }

            return totalPay;
        }

    }
}
