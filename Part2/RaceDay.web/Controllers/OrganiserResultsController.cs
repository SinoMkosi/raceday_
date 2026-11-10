using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserResultsController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserResultsController(ApiClient api)
        {
            _api = api;
        }

        private async Task<EventItem?> GetMyEventAsync(int eventId)
        {
            var result = await _api.SendAsync(HttpMethod.Get, $"api/Events/{eventId}");

            if (!result.Success)
            {
                TempData["Error"] = ApiErrors.Friendly(result);
                return null;
            }

            var item = JsonSerializer.Deserialize<EventItem>(result.Body, JsonSerializerOptions.Web);

            if (item == null || item.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You can only manage results for your own events.";
                return null;
            }

            return item;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            var ev = await GetMyEventAsync(eventId);
            if (ev == null)
            {
                return RedirectToAction("Index", "OrganiserEvents");
            }

            var model = new ResultsPageViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name,
                EventDate = ev.Date,
                CanCapture = ev.Date <= DateTime.Now
            };

            var enrolResult = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/enrolments");

            if (!enrolResult.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(enrolResult);
                return View(model);
            }

            var enrolments = JsonSerializer.Deserialize<List<EnrolmentItem>>(enrolResult.Body, JsonSerializerOptions.Web)
                             ?? new List<EnrolmentItem>();

            var results = new List<ResultItem>();
            var resultResult = await _api.SendAsync(HttpMethod.Get, $"api/Results/event/{eventId}");

            if (resultResult.Success)
            {
                results = JsonSerializer.Deserialize<List<ResultItem>>(resultResult.Body, JsonSerializerOptions.Web)
                          ?? new List<ResultItem>();
            }
            else
            {
                ViewBag.Error = ApiErrors.Friendly(resultResult);
            }

            model.Rows = enrolments.Select(e =>
            {
                var r = results.FirstOrDefault(x => x.EnrolmentId == e.EnrolmentId);
                return new ResultRow
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantName = e.ParticipantName,
                    CategoryName = e.CategoryName,
                    ResultId = r?.ResultId,
                    FinishTime = r == null ? "" : r.FinishTime.ToString(@"hh\:mm\:ss"),
                    Position = r?.Position
                };
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int eventId, int enrolmentId, int? resultId, string? finishTime, int? position)
        {
            var ev = await GetMyEventAsync(eventId);
            if (ev == null)
            {
                return RedirectToAction("Index", "OrganiserEvents");
            }

            if (ev.Date > DateTime.Now)
            {
                TempData["Error"] = "Results can only be captured after the event has taken place.";
                return RedirectToAction("Index", new { eventId });
            }

            var formats = new[] { @"hh\:mm\:ss", @"h\:mm\:ss" };

            if (!TimeSpan.TryParseExact(finishTime?.Trim(), formats, CultureInfo.InvariantCulture, out var time)
                || time <= TimeSpan.Zero)
            {
                TempData["Error"] = "Please enter the finish time as hours:minutes:seconds, for example 00:45:30.";
                return RedirectToAction("Index", new { eventId });
            }

            if (position == null || position < 1)
            {
                TempData["Error"] = "Please enter a finishing position of 1 or higher.";
                return RedirectToAction("Index", new { eventId });
            }

            var body = new
            {
                EnrolmentId = enrolmentId,
                FinishTime = time,
                Position = position.Value
            };

            ApiResult result;

            if (resultId.HasValue)
            {
                result = await _api.SendAsync(HttpMethod.Put, $"api/Results/{resultId.Value}", body);
            }
            else
            {
                result = await _api.SendAsync(HttpMethod.Post, "api/Results", body);
            }

            if (result.Success)
            {
                TempData["Success"] = "Result saved.";
            }
            else
            {
                TempData["Error"] = ApiErrors.Friendly(result);
            }

            return RedirectToAction("Index", new { eventId });
        }
    }
}