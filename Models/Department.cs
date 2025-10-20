namespace HRManagementSystem.Models;

    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = default!;
        public ICollection<EmployeeDepartment> EmployeeDepartments { get; set; } = new List<EmployeeDepartment>();

    }

