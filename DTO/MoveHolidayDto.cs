namespace HRManagementSystem.DTO
{
    public class MoveHolidayDto // Used to map htttp JSON body to move events
    {
        public string Id { get; set; }
        public string NewStartDate { get; set; }
        public string NewEndDate { get; set; }
    }
}
