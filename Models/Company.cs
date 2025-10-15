namespace HRManagementSystem.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? TaxId { get; set; }
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<User> Employees { get; set; } = new List<User>(); 
    }
}
