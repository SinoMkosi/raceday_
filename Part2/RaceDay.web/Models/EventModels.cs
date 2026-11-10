using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class EventItem
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Location { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int OrganiserId { get; set; }

        [JsonPropertyName("bannerUrl")]
        public string? BannerName { get; set; }

        [JsonIgnore]
        public string? BannerUrl => string.IsNullOrWhiteSpace(BannerName)
            ? null
            : "/Images/Get?name=" + Uri.EscapeDataString(BannerName);
    }

    public class EventViewModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Please enter the event name.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a description.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a date and time.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Date and time")]
        public DateTime Date { get; set; } = DateTime.Today.AddDays(7).AddHours(8);

        [Required(ErrorMessage = "Please enter the location.")]
        public string Location { get; set; } = string.Empty;

        [Range(0.1, 1000, ErrorMessage = "Distance must be between 0.1 and 1000 km.")]
        [Display(Name = "Distance (km)")]
        public double DistanceKm { get; set; }

        [Required(ErrorMessage = "Please choose an event type.")]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = string.Empty;

        [Display(Name = "Banner image (optional)")]
        public IFormFile? Banner { get; set; }

        public string? CurrentBannerSrc { get; set; }
    }
}