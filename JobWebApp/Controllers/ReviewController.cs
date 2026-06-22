using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    // Employees rate employers. Forwards to the web service like the other proxies.
    public class ReviewController : Controller
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

        // ── POST: /Review/Submit ───────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Submit(string employerId, int rating, string? reviewText, string? returnUrl)
        {
            // Only employees may leave employer reviews.
            if (!SessionHelper.IsEmployee(HttpContext.Session))
                return RedirectToAction("Login", "Guest");

            Review review = new Review
            {
                UserID = SessionHelper.GetUserID(HttpContext.Session),
                EmployerID = employerId,
                RatingTitle = rating,
                ReviewText = reviewText
            };

            ApiClient<Review> client = BuildClient<Review>("Review", "SubmitReview");
            bool success = await client.PostAsync(review);

            TempData[success ? "Success" : "Error"] = success
                ? "Thanks — your review was submitted."
                : "Couldn't submit your review. Please pick a rating and try again.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("JobCatalog", "Employee");
        }
    }
}
