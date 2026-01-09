using System.ComponentModel.DataAnnotations.Schema;

namespace HRManagementSystem.Models;

    public class Employee
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public DateOnly HireDate { get; set; }
        public DateOnly? TerminationDate { get; set; }
        public bool IsEmployed { get; set; } = true; // always true unless contract terminated which sets it to false
        public Guid? EmployeeLevelId { get; set; } // Fk

        [ForeignKey("EmployeeLevelId")]
        public virtual EmployeeLevel? EmployeeLevel { get; set; } // Navigation Property
        public virtual ICollection<PayRate> PayRates { get; set; } = new List<PayRate>();
}

