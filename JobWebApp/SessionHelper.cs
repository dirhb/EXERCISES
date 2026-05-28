// SessionHelper.cs
// This is a helper class — it's not a controller or a model,
// it's just a utility that makes working with sessions easier.
//
// Instead of writing HttpContext.Session.GetString("UserID")
// everywhere, we write SessionHelper.GetUserID(HttpContext.Session)
// which is much cleaner and harder to make typos in.

namespace JobWebApp
{
    public static class SessionHelper
    {
        // ── Key constants ──────────────────────────────────────
        private const string USER_ID = "UserID";
        private const string USER_TYPE = "UserTypeID";
        private const string USER_NAME = "UserName";
        private const string FULL_NAME = "FullName";

        // ── Save user to session (called after login) ──────────
        public static void SetUser(ISession session, string userId, int userTypeId, string userName, string fullName)
        {
            session.SetString(USER_ID, userId);
            session.SetString(USER_TYPE, userTypeId.ToString());
            session.SetString(USER_NAME, userName);
            session.SetString(FULL_NAME, fullName);
        }

        // ── Read user info from session ────────────────────────
        public static string? GetUserID(ISession session)
        {
            return session.GetString(USER_ID);
        }

        public static int GetUserTypeID(ISession session)
        {
            // TryParse safely converts the string "2" to the number 2
            // If it fails for any reason, it returns 0 (not logged in)
            int.TryParse(session.GetString(USER_TYPE), out int result);
            return result;
        }

        public static string? GetUserName(ISession session)
        {
            return session.GetString(USER_NAME);
        }

        public static string? GetFullName(ISession session)
        {
            return session.GetString(FULL_NAME);
        }

        // ── Check if logged in ─────────────────────────────────
        public static bool IsLoggedIn(ISession session)
        {
            return !string.IsNullOrEmpty(session.GetString(USER_ID));
        }

        // ── Check role ─────────────────────────────────────────
        // UserTypeID: 1 = Guest, 2 = Employee, 3 = Employer, 4 = Admin
        public static bool IsEmployee(ISession session) => GetUserTypeID(session) == 2;
        public static bool IsEmployer(ISession session) => GetUserTypeID(session) == 3;
        public static bool IsAdmin(ISession session) => GetUserTypeID(session) == 4;

        // ── Clear session (called on logout) ───────────────────
        public static void ClearUser(ISession session)
        {
            session.Clear();
        }
    }
}