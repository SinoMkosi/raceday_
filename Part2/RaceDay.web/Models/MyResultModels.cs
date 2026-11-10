namespace RaceDay.Web.Models
{
    public class MyResultItem
    {
        public int ResultId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public int TotalFinishers { get; set; }

        public string PositionText
        {
            get
            {
                var lastTwo = Position % 100;
                var lastOne = Position % 10;
                string suffix;

                if (lastTwo >= 11 && lastTwo <= 13)
                {
                    suffix = "th";
                }
                else if (lastOne == 1)
                {
                    suffix = "st";
                }
                else if (lastOne == 2)
                {
                    suffix = "nd";
                }
                else if (lastOne == 3)
                {
                    suffix = "rd";
                }
                else
                {
                    suffix = "th";
                }

                return $"{Position}{suffix} out of {TotalFinishers}";
            }
        }
    }
}