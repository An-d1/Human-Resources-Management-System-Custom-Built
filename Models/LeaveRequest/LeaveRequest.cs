using HRManagementSystem.Enums;

namespace HRManagementSystem.Models
{
    public class LeaveRequest
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CalculatedDays { get; set; }
        public LeaveType LeaveType { get; set; }

        public LeaveStatus Status { get; set; } // Pending / Approved / Rejected

        public string? Reason { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public Guid? ReviewedByAdminId { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = default!;
    }

}
