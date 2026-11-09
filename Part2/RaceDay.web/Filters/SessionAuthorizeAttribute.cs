using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string? _role;

        public SessionAuthorizeAttribute(string? role = null)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var role = session.GetString("Role");

            if (userId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_role != null && role != _role)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}