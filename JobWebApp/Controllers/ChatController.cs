using Microsoft.AspNetCore.Mvc;
using JobWebApp;
using JobModels;
using LibraryWSClient;

namespace JobWebApp.Controllers
{
    public class ChatController : Controller
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

        private bool IsAuthorized()
        {
            return SessionHelper.IsLoggedIn(HttpContext.Session);
        }

        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            if (!IsAuthorized()) return Unauthorized();
            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<List<User>> client = BuildClient<List<User>>("Chat", "GetConversations");
            client.AddParameter("userId", userId);
            List<User> conversations = await client.GetAsync() ?? new List<User>();

            return Json(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(string partnerId)
        {
            if (!IsAuthorized()) return Unauthorized();
            string userId = SessionHelper.GetUserID(HttpContext.Session)!;

            ApiClient<List<ChatMessage>> client = BuildClient<List<ChatMessage>>("Chat", "GetChatHistory");
            client.AddParameter("user1", userId);
            client.AddParameter("user2", partnerId);
            List<ChatMessage> history = await client.GetAsync() ?? new List<ChatMessage>();

            return Json(history);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string messageText)
        {
            if (!IsAuthorized()) return Unauthorized();
            string senderId = SessionHelper.GetUserID(HttpContext.Session)!;

            ChatMessage msg = new ChatMessage
            {
                SenderID = senderId,
                ReceiverID = receiverId,
                MessageText = messageText,
                SentAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            ApiClient<ChatMessage> client = BuildClient<ChatMessage>("Chat", "SendMessage");
            bool success = await client.PostAsync(msg);

            return Json(new { success });
        }
    }
}
