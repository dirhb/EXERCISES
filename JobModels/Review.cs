namespace JobModels
{
    public class Review
    {
        public string? ReviewID { get; set; }
        public string? ReviewText { get; set; }   // stored in the ReviewsText column
        public int? RatingTitle { get; set; }      // 1-5 star rating
        public string? UserID { get; set; }         // reviewer (the employee)
        public string? EmployerID { get; set; }     // employer being reviewed
        public string? ReviewDate { get; set; }
        public string? ReviewerName { get; set; }    // resolved for display, not stored
    }
}
