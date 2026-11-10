using System.Net.Http.Headers;
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

    public class ApiImage
    {
        public byte[] Bytes { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
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

        private HttpRequestMessage NewRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);

            var savedCookie = Session.GetString("ApiCookie");
            if (!string.IsNullOrEmpty(savedCookie))
            {
                request.Headers.Add("Cookie", savedCookie);
            }

            return request;
        }

        private void SaveCookie(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var first = cookies.FirstOrDefault();
                if (first != null)
                {
                    Session.SetString("ApiCookie", first.Split(';')[0]);
                }
            }
        }

        private static ApiResult Unreachable()
        {
            return new ApiResult
            {
                Success = false,
                StatusCode = 0,
                Body = "The server could not be reached. Please try again later."
            };
        }

        public async Task<ApiResult> SendAsync(HttpMethod method, string url, object? body = null)
        {
            try
            {
                var request = NewRequest(method, url);

                if (body != null)
                {
                    var json = JsonSerializer.Serialize(body);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await _http.SendAsync(request);
                SaveCookie(response);

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
                return Unreachable();
            }
        }

        public async Task<ApiResult> SendFileAsync(string url, IFormFile file)
        {
            try
            {
                var request = NewRequest(HttpMethod.Post, url);

                await using var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                var form = new MultipartFormDataContent();
                form.Add(fileContent, "file", file.FileName);
                request.Content = form;

                var response = await _http.SendAsync(request);
                SaveCookie(response);

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
                return Unreachable();
            }
        }

        public async Task<ApiImage?> GetImageAsync(string url)
        {
            try
            {
                var request = NewRequest(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();
                var type = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

                return new ApiImage { Bytes = bytes, ContentType = type };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}