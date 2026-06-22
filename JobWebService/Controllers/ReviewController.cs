using JobModels;
using JobWebService.ORM.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace JobWebService.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ReviewController : ControllerBase
    {
        LibraryUOW libraryUOW;

        public ReviewController()
        {
            this.libraryUOW = new LibraryUOW();
        }

        // An employee submits a 1-5 star review of an employer.
        [HttpPost]
        public bool SubmitReview([FromBody] Review review)
        {
            try
            {
                if (review == null
                    || string.IsNullOrWhiteSpace(review.UserID)
                    || string.IsNullOrWhiteSpace(review.EmployerID)
                    || !(review.RatingTitle >= 1 && review.RatingTitle <= 5))
                {
                    Console.WriteLine("SubmitReview failed: missing reviewer/employer or rating out of range.");
                    return false;
                }

                review.ReviewText = review.ReviewText?.Trim();
                review.ReviewDate = DateTime.UtcNow.ToString("O");

                this.libraryUOW.HelperOledb.OpenConnection();

                // Only an employee who was ACCEPTED to one of this employer's jobs may review them.
                if (!HasAcceptedApplication(review.UserID, review.EmployerID))
                {
                    Console.WriteLine("SubmitReview rejected: reviewer was never accepted by this employer.");
                    return false;
                }

                return this.libraryUOW.ReviewRepository.Insert(review);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // All reviews for an employer, newest first.
        [HttpGet]
        public List<Review> GetReviewsForEmployer(string employerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employerId)) return new List<Review>();
                this.libraryUOW.HelperOledb.OpenConnection();
                return this.libraryUOW.ReviewRepository.ReadByEmployer(employerId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return new List<Review>();
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // Used by the UI to decide whether to show the review form.
        [HttpGet]
        public bool CanReview(string userId, string employerId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(employerId)) return false;
                this.libraryUOW.HelperOledb.OpenConnection();
                return HasAcceptedApplication(userId, employerId);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
            finally
            {
                this.libraryUOW.HelperOledb.CloseConnection();
            }
        }

        // True if the employee has at least one Accepted application to a job
        // owned by the given employer. Assumes the DB connection is already open.
        private bool HasAcceptedApplication(string userId, string employerId)
        {
            HashSet<string> employerJobIds = this.libraryUOW.JobRepository.ReadByEmployer(employerId)
                .Select(j => j.JobID)
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)!;

            return this.libraryUOW.JobApplicationRepository.ReadByUserId(userId)
                .Any(a => string.Equals(a.Status, "Accepted", StringComparison.OrdinalIgnoreCase)
                          && employerJobIds.Contains(a.JobId.ToString()));
        }
    }
}
