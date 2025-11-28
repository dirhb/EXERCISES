using System.IO.IsolatedStorage;
using System.Net.Http.Headers;
using System.Text.Json;
namespace tEsts
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConvertCurrency();
            Console.ReadLine(); 
        }

        static async void ViewCurrencyList()
        {
          
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://currency-converter18.p.rapidapi.com/api/v1/supportedCurrencies"),
                Headers =
    {
        { "x-rapidapi-key", "6af41fd3c5mshad393aab0dc6c95p14267djsn82645298eb38" },
        { "x-rapidapi-host", "currency-converter18.p.rapidapi.com" },
    },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                List<Currency> list = JsonSerializer.Deserialize<List<Currency>>(body);
                foreach(Currency currency in list)
                {
                    Console.WriteLine($"{currency.symbol} - {currency.name}");
                }
                
            }
        }

        static async void ConvertCurrency()
        {

            string from = "EUR";
            string to = "USD";
            int amount = 1000;
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://currency-converter18.p.rapidapi.com/api/v1/convert?from={from}&to={to}&amount={amount}"),
                Headers =
                {
                    { "x-rapidapi-key", "6af41fd3c5mshad393aab0dc6c95p14267djsn82645298eb38" },
                    { "x-rapidapi-host", "currency-converter18.p.rapidapi.com" },
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
//https://rapidapi.com/letscrape-6bRBa3QguO5/api/job-salary-data/playground/apiendpoint_15b153c8-1cc8-488b-acd9-7083754ce94c
