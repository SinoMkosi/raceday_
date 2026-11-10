using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserCategoriesController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserCategoriesController(ApiClient api)
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
                TempData["Error"] = "You can only manage categories for your own events.";
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

            var model = new CategoriesPageViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name
            };

            var result = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");

            if (result.Success)
            {
                model.Categories = JsonSerializer.Deserialize<List<CategoryItem>>(result.Body, JsonSerializerOptions.Web)
                                   ?? new List<CategoryItem>();
            }
            else
            {
                ViewBag.Error = ApiErrors.Friendly(result);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int eventId, string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Please enter a category name.";
                return RedirectToAction("Index", new { eventId });
            }

            var result = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", new
            {
                Name = name.Trim()
            });

            if (result.Success)
            {
                TempData["Success"] = "Category added.";
            }
            else
            {
                TempData["Error"] = ApiErrors.Friendly(result);
            }

            return RedirectToAction("Index", new { eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int eventId, int id)
        {
            var result = await _api.SendAsync(HttpMethod.Delete, $"api/categories/{id}");

            if (result.Success)
            {
                TempData["Success"] = "Category deleted.";
            }
            else
            {
                TempData["Error"] = ApiErrors.Friendly(result);
            }

            return RedirectToAction("Index", new { eventId });
        }
    }
}