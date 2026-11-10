using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Dtos;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly BlobStorageService _blobs;

        public ProfileController(AppDbContext db, BlobStorageService blobs)
        {
            _db = db;
            _blobs = blobs;
        }

        /// <summary>Gets the profile of the logged-in user.</summary>
        /// <response code="200">Returns the profile.</response>
        /// <response code="401">Not logged in.</response>
        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            return Ok(new { user.UserId, user.FirstName, user.LastName, user.Email, user.Role, user.ProfilePictureUrl });
        }

        /// <summary>Updates the profile of the logged-in user.</summary>
        /// <response code="200">Profile updated.</response>
        /// <response code="400">Email already used by someone else.</response>
        /// <response code="401">Not logged in.</response>
        [HttpPut]
        public async Task<IActionResult> UpdateMyProfile(UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.UserId != userId))
                return BadRequest("Email is already used by another account.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Email = dto.Email;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated." });
        }

        /// <summary>Uploads or replaces the profile picture of the logged-in user.</summary>
        /// <response code="200">Picture saved.</response>
        /// <response code="400">Not a valid image.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="503">Image storage is not available.</response>
        [HttpPost("picture")]
        public async Task<IActionResult> UploadPicture(IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null) return NotFound();

            var problem = BlobStorageService.Validate(file);
            if (problem != null) return BadRequest(problem);

            if (!_blobs.IsConfigured)
                return StatusCode(503, "Image storage is not available right now.");

            try
            {
                var oldName = user.ProfilePictureUrl;
                user.ProfilePictureUrl = await _blobs.UploadAsync(file, "profiles");
                await _db.SaveChangesAsync();
                await _blobs.DeleteAsync(oldName);

                return Ok(new { user.ProfilePictureUrl });
            }
            catch (Exception)
            {
                return StatusCode(503, "Image storage is not available right now.");
            }
        }
    }
}