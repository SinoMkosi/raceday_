namespace RaceDay.Web.Models
{
    public class EnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolledOn { get; set; }
    }

    public class EnrolmentsPageViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<EnrolmentItem> Enrolments { get; set; } = new();
    }
}