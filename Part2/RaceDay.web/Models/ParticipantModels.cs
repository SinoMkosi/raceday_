namespace RaceDay.Web.Models
{
    public class MyEnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolledOn { get; set; }
        public string Status { get; set; } = "Pending";
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class EventDetailsViewModel
    {
        public EventItem Event { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();
        public bool AlreadyEnrolled { get; set; }
        public string? EnrolmentStatus { get; set; }
        public string? EnrolledCategory { get; set; }
        public bool IsPast { get; set; }
    }
}