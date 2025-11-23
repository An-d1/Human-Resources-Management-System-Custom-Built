using HRManagementSystem.Data;
using HRManagementSystem.Models;
using Microsoft.EntityFrameworkCore;


public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly ApplicationDbContext _context;
    public TimeEntryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsCurrentlyCheckedInAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        // Checks if user is logged in but not logged out, returns true
        return await _context.TimeEntries
            .AnyAsync(t =>
                t.UserId == userId &&
                t.ClockInUtc.Date == today &&
                t.ClockOutUtc == null
            );
    }

    public async Task<bool> CheckInEmployeeAsync(Guid userId)
    {
        // check the user is not already checked in
        if (await IsCurrentlyCheckedInAsync(userId))
            return false;

        var entry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ClockInUtc = DateTime.UtcNow
        };

        _context.TimeEntries.Add(entry);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CheckOutEmployeeAsync(Guid userId)
    {
        // ensure user is actually checked in
        if (!await IsCurrentlyCheckedInAsync(userId))
            return false;

        var today = DateTime.UtcNow.Date;

        // time entry for today
        var openEntry = await _context.TimeEntries
            .Where(t =>
                t.UserId == userId &&
                t.ClockInUtc.Date == today &&
                t.ClockOutUtc == null
            )
            .FirstOrDefaultAsync();

        if (openEntry == null)
            return false;  // should never happen

        // stamp checkout time
        openEntry.ClockOutUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<TimeEntry>> GetEntriesForTodayAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        // gets only time entries of today

        return await _context.TimeEntries
            .Where(t =>
                t.UserId == userId &&
                t.ClockInUtc >= today &&
                t.ClockInUtc < tomorrow
            )
            .OrderBy(t => t.ClockInUtc)
            .ToListAsync();
    }

    // used to calculate payrolls
    public async Task<List<TimeEntry>> GetEntriesWithinPeriodAsync(Guid userId, DateTime from, DateTime to)
    {
        return await _context.TimeEntries
            .Where(t =>
                t.UserId == userId &&
                t.ClockInUtc >= from &&
                t.ClockInUtc < to &&
                t.ClockOutUtc != null
            )
            .OrderBy(t => t.ClockInUtc)
            .ToListAsync();
    }



}
