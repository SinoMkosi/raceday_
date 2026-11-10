using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiClient _api;

        public ParticipantController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var result = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");

            if (!result.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(result);
                return View(new List<MyEnrolmentItem>());
            }

            var list = JsonSerializer.Deserialize<List<MyEnrolmentItem>>(result.Body, JsonSerializerOptions.Web)
                       ?? new List<MyEnrolmentItem>();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            var result = await _api.SendAsync(HttpMethod.Get, "api/Results/mine");

            if (!result.Success)
            {
                ViewBag.Error = ApiErrors.Friendly(result);
                return View(new List<MyResultItem>());
            }

            var list = JsonSerializer.Deserialize<List<MyResultItem>>(result.Body, JsonSerializerOptions.Web)
                       ?? new List<MyResultItem>();

            return View(list);
        }
    }
}