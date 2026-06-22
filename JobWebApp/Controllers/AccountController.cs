using JobWebApp;
using JobModels;
using LibraryWSClient;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    // Handles edits to the logged-in user's own account (profile + password).
    // Works for any logged-in role; redirects back to that role's profile page.
    public class AccountController : Controller
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

        // ── POST: /Account/UpdateProfile ───────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string email, string? phoneNum, string? country)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Guest");

            User user = new User
            {
                UserID = SessionHelper.GetUserID(HttpContext.Session),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNum = phoneNum,
                Country = country
            };

            bool ok = await BuildClient<User>("User", "UpdateProfile").PostAsync(user);

            if (ok)
            {
                // Keep the session's display name in sync with the new name.
                SessionHelper.SetUser(
                    HttpContext.Session,
                    SessionHelper.GetUserID(HttpContext.Session)!,
                    SessionHelper.GetUserTypeID(HttpContext.Session),
                    SessionHelper.GetUserName(HttpContext.Session)!,
                    $"{firstName} {lastName}".Trim());
            }

            TempData[ok ? "Success" : "Error"] = ok ? "Profile updated." : "Couldn't update your profile.";
            return RedirectToProfile();
        }

        // ── POST: /Account/ChangePassword ──────────────────────
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Guest");

            // Verify the current password before allowing a change.
            ApiClient<bool> checkClient = BuildClient<bool>("User", "CheckPassword");
            checkClient.AddParameter("username", SessionHelper.GetUserName(HttpContext.Session));
            checkClient.AddParameter("password", currentPassword);
            bool valid = await checkClient.GetAsync();

            if (!valid)
            {
                TempData["Error"] = "Your current password is incorrect.";
                return RedirectToProfile();
            }

            User user = new User
            {
                UserID = SessionHelper.GetUserID(HttpContext.Session),
                Password = newPassword
            };
            bool ok = await BuildClient<User>("User", "UpdatePassword").PostAsync(user);

            TempData[ok ? "Success" : "Error"] = ok ? "Password changed." : "Couldn't change your password.";
            return RedirectToProfile();
        }

        // ── POST: /Account/UpdateCurrency ──────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateCurrency(string currency)
        {
            if (!SessionHelper.IsLoggedIn(HttpContext.Session))
                return RedirectToAction("Login", "Guest");

            string code = string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();

            ApiClient<bool> client = BuildClient<bool>("User", "UpdateCurrency");
            client.AddParameter("userId", SessionHelper.GetUserID(HttpContext.Session));
            client.AddParameter("currency", code);
            bool ok = await client.PostAsync();

            if (ok)
                SessionHelper.SetCurrency(HttpContext.Session, code);

            TempData[ok ? "Success" : "Error"] = ok
                ? $"Display currency set to {code}."
                : "Couldn't change your currency.";

            return RedirectToProfile();
        }

        private IActionResult RedirectToProfile()
        {
            if (SessionHelper.IsEmployer(HttpContext.Session)) return RedirectToAction("Profile", "Employer");
            if (SessionHelper.IsAdmin(HttpContext.Session)) return RedirectToAction("Profile", "Admin");
            return RedirectToAction("Profile", "Employee");
        }
    }
}
