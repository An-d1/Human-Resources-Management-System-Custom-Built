using HRManagementSystem.DTO;
using HRManagementSystem.Enums;
using HRManagementSystem.Models;
using HRManagementSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HRManagementSystem.Controllers
{
    public class HolidayCalendarController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IHolidayCalendarRepository _holidayCalendarRepository;
        private readonly ILeaveRequestRepository _leaveRequestRepository;
        private readonly NationalHolidayService _nationalHolidayService;
        public HolidayCalendarController(UserManager<User> userManager, IHolidayCalendarRepository holidayCalendarRepository, NationalHolidayService nationalHolidayService, ILeaveRequestRepository leaveRequestRepository)
        {
            _userManager = userManager;
            _holidayCalendarRepository = holidayCalendarRepository;
            _nationalHolidayService = nationalHolidayService;
            _leaveRequestRepository = leaveRequestRepository;
        }
        public IActionResult CalendarIndex()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View();
        }

        private static (string bg, string border) GetLeaveColors(LeaveStatus status)
        {
            return status switch
            {
                LeaveStatus.Pending => ("#ffc107", "#ffc107"), // warning
                LeaveStatus.Approved => ("#198754", "#198754"), // success
                LeaveStatus.Rejected => ("#dc3545", "#dc3545"), // danger
                _ => ("#6c757d", "#6c757d")
            };
        }


        [HttpGet]
        public async Task<IActionResult> GetHolidays()
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = User.IsInRole("Admin");

            // holidays (everyone sees these)
            var holidays = await _holidayCalendarRepository
                .GetAllByCompanyIdAsync(user.CompanyId);

            var holidayEvents = holidays.Select(h => new CalendarEventDto
            {
                Id = $"holiday-{h.Id}",
                Title = h.Name,
                Start = h.StartDate,
                End = h.EndDate.AddDays(1),
                AllDay = true, 

                BackgroundColor = h.IsCompanyHoliday ? "#3B82F6" : "#e66b63",
                BorderColor = h.IsCompanyHoliday ? "#3B82F6" : "#e66b63"
            });

            // leaves (filtered by role, so that only admins can see all the requests on their calendar)
            IEnumerable<LeaveRequest> leaves;

            if (isAdmin)
            {
                leaves = await _leaveRequestRepository
                    .GetApprovedAndPendingByCompanyAsync(user.CompanyId);
            }
            else
            {
                leaves = await _leaveRequestRepository
                    .GetByUserAsync(user.Id);
            }

            var leaveEvents = leaves.Select(l =>
            {
                var (bg, border) = GetLeaveColors(l.Status);

                return new CalendarEventDto
                {
                    // need to add first name and last name at every request i send to the database
                    Id = $"leave-{l.Id}",
                    Title = isAdmin
                        ? $"{l.User.FirstName} {l.User.LastName} – {l.LeaveType}"
                        : $"{l.LeaveType} ({l.Status})",

                    Start = l.StartDate,
                    End = l.EndDate.AddDays(1),
                    BackgroundColor = bg,
                    BorderColor = border
                };
            });

            return Ok(holidayEvents.Concat(leaveEvents)); // so FullCalendar gets them all as one
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddHoliday([FromBody] HolidayCreateDto dto)
        {

            var currentUser = await _userManager.GetUserAsync(User); // Get the current user

            if (dto == null || string.IsNullOrWhiteSpace(dto.StartDate.ToString()))
                return BadRequest("Invalid date.");

            var holiday = new HolidayCalendar
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CompanyId = currentUser.CompanyId,
                IsPublicHoliday = dto.IsPublicHoliday,
                IsCompanyHoliday = dto.IsCompanyHoliday
            };

            await _holidayCalendarRepository.AddAsync(holiday);

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEvent([FromBody] string compositeId)
        {
            if (string.IsNullOrEmpty(compositeId)) return BadRequest("Invalid ID");

            // Split the id ("leave-guid" or "holiday-guid") Because it was converted when send as JSON
            var parts = compositeId.Split('-', 2); // so it splits only the first dash and keeps the GUID intact 

            if (parts.Length < 2) return BadRequest("Invalid ID format");

            string type = parts[0]; // "holiday" or "leave"
            string guidString = parts[1].Trim('"'); // Remove any accidental quotes

            if (!Guid.TryParse(parts[1], out Guid actualId)) return BadRequest("Invalid Guid");

            if (type == "holiday")
            {
                var holiday = await _holidayCalendarRepository.GetByIdAsync(actualId);
                if (holiday == null) return NotFound();

                await _holidayCalendarRepository.DeleteAsync(holiday);
            }
            else if (type == "leave")
            {
                var leaveReq = await _leaveRequestRepository.GetByIdAsync(actualId);
                if (leaveReq == null) return NotFound();

                await _leaveRequestRepository.DeleteAsync(leaveReq);
            }
            else
            {
                return BadRequest("Unknown event type");
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MoveEvent([FromBody] MoveHolidayDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Id)) return BadRequest();

            // 1. Split the "type-id" string ("leave-xyz")
            var parts = dto.Id.Split('-', 2);
            if (parts.Length < 2) return BadRequest("Invalid ID format");

            string type = parts[0].ToLower();
            if (!Guid.TryParse(parts[1], out Guid actualGuid)) return BadRequest("Invalid Guid");

            // 2. Parse new dates
            var newStart = DateTime.Parse(dto.NewStartDate);
            // FullCalendar sends the day AFTER the event ends. 
            // We must subtract 1 day to save the correct date in our DB.
            var newEnd = string.IsNullOrEmpty(dto.NewEndDate)
                ? newStart
                : DateTime.Parse(dto.NewEndDate).AddDays(-1);

            // 3. Route to the correct repository
            if (type == "holiday")
            {
                var holiday = await _holidayCalendarRepository.GetByIdAsync(actualGuid);
                if (holiday == null) return NotFound();

                holiday.StartDate = newStart;
                holiday.EndDate = newEnd;
                await _holidayCalendarRepository.UpdateAsync(holiday);
            }
            else if (type == "leave")
            {
                var leave = await _leaveRequestRepository.GetByIdAsync(actualGuid);
                if (leave == null) return NotFound();

                leave.StartDate = newStart;
                leave.EndDate = newEnd; // Subtract the 1-day FullCalendar offset
                await _leaveRequestRepository.UpdateAsync(leave);
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResizeEvent([FromBody] ResizeHolidayDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Id)) return BadRequest();

            var parts = dto.Id.Split('-', 2);
            if (parts.Length < 2) return BadRequest();

            string type = parts[0].ToLower();
            if (!Guid.TryParse(parts[1], out Guid actualGuid)) return BadRequest();

            var newStart = DateTime.Parse(dto.NewStartDate);
            var newEnd = DateTime.Parse(dto.NewEndDate).AddDays(-1);

            if (type == "holiday")
            {
                var holiday = await _holidayCalendarRepository.GetByIdAsync(actualGuid);
                if (holiday == null) return NotFound();

                holiday.StartDate = newStart;
                holiday.EndDate = newEnd;
                await _holidayCalendarRepository.UpdateAsync(holiday);
            }
            else if (type == "leave")
            {
                var leave = await _leaveRequestRepository.GetByIdAsync(actualGuid);
                if (leave == null) return NotFound();

                leave.StartDate = newStart;
                leave.EndDate = newEnd;
                await _leaveRequestRepository.UpdateAsync(leave);
            }

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ImportNationalHolidays([FromBody] ImportHolidayRequest dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var holidays = await _nationalHolidayService.GetHolidaysAsync(dto.Year, dto.CountryCode);

            foreach (var item in holidays)
            {
              
                var date = DateTime.Parse(item.Date);

                if (await _holidayCalendarRepository.ExistsAsync(user.CompanyId, date)) // skip duplicates if they already exist
                    continue;

                var holiday = new HolidayCalendar
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    StartDate = date,
                    EndDate = date,
                    IsPublicHoliday = true,
                    IsCompanyHoliday = false,
                    CompanyId = user.CompanyId
                };

                await _holidayCalendarRepository.AddAsync(holiday);
            }

            return Ok();
        }

        public class ImportHolidayRequest
        {
            public string CountryCode { get; set; }
            public int Year { get; set; }
        }

    }
}
