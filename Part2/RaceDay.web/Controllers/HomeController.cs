using Microsoft.AspNetCore.Mvc;
using RaceDay.web.Models;
using RaceDay.Web.Filters;
using RaceDay.Web.Models;
using System.Diagnostics;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        [SessionAuthorize]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") == "Organiser")
            {
                return RedirectToAction("Dashboard", "Organiser");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}