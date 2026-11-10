using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class ImagesController : ControllerBase
    {
        private readonly BlobStorageService _blobs;

        public ImagesController(BlobStorageService blobs)
        {
            _blobs = blobs;
        }

        /// <summary>Gets an image from Blob Storage by its name. Logged-in users only.</summary>
        /// <response code="200">Returns the image.</response>
        /// <response code="400">Invalid image name.</response>
        /// <response code="404">Image not found.</response>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string name)
        {
            var valid = !string.IsNullOrWhiteSpace(name)
                        && !name.Contains("..")
                        && (name.StartsWith("banners/") || name.StartsWith("profiles/"));

            if (!valid)
            {
                return BadRequest("Invalid image name.");
            }

            if (!_blobs.IsConfigured)
            {
                return NotFound();
            }

            try
            {
                var image = await _blobs.OpenAsync(name);

                if (image == null)
                {
                    return NotFound();
                }

                return File(image.Value.Content, image.Value.ContentType);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}