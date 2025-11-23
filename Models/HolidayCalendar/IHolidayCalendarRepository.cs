using HRManagementSystem.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

public interface IHolidayCalendarRepository
{
    Task<IEnumerable<HolidayCalendar>> GetAllByCompanyIdAsync(Guid companyId);
    Task <HolidayCalendar> AddAsync(HolidayCalendar holiday);
    Task<HolidayCalendar> GetByIdAsync(Guid id);
    Task DeleteAsync(HolidayCalendar holiday);
    Task UpdateAsync(HolidayCalendar holiday);
    Task<bool> ExistsAsync(Guid companyId, DateTime date);
    Task<bool> IsHolidayAsync(Guid companyId, DateTime date);
    bool IsSunday(DateTime date);
}
