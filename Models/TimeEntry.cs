namespace HRManagementSystem.Models
{
    public class TimeEntry
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;
        public DateTime ClockInUtc { get; set; }
        public DateTime? ClockOutUtc { get; set; }
    }

}
