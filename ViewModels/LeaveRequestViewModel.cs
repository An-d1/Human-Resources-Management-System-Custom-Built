using HRManagementSystem.Enums;

namespace HRManagementSystem.ViewModels
{
    public class LeaveRequestViewModel
    {
        public LeaveType LeaveType { get; set; } = default!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Reason { get; set; }

    }
}
