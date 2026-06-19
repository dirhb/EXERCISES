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
        JobTypeCreator jobTypeCreator;
        JobApplicationCreator jobApplicationCreator;
        NotificationCreator notificationCreator;
        ReviewCreator reviewCreator;
        EducationCreator educationCreator;
        EducationTypeCreator educationTypeCreator;
        ChatMessageCreator chatMessageCreator;

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
        public JobTypeCreator JobTypeCreator
        {
            get
            {
                if (jobTypeCreator == null)
                {
                    jobTypeCreator = new JobTypeCreator();
                }
                return jobTypeCreator;
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
        public JobApplicationCreator JobApplicationCreator
        {
            get
            {
                if (jobApplicationCreator == null)
                {
                    jobApplicationCreator = new JobApplicationCreator();
                }
                return jobApplicationCreator;
            }
        }
        public ChatMessageCreator ChatMessageCreator
        {
            get
            {
                if (chatMessageCreator == null)
                {
                    chatMessageCreator = new ChatMessageCreator();
                }
                return chatMessageCreator;
            }
        }
    }
}
