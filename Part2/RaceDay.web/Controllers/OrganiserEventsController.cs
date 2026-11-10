using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserEventsController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserEventsController(ApiClient api)
        {
            _api = api;
        }

        private static object ToDto(EventViewModel m)
        {
            return new
            {
                m.Name,
                m.Description,
                m.Date,
                m.Location,
                m.DistanceKm,
                m.EventType
            };
        }

        private static string UploadMessage(ApiResult r)
        {
            if (r.StatusCode == 400 || r.StatusCode == 503)
            {
                return r.Body.Trim().Trim('"');
            }

            return ApiErrors.Friendly(r);
        }

        private static int? ReadEventId(string body)
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                return doc.RootElement.GetProperty("eventId").GetInt32();
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _api.SendAsync(HttpMethod.Get, "api/Events");

            if (!result.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(result);
                return View(new List<EventItem>());
            }

            var all = JsonSerializer.Deserialize<List<EventItem>>(result.Body, JsonSerializerOptions.Web)
                      ?? new List<EventItem>();

            var myId = HttpContext.Session.GetInt32("UserId");
            var mine = all.Where(e => e.OrganiserId == myId).OrderBy(e => e.Date).ToList();

            return View(mine);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new EventViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.SendAsync(HttpMethod.Post, "api/Events", ToDto(model));

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, ApiErrors.Friendly(result));
                return View(model);
            }

            if (model.Banner != null && model.Banner.Length > 0)
            {
                var newId = ReadEventId(result.Body);

                if (newId.HasValue)
                {
                    var up = await _api.SendFileAsync($"api/Events/{newId.Value}/banner", model.Banner);

                    if (!up.Success)
                    {
                        TempData["Error"] = "Event created, but the banner was not saved: " + UploadMessage(up);
                        return RedirectToAction("Index");
                    }
                }
            }

            TempData["Success"] = "Event created.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var result = await _api.SendAsync(HttpMethod.Get, $"api/Events/{id}");

            if (!result.Success)
            {
                TempData["Error"] = ApiErrors.Friendly(result);
                return RedirectToAction("Index");
            }

            var item = JsonSerializer.Deserialize<EventItem>(result.Body, JsonSerializerOptions.Web);

            if (item == null || item.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You can only edit your own events.";
                return RedirectToAction("Index");
            }

            return View(new EventViewModel
            {
                EventId = item.EventId,
                Name = item.Name,
                Description = item.Description,
                Date = item.Date,
                Location = item.Location,
                DistanceKm = item.DistanceKm,
                EventType = item.EventType,
                CurrentBannerSrc = item.BannerUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EventViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.SendAsync(HttpMethod.Put, $"api/Events/{model.EventId}", ToDto(model));

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, ApiErrors.Friendly(result));
                return View(model);
            }

            if (model.Banner != null && model.Banner.Length > 0)
            {
                var up = await _api.SendFileAsync($"api/Events/{model.EventId}/banner", model.Banner);

                if (!up.Success)
                {
                    TempData["Error"] = "Event updated, but the banner was not saved: " + UploadMessage(up);
                    return RedirectToAction("Index");
                }
            }

            TempData["Success"] = "Event updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _api.SendAsync(HttpMethod.Delete, $"api/Events/{id}");

            if (result.Success)
            {
                TempData["Success"] = "Event deleted.";
            }
            else
            {
                TempData["Error"] = ApiErrors.Friendly(result);
            }

            return RedirectToAction("Index");
        }
    }
}