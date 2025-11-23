using HRManagementSystem.Data;
using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

public class HolidayRepository : IHolidayCalendarRepository
{
    private readonly ApplicationDbContext _context;

    public HolidayRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HolidayCalendar>> GetAllByCompanyIdAsync(Guid companyId)
    {
        return await _context.HolidayCalendars
            .Where(h => h.CompanyId == companyId)
            .ToListAsync();
    }
    public async Task<HolidayCalendar> AddAsync(HolidayCalendar holiday)
    {
        _context.HolidayCalendars.Add(holiday);
        await _context.SaveChangesAsync();
        return holiday;
    }
    public Task<HolidayCalendar> GetByIdAsync(Guid id) =>
    _context.HolidayCalendars.FirstOrDefaultAsync(h => h.Id == id);

    public async Task DeleteAsync(HolidayCalendar holiday)
    {
        _context.HolidayCalendars.Remove(holiday);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(HolidayCalendar holiday)
    {
        _context.HolidayCalendars.Update(holiday);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsAsync(Guid companyId, DateTime date)
    {
        return await _context.HolidayCalendars
            .AnyAsync(h =>
                h.CompanyId == companyId &&
                h.StartDate.Date == date.Date
            );
    }
    public void Save()
    {
        _context.SaveChanges();
    }
}
