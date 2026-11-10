namespace RaceDay.Web.Models
{
    public class DashboardEventRow
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public int EnrolmentCount { get; set; }
        public bool IsUpcoming { get; set; }
    }

    public class DashboardViewModel
    {
        public int TotalEvents { get; set; }
        public int UpcomingEvents { get; set; }
        public int TotalEnrolments { get; set; }
        public List<DashboardEventRow> Events { get; set; } = new();
    }
}