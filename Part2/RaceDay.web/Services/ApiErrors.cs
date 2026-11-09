namespace RaceDay.Web.Services
{
    public static class ApiErrors
    {
        public static string Friendly(ApiResult result)
        {
            if (result.StatusCode == 0)
            {
                return result.Body;
            }

            if (result.StatusCode == 401)
            {
                return "Your session has ended. Please log in again.";
            }

            if (result.StatusCode == 404)
            {
                return "That item could not be found.";
            }

            var text = (result.Body ?? "").Trim().Trim('"');

            if ((result.StatusCode == 400 || result.StatusCode == 403)
                && text.Length > 0
                && !text.StartsWith("{"))
            {
                return text;
            }

            return "Something went wrong. Please check your details and try again.";
        }
    }
}