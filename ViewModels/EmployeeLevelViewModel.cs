
namespace HRManagementSystem.ViewModels
{
    public class EmployeeLevelViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int MaxAnnualLeave { get; set; }
        public List<string> ExcludedDays { get; set; }
    }

}
