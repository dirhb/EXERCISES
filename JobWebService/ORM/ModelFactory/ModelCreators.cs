using JobModels;
using JobWebService.ORM.ModelFactory;

namespace JobWebService.ORM
{
    public class ModelCreators
    {
        CountryCreator countryCreator;
        UserTypeCreator userTypeCreator;
        UserCreator userCreator;
        GenreCreator genreCreator;
        JobCreator jobCreator;
        NotificationCreator notificationCreator;
        ReviewCreator reviewCreator;
        EducationCreator educationCreator;
        EducationTypeCreator educationTypeCreator;

        public CountryCreator CountryCreator
        {
            get
            {
                if (countryCreator == null)
                {
                    this.countryCreator = new CountryCreator();
                }
                return countryCreator;
            }
        }
        public UserTypeCreator UserTypeCreator
        {
            get
            {
                if (userTypeCreator == null)
                {
                    userTypeCreator = new UserTypeCreator();
                }
                return userTypeCreator;
            }
        }
        public UserCreator UserCreator
        {
            get
            {
                if 
                   (userCreator == null)
                    userCreator = new UserCreator();
                return userCreator;
            }
        }
        public GenreCreator GenreCreator
        {
            get
            {
                if (genreCreator == null)
                {
                    genreCreator = new GenreCreator();
                }
                return genreCreator;
            }
        }
        public JobCreator JobCreator
        {
            get
            {
                if (jobCreator == null)
                {
                    jobCreator = new JobCreator();
                }
                return jobCreator;
            }
        }
        public NotificationCreator NotificationCreator
        {
            get
            {
                if (notificationCreator == null)
                {
                    notificationCreator = new NotificationCreator();
                }
                return notificationCreator;
            }
        }
        public ReviewCreator ReviewsCreator
        {
            get
            {
                if (reviewCreator == null)
                {
                    reviewCreator = new ReviewCreator();
                }
                return reviewCreator;
            }
        }
        public EducationCreator EducationCreator
        {
            get
            {
                if (educationCreator == null)
                {
                    educationCreator = new EducationCreator();
                }
                return educationCreator;
            }
        }
        public EducationTypeCreator EducationTypeCreator
        {
            get
            {
                if (educationTypeCreator == null)
                {
                    educationTypeCreator = new EducationTypeCreator();
                }
                return educationTypeCreator;
            }
        }
    }
}
