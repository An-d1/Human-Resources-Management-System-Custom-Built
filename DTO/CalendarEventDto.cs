using System.Text.Json.Serialization;

namespace HRManagementSystem.DTO
{
    public class CalendarEventDto
    {
        // Required by FullCalendar 
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        // FullCalendar expects Start/End
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Color coding 
        [JsonPropertyName("allDay")]
        public bool AllDay { get; set; } = true;

        [JsonPropertyName("backgroundColor")]
        public string BackgroundColor { get; set; }

        [JsonPropertyName("borderColor")]
        public string BorderColor { get; set; }
    }
}
