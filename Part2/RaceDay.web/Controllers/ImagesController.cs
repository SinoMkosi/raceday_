using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize]
    public class ImagesController : Controller
    {
        private readonly ApiClient _api;

        public ImagesController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> Get(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

            var image = await _api.GetImageAsync($"api/Images?name={Uri.EscapeDataString(name)}");

            if (image == null)
            {
                return NotFound();
            }

            return File(image.Bytes, image.ContentType);
        }
    }
}