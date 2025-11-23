namespace HRManagementSystem.Models
{
    public class HolidayCalendar
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; }
        public bool IsPublicHoliday { get; set; }  // official government holiday
        public bool IsCompanyHoliday { get; set; } // optional custom holiday
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
