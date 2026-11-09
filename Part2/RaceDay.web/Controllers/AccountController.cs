using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClient _api;

        public AccountController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.SendAsync(HttpMethod.Post, "api/Auth/register", new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.Role
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, FriendlyError(result));
                return View(model);
            }

            TempData["Success"] = "Account created. Please log in.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _api.SendAsync(HttpMethod.Post, "api/Auth/login", new
            {
                model.Email,
                model.Password
            });

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, FriendlyError(result));
                return View(model);
            }

            try
            {
                using var doc = JsonDocument.Parse(result.Body);
                var userId = doc.RootElement.GetProperty("userId").GetInt32();
                var role = doc.RootElement.GetProperty("role").GetString() ?? "";

                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("Role", role);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Something went wrong. Please try again.");
                return View(model);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.SendAsync(HttpMethod.Post, "api/Auth/logout");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        private static string FriendlyError(ApiResult result)
        {
            if (result.StatusCode == 0)
            {
                return result.Body;
            }

            if (result.StatusCode == 401)
            {
                return "Invalid email or password.";
            }

            if (result.StatusCode == 400
                && !string.IsNullOrWhiteSpace(result.Body)
                && !result.Body.TrimStart().StartsWith("{"))
            {
                return result.Body.Trim('"');
            }

            return "Something went wrong. Please check your details and try again.";
        }
    }
}