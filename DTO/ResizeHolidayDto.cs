namespace HRManagementSystem.DTO
{
    public class ResizeHolidayDto // JSON to entity to resize
    {
        public Guid Id { get; set; }
        public string NewStartDate { get; set; }
        public string NewEndDate { get; set; }
    }
}

