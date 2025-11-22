namespace HRManagementSystem.Models;

    public class Employee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public DateOnly HireDate { get; set; }
        public DateOnly? TerminationDate { get; set; }
        public bool IsEmployed { get; set; } = true; // always true unless contract terminated which sets it to false
    }

