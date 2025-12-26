namespace HRManagementSystem.Models
{
    public class EmployeeLevel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // "Senior", ,"Junior","Intern"
        public Guid CompanyId { get; set; }

        public virtual Company Company { get; set; } = default!;
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
