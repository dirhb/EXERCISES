using EXERCISES;
using EXERCISES.Models;

namespace JobWebService.ORM
{
    public class ModelCreators
    {
        Countries countries;
        Employee employee;
        Employer employer;
        Genre genre;
        Jobs jobs;
        Notification notification;
        Reviews reviews;

        public Countries Countries
        {
            get
            {
                if (countries == null)
                {
                    countries = new Countries();
                }
                return countries;
            }
        }
        public Employee Employee
        {
            get
            {
                if (employee == null)
                {
                    employee = new Employee();
                }
                return employee;
            }
        }
        public Employer Employer
        {
            get
            {
                if (employer == null)
                {
                    employer = new Employer();
                }
                return employer;
            }
        }
        public Genre Genre
        {
            get
            {
                if (genre == null)
                {
                    genre = new Genre();
                }
                return genre;
            }
        }
        public Jobs Jobs
        {
            get
            {
                if (jobs == null)
                {
                    jobs = new Jobs();
                }
                return jobs;
            }
        }
        public Notification Notification
        {
            get
            {
                if (notification == null)
                {
                    notification = new Notification();
                }
                return notification;
            }
        }
        public Reviews Reviews
        {
            get
            {
                if (reviews == null)
                {
                    reviews = new Reviews();
                }
                return reviews;
            }
        }

    }
}
