namespace RaceDay.Web.Models
{
    public class ResultItem
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }

    public class ResultRow
    {
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int? ResultId { get; set; }
        public string FinishTime { get; set; } = string.Empty;
        public int? Position { get; set; }
    }

    public class ResultsPageViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public bool CanCapture { get; set; }
        public List<ResultRow> Rows { get; set; } = new();
    }
}