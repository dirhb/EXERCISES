using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    // Handles contextual reports: an employee reporting a job, or an employer
    // reporting an applicant. Triggered from a shared modal; forwards to the
    // web service like the other proxies.
    public class ReportController : Controller
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

        // Only logged-in, non-admin users may file a report.
        private bool CanReport()
        {
            return SessionHelper.IsLoggedIn(HttpContext.Session)
                && !SessionHelper.IsAdmin(HttpContext.Session);
        }

        // ── POST: /Report/Submit ───────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Submit(string targetType, string targetId, string category, string reportText, string subject, string? returnUrl)
        {
            if (!CanReport())
                return RedirectToAction("Login", "Guest");

            Report report = new Report
            {
                ReporterUserID = SessionHelper.GetUserID(HttpContext.Session),
                TargetType = targetType,
                TargetID = targetId,
                Subject = subject,
                Category = category,
                ReportText = reportText
            };

            ApiClient<Report> client = BuildClient<Report>("Report", "SubmitReport");
            bool success = await client.PostAsync(report);

            TempData[success ? "Success" : "Error"] = success
                ? "Thanks — your report was sent to the admins."
                : "Could not submit your report. Please try again.";

            // Return the user to the page they reported from.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Home", "Guest");
        }
    }
}
