using JobModels;

namespace JobWebService.Controllers
{
    // temporary memory store for use-cases that don't have DB tables yet
    public static class UseCaseMemoryStore
    {
        public static List<JobApplicationLog> Applications = new List<JobApplicationLog>();
        public static Dictionary<string, string> UserResumes = new Dictionary<string, string>();
        public static Dictionary<string, decimal> UserSalaries = new Dictionary<string, decimal>();
        public static Dictionary<string, List<string>> UserJobHistory = new Dictionary<string, List<string>>();
    }

    public class JobApplicationLog
    {
        public string? ApplicationID { get; set; }
        public string? JobID { get; set; }
        public string? UserID { get; set; }
        public string? ResumeText { get; set; }
        public string? Status { get; set; }
        public string? CreatedAt { get; set; }
    }
}