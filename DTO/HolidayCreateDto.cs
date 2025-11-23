namespace HRManagementSystem.DTO
{
    public class HolidayCreateDto // used to read the incoming JSON from the front for creating new events
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsPublicHoliday { get; set; } = true;
        public bool IsCompanyHoliday { get; set; } = false;
    }
}
