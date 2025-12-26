using HRManagementSystem.Enums;
using HRManagementSystem.Models;
using HRManagementSystem.Services;
using HRManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HRManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]

    public class PayRollController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ITimeEntryRepository _timeEntryRepository;
        private readonly IPayRateRepository _payRateRepository;
        private readonly IHolidayCalendarRepository _holidayCalendarRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly CalculateLeaveDays _leaveCalculator;
        private readonly ILeaveRuleRepository _leaveRuleRepository;
        public PayRollController(UserManager<User> userManager,
            IUserRepository userRepository,
            ITimeEntryRepository timeEntryRepository,
            IPayRateRepository payRateRepository,
            IHolidayCalendarRepository holidayCalendarRepository,
            IEmployeeRepository employeeRepository,
            ILeaveRequestRepository leaveRequestRepository,
            CalculateLeaveDays leaveCalculator,
            ILeaveRuleRepository leaveRuleRepository
            )
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _timeEntryRepository = timeEntryRepository;
            _payRateRepository = payRateRepository;
            _holidayCalendarRepository = holidayCalendarRepository;
            _employeeRepository = employeeRepository;
            _leaveRequestRepository = leaveRequestRepository;
            _leaveCalculator = leaveCalculator;
            _leaveRuleRepository = leaveRuleRepository;
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
                    EmployeeLevel = employee.EmployeeLevel?.Name ?? "N/A"
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
            ViewBag.ShowPayrollModal = false; // if modal needed set to true
            ViewBag.Total = total.ToString("F2");

            //For Pdf
            ViewBag.AutoDownloadUrl = Url.Action("DownloadPayslip", "PayRoll", new { userId = userId, from = from, to = to });

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

        [HttpGet]
        public async Task<IActionResult> DownloadPayslip(Guid userId, DateTime from, DateTime to)
        {
            // 1. Get Data
            var admin = await _userManager.GetUserAsync(User); // Get current admin for company ID
            var user = await _userManager.FindByIdAsync(userId.ToString()); // Get the employee user details

            if (user == null || admin == null) return NotFound();

            decimal totalAmount = await CalculatePayrollAsync(userId, admin.CompanyId, from, to);

            // 2. Configure QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // 3. Create PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text($"Payslip: {user.FirstName} {user.LastName}")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Item().Text($"Period: {from:MMM dd, yyyy} - {to.AddDays(-1):MMM dd, yyyy}");
                            x.Item().Text($"Employee Email: {user.Email}");

                            x.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                            x.Item().PaddingTop(10).Text("Net Payable:")
                                .FontSize(16).Bold();

                            x.Item().Text($"€ {totalAmount:F2}")
                                .FontSize(30).Bold().FontColor(Colors.Green.Darken1);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated by SmartStaff -");
                            x.CurrentPageNumber();
                        });
                });
            });

            // 4. Return File
            var pdfBytes = document.GeneratePdf();
            string fileName = $"Payslip_{user.FirstName}_{from:yyyyMM}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
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
            // 1. setup: Fetch Users specific Rule from their Employee Level
            var employee = await _employeeRepository.FindEmployeeByIdAsync(userId.ToString());
            if (employee == null) throw new Exception("Employee not found");

            // Fetch the rule that applies to this employees level
            var rule = await _leaveRuleRepository.GetRuleByLevelIdAsync(employee.EmployeeLevelId.Value);

            // If a rule exists, use its excluded days; otherwise default to Sat/Sun
            string excludedDaysConfig = rule != null ? rule.ExcludedDays : "Saturday,Sunday";

            decimal totalPay = 0m;

            // 2. WORKED HOURS
            var entries = await _timeEntryRepository.GetEntriesWithinPeriodAsync(userId, from, to);

            foreach (var entry in entries)
            {
                var segments = SplitEntryByDay(entry.ClockInUtc, entry.ClockOutUtc!.Value);

                foreach (var (segmentStart, segmentEnd) in segments)
                {
                    var hours = (decimal)(segmentEnd - segmentStart).TotalHours;

                    // rate for this day
                    var rate = await _payRateRepository.GetPayRateForDateAsync(userId, segmentStart);
                    if (rate == null)
                        //ModelState.AddModelError("", $"Missing pay rate for {segmentStart}")
                        throw new Exception($"Missing pay rate for {segmentStart}");

                    // multiplier
                    decimal multiplier = 1.0m;

                    if (await _holidayCalendarRepository.IsHolidayAsync(companyId, segmentStart))
                        multiplier = 1.5m;
                    else if (_holidayCalendarRepository.IsSunday(segmentStart))
                        multiplier = 1.75m;

                    totalPay += hours * rate.HourlyRate * multiplier;
                }
            }

            // 3. paid leave
            var leaveRequests = await _leaveRequestRepository.GetApprovedRequestsOverlappingPeriodAsync(userId, from, to);

            foreach (var request in leaveRequests)
            {
                // filter: Only pay for Annual leave 
                if (request.LeaveType != LeaveType.Annual)
                    continue;

                // 1. Determine Dates
                DateTime overlapStart = request.StartDate < from ? from : request.StartDate;
                DateTime overlapEnd = request.EndDate > to ? to : request.EndDate;

                // 2. Calculate Billable Days 
                int billableDays = _leaveCalculator.CalculateActualLeaveDays(
                    overlapStart,
                    overlapEnd,
                    excludedDaysConfig
                );

                if (billableDays > 0)
                {
                    var rate = await _payRateRepository.GetPayRateForDateAsync(userId, overlapStart);
                    if (rate != null)
                    {
                        decimal hoursPerDay = 8.0m; //standard hours, but needs to be made dynamic so it allows for part time employees aswell
                        totalPay += billableDays * hoursPerDay * rate.HourlyRate;
                    }
                }
            }

            return totalPay;
        }
    }
}

