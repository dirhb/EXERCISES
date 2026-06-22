namespace JobModels
{
    public class Report
    {
        public string? ReportID { get; set; }
        public string? ReporterUserID { get; set; }
        public string? TargetType { get; set; }   // "Job" | "Applicant"
        public string? TargetID { get; set; }     // the reported job's / user's id
        public string? Category { get; set; }      // reason, e.g. "Offensive content"
        public string? Subject { get; set; }       // snapshot label (job title / applicant name)
        public string? ReportText { get; set; }    // free-text details
        public string? Status { get; set; }        // "Open" | "Resolved"
        public string? ReportDate { get; set; }
    }
}
