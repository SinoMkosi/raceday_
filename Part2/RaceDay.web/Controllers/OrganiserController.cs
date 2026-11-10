using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = new DashboardViewModel();

            var result = await _api.SendAsync(HttpMethod.Get, "api/Events");

            if (!result.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(result);
                return View(model);
            }

            var all = JsonSerializer.Deserialize<List<EventItem>>(result.Body, JsonSerializerOptions.Web)
                      ?? new List<EventItem>();

            var myId = HttpContext.Session.GetInt32("UserId");
            var mine = all.Where(e => e.OrganiserId == myId).OrderBy(e => e.Date).ToList();

            foreach (var ev in mine)
            {
                var count = 0;
                var enrolResult = await _api.SendAsync(HttpMethod.Get, $"api/events/{ev.EventId}/enrolments");

                if (enrolResult.Success)
                {
                    var list = JsonSerializer.Deserialize<List<EnrolmentItem>>(enrolResult.Body, JsonSerializerOptions.Web);
                    count = list?.Count ?? 0;
                }

                model.Events.Add(new DashboardEventRow
                {
                    EventId = ev.EventId,
                    Name = ev.Name,
                    Date = ev.Date,
                    Location = ev.Location,
                    EnrolmentCount = count,
                    IsUpcoming = ev.Date >= DateTime.Now
                });
            }

            model.TotalEvents = model.Events.Count;
            model.UpcomingEvents = model.Events.Count(e => e.IsUpcoming);
            model.TotalEnrolments = model.Events.Sum(e => e.EnrolmentCount);

            return View(model);
        }
    }
}