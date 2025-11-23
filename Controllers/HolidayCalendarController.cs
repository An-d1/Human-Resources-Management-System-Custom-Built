using HRManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HRManagementSystem.Controllers
{
    public class HolidayCalendarController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IHolidayCalendarRepository _holidayCalendarRepository;
        private readonly NationalHolidayService _nationalHolidayService;
        public HolidayCalendarController(UserManager<User> userManager, IHolidayCalendarRepository holidayCalendarRepository, NationalHolidayService nationalHolidayService)
        {
            _userManager = userManager;
            _holidayCalendarRepository = holidayCalendarRepository;
            _nationalHolidayService = nationalHolidayService;
        }
        public IActionResult CalendarIndex()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetHolidays()
        {
            var currentUser = await _userManager.GetUserAsync(User); // Get the current user

            var holidays = await _holidayCalendarRepository 
                .GetAllByCompanyIdAsync(currentUser.CompanyId);

            var events = holidays // get all the holidays of the company and conver to JSON so FullCalendar can read
                .Select(h => new
            {
                id = h.Id,
                title = h.Name,
                start = h.StartDate.ToString("yyyy-MM-dd"),
                end = h.EndDate.ToString("yyyy-MM-dd"),
                color = h.IsCompanyHoliday ? "#3B82F6" : "#e66b63"   // blue for company, orange for public
                });

            return Json(events);
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
        public async Task<IActionResult> DeleteHoliday([FromBody] Guid id) // read JSON from Http body
        {
            var holiday = await _holidayCalendarRepository.GetByIdAsync(id);

            if (holiday == null)
                return NotFound();

            await _holidayCalendarRepository.DeleteAsync(holiday);

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MoveHoliday([FromBody] MoveHolidayDto dto)
        {
            if (dto == null)
                return BadRequest();

            var holiday = await _holidayCalendarRepository.GetByIdAsync(dto.Id);

            if (holiday == null)
                return NotFound();

            // Parse new dates
            var newStart = DateTime.Parse(dto.NewStartDate);

            // If no end date provided (single day), keep a single day
            var newEnd = string.IsNullOrEmpty(dto.NewEndDate)
                ? newStart
                : DateTime.Parse(dto.NewEndDate);

            holiday.StartDate = newStart;
            holiday.EndDate = newEnd;

            await _holidayCalendarRepository.UpdateAsync(holiday);

            return Ok();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResizeHoliday([FromBody] ResizeHolidayDto dto)
        {
            if (dto == null)
                return BadRequest();

            var holiday = await _holidayCalendarRepository.GetByIdAsync(dto.Id);

            if (holiday == null)
                return NotFound();

            var newStart = DateTime.Parse(dto.NewStartDate);
            var newEnd = DateTime.Parse(dto.NewEndDate);

            holiday.StartDate = newStart;
            holiday.EndDate = newEnd;

            await _holidayCalendarRepository.UpdateAsync(holiday);

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

public class HolidayCreateDto // used to read the incoming JSON from the front
{
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsPublicHoliday { get; set; } = true;
    public bool IsCompanyHoliday { get; set; } = false;
}
public class MoveHolidayDto
{
    public Guid Id { get; set; }
    public string NewStartDate { get; set; }
    public string NewEndDate { get; set; }
}

public class ResizeHolidayDto
{
    public Guid Id { get; set; }
    public string NewStartDate { get; set; }
    public string NewEndDate { get; set; }
}
