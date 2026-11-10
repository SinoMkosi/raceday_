using Microsoft.AspNetCore.Mvc;
using RaceDay.web.Models;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using System.Diagnostics;
using System.Text.Json;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiClient _api;

        public HomeController(ApiClient api)
        {
            _api = api;
        }

        [SessionAuthorize]
        public async Task<IActionResult> Index(string? search, string? eventType, double? maxDistance)
        {
            if (HttpContext.Session.GetString("Role") == "Organiser")
            {
                return RedirectToAction("Dashboard", "Organiser");
            }

            var model = new EventsFilterViewModel
            {
                Search = search,
                EventType = eventType,
                MaxDistance = maxDistance
            };

            var result = await _api.SendAsync(HttpMethod.Get, "api/Events");

            if (!result.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(result);
                return View(model);
            }

            var all = JsonSerializer.Deserialize<List<EventItem>>(result.Body, JsonSerializerOptions.Web)
                      ?? new List<EventItem>();

            var query = all.Where(e => e.Date >= DateTime.Now);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var text = search.Trim();
                query = query.Where(e =>
                    e.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                    e.Location.Contains(text, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(e => e.EventType == eventType);
            }

            if (maxDistance.HasValue)
            {
                query = query.Where(e => e.DistanceKm <= maxDistance.Value);
            }

            model.Events = query.OrderBy(e => e.Date).ToList();

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}