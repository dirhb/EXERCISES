using System.Drawing;

namespace EXERCISES
{
    internal class Program
    {

        static void T1()
        {
            //way number 1
            Employee employee1 = new Employee()
            {
                Id = "123456789",
                FullName = "Ofek Cockhen",
                PhoneNumber = 1234567890,
            };
            //way number 2
            Employee employee2 = new Employee();
            employee2.Id = "123456789";
            employee2.FullName = "Danie vladimirov";
            employee2.PhoneNumber = 987654321;

            List<Employee> Employees = new List<Employee>();
            Employees.Add(employee1);
            Employees.Add(employee2);
            Employer employer1 = new Employer()
            {
                FullName = "ofri Jambar",
                Id = "12345",
                PhoneNumber = 12345678,
                Employees = new List<Employee>()
            };
            List<Person> Persons = new List<Person>();
            Persons.Add(employer1);
            Persons.Add(employee1);
            Persons.Add(employee2);
            foreach (Person per in Persons)
            {
                Console.WriteLine(per.FullName);
            }
            Console.WriteLine();
            Console.WriteLine("Only Employer");
            Console.WriteLine();
            foreach (Person per in Persons)
            {
                if (per is Employer)
                {
                    Console.WriteLine(per.FullName);
                }
            }
            Console.ReadLine();
        }
        static void Main(string[] args)
        {
            Task t1 = PrintAsync(6, 10);
            Task t2 = PrintAsync(7, 10);
            Task.WaitAll(t1, t2);
            Console.WriteLine();
            Console.WriteLine("I have finished");
            Console.ReadLine();
        }
        static async Task PrintAsync(int color, int interval)
        {
            ConsoleColor c = (ConsoleColor)color;
            Console.ForegroundColor = c;
            for (int i = 1; i <= 100; i++)
            {
                Console.Write("█");
                await Task.Delay(interval);
            }
        }
    }
}
