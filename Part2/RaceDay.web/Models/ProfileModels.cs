using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your last name.")]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("profilePictureUrl")]
        public string? PictureName { get; set; }

        [JsonIgnore]
        public string? PictureSrc => string.IsNullOrWhiteSpace(PictureName)
            ? null
            : "/Images/Get?name=" + Uri.EscapeDataString(PictureName);
    }
}