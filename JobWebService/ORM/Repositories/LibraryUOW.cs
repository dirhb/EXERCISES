using JobWebService.ORM.ModelFactory;

namespace JobWebService.ORM.Repositories
{
    public class LibraryUOW
    {
        CountryRepository countryRepository;
        EducationRepository educationRepository;
        EducationTypeRepository educationTypeRepository;
        GenreRepository genreRepository;
        JobRepository jobRepository;
        NotificationRepository notificationRepository;
        ReviewRepository reviewRepository;
        UserRepository userRepository;
        UserTypeRepository userTypeRepository;

        DBHelperOledb helperOledb;
        public LibraryUOW()
        {
            this.helperOledb = new DBHelperOledb();
        }
        public DBHelperOledb HelperOledb
        {
            get { return helperOledb; }
        }
        public CountryRepository CountryRepository
        {
            get
            {
                if (countryRepository == null)
                    countryRepository = new CountryRepository(helperOledb);
                return countryRepository;
            }
        }

        public EducationRepository EducationRepository
        {
            get
            {
                if (educationRepository == null)
                    educationRepository = new EducationRepository(helperOledb);
                return educationRepository;
            }
        }

        public EducationTypeRepository EducationTypeRepository
        {
            get
            {
                if (educationTypeRepository == null)
                    educationTypeRepository = new EducationTypeRepository(helperOledb);
                return educationTypeRepository;
            }
        }

        public GenreRepository GenreRepository
        {
            get
            {
                if (genreRepository == null)
                    genreRepository = new GenreRepository(helperOledb);
                return genreRepository;
            }
        }

        public JobRepository JobRepository
        {
            get
            {
                if (jobRepository == null)
                    jobRepository = new JobRepository(helperOledb);
                return jobRepository;
            }
        }

        public NotificationRepository NotificationRepository
        {
            get
            {
                if (notificationRepository == null)
                    notificationRepository = new NotificationRepository(helperOledb);
                return notificationRepository;
            }
        }

        public ReviewRepository ReviewRepository
        {
            get
            {
                if (reviewRepository == null)
                    reviewRepository = new ReviewRepository(helperOledb);
                return reviewRepository;
            }
        }

        public UserRepository UserRepository
        {
            get
            {
                if (userRepository == null)
                    userRepository = new UserRepository(helperOledb);
                return userRepository;
            }
        }

        public UserTypeRepository UserTypeRepository
        {
            get
            {
                if (userTypeRepository == null)
                    userTypeRepository = new UserTypeRepository(helperOledb);
                return userTypeRepository;
            }
        }

    }
}
