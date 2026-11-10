using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Participant")]
    public class EventsController : Controller
    {
        private readonly ApiClient _api;

        public EventsController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var evResult = await _api.SendAsync(HttpMethod.Get, $"api/Events/{id}");

            if (!evResult.Success)
            {
                TempData["Error"] = ApiErrors.Friendly(evResult);
                return RedirectToAction("Index", "Home");
            }

            var ev = JsonSerializer.Deserialize<EventItem>(evResult.Body, JsonSerializerOptions.Web);

            if (ev == null)
            {
                TempData["Error"] = "That event could not be found.";
                return RedirectToAction("Index", "Home");
            }

            var model = new EventDetailsViewModel
            {
                Event = ev,
                IsPast = ev.Date < DateTime.Now
            };

            var catResult = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");

            if (catResult.Success)
            {
                model.Categories = JsonSerializer.Deserialize<List<CategoryItem>>(catResult.Body, JsonSerializerOptions.Web)
                                   ?? new List<CategoryItem>();
            }
            else
            {
                ViewBag.Error = ApiErrors.Friendly(catResult);
            }

            var mineResult = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");

            if (mineResult.Success)
            {
                var mine = JsonSerializer.Deserialize<List<MyEnrolmentItem>>(mineResult.Body, JsonSerializerOptions.Web)
                           ?? new List<MyEnrolmentItem>();

                var existing = mine.FirstOrDefault(x => x.EventId == id);

                if (existing != null)
                {
                    model.AlreadyEnrolled = true;
                    model.EnrolmentStatus = existing.Status;
                    model.EnrolledCategory = existing.CategoryName;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int eventId, int? categoryId)
        {
            if (categoryId == null)
            {
                TempData["Error"] = "Please choose a category before enrolling.";
                return RedirectToAction("Details", new { id = eventId });
            }

            var evResult = await _api.SendAsync(HttpMethod.Get, $"api/Events/{eventId}");

            if (evResult.Success)
            {
                var ev = JsonSerializer.Deserialize<EventItem>(evResult.Body, JsonSerializerOptions.Web);

                if (ev != null && ev.Date < DateTime.Now)
                {
                    TempData["Error"] = "This event has already taken place, so you cannot enrol.";
                    return RedirectToAction("Details", new { id = eventId });
                }
            }

            var result = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/enrol", new
            {
                CategoryId = categoryId.Value
            });

            if (result.Success)
            {
                TempData["Success"] = "You are enrolled. Your enrolment is pending until the organiser confirms it.";
            }
            else
            {
                TempData["Error"] = ApiErrors.Friendly(result);
            }

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}