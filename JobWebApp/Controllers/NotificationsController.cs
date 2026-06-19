using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    // Same-origin proxy for the notification bell.
    // The browser can't call the web service directly (different origin +
    // https→http mixed content), so — exactly like ChatController — the page
    // fetches from here and we forward the request server-side.
    public class NotificationsController : Controller
    {
        private const string WS_HOST = "localhost";
        private const int WS_PORT = 5015;
        private const string WS_SCHEME = "http";

        private ApiClient<T> BuildClient<T>(string controller, string action)
        {
            ApiClient<T> client = new ApiClient<T>();
            client.Scheme = WS_SCHEME;
            client.Host = WS_HOST;
            client.Port = WS_PORT;
            client.Path = $"api/{controller}/{action}";
            return client;
        }

        // ── GET: /Notifications/Get ────────────────────────────
        // Returns the notifications this logged-in user should see
        // (broadcasts + messages targeted at them) as JSON.
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return Json(new List<Notification>());

            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<List<Notification>> client = BuildClient<List<Notification>>("Admin", "GetNotifications");
            client.AddParameter("userId", userId);
            List<Notification> notifications = await client.GetAsync() ?? new List<Notification>();

            return Json(notifications);
        }
    }
}
