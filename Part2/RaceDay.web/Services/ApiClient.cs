using System.Text;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    public class ApiResult
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _accessor;

        public ApiClient(HttpClient http, IHttpContextAccessor accessor)
        {
            _http = http;
            _accessor = accessor;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public async Task<ApiResult> SendAsync(HttpMethod method, string url, object? body = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, url);

                var savedCookie = Session.GetString("ApiCookie");
                if (!string.IsNullOrEmpty(savedCookie))
                {
                    request.Headers.Add("Cookie", savedCookie);
                }

                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _http.SendAsync(request);

                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    var first = cookies.FirstOrDefault();
                    if (first != null)
                    {
                        Session.SetString("ApiCookie", first.Split(';')[0]);
                    }
                }

                var text = await response.Content.ReadAsStringAsync();

                return new ApiResult
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Body = text
                };
            }
            catch (Exception)
            {
                return new ApiResult
                {
                    Success = false,
                    StatusCode = 0,
                    Body = "The server could not be reached. Please try again later."
                };
            }
        }
    }
}
