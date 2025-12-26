namespace HRManagementSystem.Models
{
    public class LeaveRule
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid EmployeeLevelId { get; set; } // fk to the EMployeeLevel

        public int MaxAnnualLeaves { get; set; }
        public string ExcludedDays { get; set; }

        public virtual EmployeeLevel EmployeeLevel { get; set; } = default!; // virtual, so it remains null until needed
    }
}
