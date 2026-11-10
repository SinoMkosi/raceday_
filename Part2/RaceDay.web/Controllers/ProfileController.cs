using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Participant")]
    public class ProfileController : Controller
    {
        private readonly ApiClient _api;

        public ProfileController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _api.SendAsync(HttpMethod.Get, "api/Profile");

            if (!result.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(result);
                return View(new ProfileViewModel());
            }

            var model = JsonSerializer.Deserialize<ProfileViewModel>(result.Body, JsonSerializerOptions.Web)
                        ?? new ProfileViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.SendAsync(HttpMethod.Put, "api/Profile", new
            {
                model.FirstName,
                model.LastName,
                model.Email
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, ApiErrors.Friendly(result));
                return View(model);
            }

            TempData["Success"] = "Profile updated.";
            return RedirectToAction("Index");
        }
    }
}