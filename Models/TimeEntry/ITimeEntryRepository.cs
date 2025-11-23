namespace HRManagementSystem.Models
{
    public interface ITimeEntryRepository
    {
        Task<bool> IsCurrentlyCheckedInAsync(Guid userID);
        Task<bool> CheckInEmployeeAsync(Guid userId);
        Task<bool> CheckOutEmployeeAsync(Guid userId);
        Task<List<TimeEntry>> GetEntriesForTodayAsync(Guid userId);
        Task<List<TimeEntry>> GetEntriesWithinPeriodAsync(Guid userId, DateTime from, DateTime to);
    }
}
