namespace HRManagementSystem.DTO
{
    public class ResizeHolidayDto // JSON to entity to resize
    {
        public string Id { get; set; }
        public string NewStartDate { get; set; }
        public string NewEndDate { get; set; }
    }
}

