namespace RaceDay.Web.Models
{
    public class EventsFilterViewModel
    {
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public double? MaxDistance { get; set; }
        public List<EventItem> Events { get; set; } = new();
    }
}