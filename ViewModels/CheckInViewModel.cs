using HRManagementSystem.Models;

namespace HRManagementSystem.ViewModels;
public class CheckInViewModel
{
    public bool IsCheckedIn { get; set; }
    public List<TimeEntry> TodayEntries { get; set; }
}
