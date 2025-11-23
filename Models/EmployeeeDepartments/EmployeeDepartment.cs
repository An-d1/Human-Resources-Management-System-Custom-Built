namespace HRManagementSystem.Models;

    public class EmployeeDepartment
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = default!;
    }


