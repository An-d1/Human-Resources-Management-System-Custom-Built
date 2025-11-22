namespace HRManagementSystem.ViewModels
{
    public class EmployeeViewModel
    {
        public string UserId { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool IsEmailConfirmed { get; set; }
        public string Phone { get; set; } = default!;
        public string Role { get; set; } = default!;
        public List<string> Departments { get; set; } = new();
        public DateOnly HireDate { get; set; }
        public DateOnly? TerminationDate { get; set; }
        public bool IsEmployed { get; set; } = true; // always true unless contract terminated which sets it to false
    }
}
