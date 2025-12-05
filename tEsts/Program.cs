using System.Net.Http.Headers;
using System.IO.IsolatedStorage;
using System.Net.Http.Headers;
using System.Text.Json;
namespace tEsts
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CompanyJobSalary();
            Console.ReadLine();
        }

        static async Task CompanyJobSalary() //השירות פותר את הצורך בסינון מדויק של נתוני שכר. הוא מאפשר להזין חברה, תפקיד, מיקום ורמת ניסיון — וככה לקבל מידע מותאם אישית במקום לחפש נתונים כלליים ולא רלוונטיים.
        {
            Console.WriteLine("insert company");
            string company = Console.ReadLine(); //The company name for which to get salary information (e.g. Amazon).
            Console.WriteLine("insert job title");
            string job_title = Console.ReadLine(); ; //Job title for which to get salary estimation.
            Console.WriteLine("insert location");
            string location = Console.ReadLine(); ; //Free-text location/area in which to get salary estimation.
            Console.WriteLine("insert location type");
            string location_type = Console.ReadLine(); ; //Allowed values: ANY, CITY, STATE, COUNTRY
            if (location_type == ""){
                location_type = "ANY";
            }
            Console.WriteLine("insert years_of_experience");
            string years_of_experience = Console.ReadLine(); ; //Allowed values: ALL, LESS_THAN_ONE, ONE_TO_THREE, FOUR_TO_SIX, SEVEN_TO_NINE, TEN_TO_FOURTEEN, ABOVE_FIFTEEN
            if (years_of_experience == "")
            {
                years_of_experience = "ALL";
            }
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://job-salary-data.p.rapidapi.com/company-job-salary?company={company}&job_title={job_title}&location={location}&location_type={location_type}&years_of_experience={years_of_experience}"),
                Headers =
                {
                    { "x-rapidapi-key", "6af41fd3c5mshad393aab0dc6c95p14267djsn82645298eb38" },
                    { "x-rapidapi-host", "job-salary-data.p.rapidapi.com" },
                },
            };
            
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
           
                Console.WriteLine(body);
            }
        }
    }
}