using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}