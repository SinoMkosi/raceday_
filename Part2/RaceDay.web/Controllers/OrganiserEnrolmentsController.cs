using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserEnrolmentsController : Controller
    {
        private readonly ApiClient _api;

        public OrganiserEnrolmentsController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int eventId)
        {
            var evResult = await _api.SendAsync(HttpMethod.Get, $"api/Events/{eventId}");

            if (!evResult.Success)
            {
                TempData["Error"] = ApiErrors.Friendly(evResult);
                return RedirectToAction("Index", "OrganiserEvents");
            }

            var ev = JsonSerializer.Deserialize<EventItem>(evResult.Body, JsonSerializerOptions.Web);

            if (ev == null || ev.OrganiserId != HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You can only view enrolments for your own events.";
                return RedirectToAction("Index", "OrganiserEvents");
            }

            var model = new EnrolmentsPageViewModel
            {
                EventId = ev.EventId,
                EventName = ev.Name
            };

            var result = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/enrolments");

            if (result.Success)
            {
                model.Enrolments = JsonSerializer.Deserialize<List<EnrolmentItem>>(result.Body, JsonSerializerOptions.Web)
                                   ?? new List<EnrolmentItem>();
            }
            else
            {
                ViewBag.Error = ApiErrors.Friendly(result);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int eventId, int id)
        {
            var result = await _api.SendAsync(HttpMethod.Put, $"api/enrolments/{id}/confirm");

            if (result.Success)
            {
                TempData["Success"] = "Enrolment confirmed.";
            }
            else
            {
                TempData["Error"] = ApiErrors.Friendly(result);
            }

            return RedirectToAction("Index", new { eventId });
        }
    }
}