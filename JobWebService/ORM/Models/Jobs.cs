using ModelLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXERCISES
{
    public class Jobs : Model
    {

        string jobID;
        string jobTitle;
        string jobDescription;
        string jobType;
        bool jobStatus;
        string jobFilter;
        string employerID;
        string countryID;

        public Jobs()
        {
        }

        public Jobs(string JobID, string JobTitle, string JobDescription, string JobType, bool JobStatus, string JobFilter, string EmployerID, string CountryID)
        {

            this.jobID = JobID;
            this.JobTitle = JobTitle;
            this.jobDescription = JobDescription;
            this.jobType = JobType;
            this.jobStatus = JobStatus;
            this.jobFilter = JobFilter;
            this.employerID = EmployerID;
            this.countryID = CountryID;
        }

        public string JobID
        {
            get { return this.jobID; }
            set
            {
                jobID = value;
                
            }
        }
        [Required(ErrorMessage = "Job Title is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Job Title cannot be less than 3 characters")]
        public string JobTitle
        {
            get { return this.jobTitle; }
            set { this.jobTitle = value; ValidateProperty(value, "JobTitle"); }
        }

        [Required(ErrorMessage = "Job Description is required")]
        [StringLength(67, MinimumLength = 10, ErrorMessage = "Job Description cannot be less than 3 characters")]
        public string JobDescription
        {
            get { return this.jobDescription; }
            set { this.jobDescription = value; ValidateProperty(value, "JobDescription"); }
        }

        [Required(ErrorMessage = "Job Type is required")]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Job Description cannot be less than 5 characters")]
        public string JobType
        {
            get { return this.jobType; }
            set { this.jobType = value; ValidateProperty(value, "JobType"); }
        }

        public string JobStatus
        {
            get { return this.JobStatus; }
            set { this.JobStatus = value; ValidateProperty(value, "JobStatus"); }
        }

        [Required(ErrorMessage = "Job Filter is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Job Filter cannot be less than 3 characters")]
        public string JobFilter
        {
            get { return this.JobFilter; }
            set { this.JobFilter = value; ValidateProperty(value, "JobFilter"); }
        }

        public string EmployerID
        {
            get { return this.EmployerID; }
            set { this.EmployerID = value; }
        }
        public string CountryID
        {
            get { return this.CountryID; }
            set { this.CountryID = value;}
        }
    }
}
