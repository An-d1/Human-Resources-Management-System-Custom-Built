namespace HRManagementSystem.Models
{
    public class PayRate
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public decimal HourlyRate;
        public DateOnly EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
    }
}
