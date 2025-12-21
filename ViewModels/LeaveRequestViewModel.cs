using HRManagementSystem.Enums;
using HRManagementSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace HRManagementSystem.ViewModels
{
    public class LeaveRequestViewModel
    {
        public Guid RequestId { get; set; }
        public string EmployeeFirstName { get; set; } = string.Empty;
        public string EmployeeLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a leave type.")]
        public LeaveType? LeaveType { get; set; } = default!;
        public LeaveStatus? LeaveStatus { get; set; }

        [Required(ErrorMessage = "Please select a start date.")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "Please select an end date.")]
        public DateTime? EndDate { get; set; }
        public string? Reason { get; set; }
        public List<LeaveRequest> OutgoingRequests { get; set; }
                = new();
    }
}
