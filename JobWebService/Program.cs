
using JobWebService.ORM;
using JobWebService.ORM.Repositories;
using JobModels;

namespace JobWebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // database helper and model creators
            builder.Services.AddSingleton<DBHelperOledb>();
            builder.Services.AddSingleton<ModelCreators>();

            // repository registrations (interface -> implementation)
            builder.Services.AddScoped<IRepository<User>, UserRepository>();
            builder.Services.AddScoped<IRepository<Job>, JobRepository>();
            builder.Services.AddScoped<IRepository<Genre>, GenreRepository>();
            builder.Services.AddScoped<IRepository<Review>, ReviewRepository>();
            builder.Services.AddScoped<IRepository<Notification>, NotificationRepository>();
            builder.Services.AddScoped<IRepository<Country>, CountryRepository>();
            builder.Services.AddScoped<IRepository<Education>, EducationRepository>();
            builder.Services.AddScoped<IRepository<EducationType>, EducationTypeRepository>();
            builder.Services.AddScoped<IRepository<UserType>, UserTypeRepository>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
