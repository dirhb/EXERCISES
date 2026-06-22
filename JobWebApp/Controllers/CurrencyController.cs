using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace JobWebApp.Controllers
{
    // Same-origin proxy for the currency exchange-rate API.
    // The browser detects the user's currency from its locale and asks here for
    // the USD→{currency} rate. We call RapidAPI server-side so the API key stays
    // out of the page, exactly like the Notifications/Chat proxies.
    public class CurrencyController : Controller
    {
        private static readonly HttpClient httpClient = new HttpClient();

        // Cache rates for an hour so we don't hammer the API (free tier is limited).
        private static readonly ConcurrentDictionary<string, (double rate, DateTime fetchedUtc)> cache
            = new ConcurrentDictionary<string, (double, DateTime)>();
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromHours(1);

        private readonly IConfiguration config;

        public CurrencyController(IConfiguration config)
        {
            this.config = config;
        }

        // ── GET: /Currency/Rate?to=EUR ─────────────────────────
        // Returns { rate } = how many of "to" currency one USD is worth.
        // Returns { rate = null } when conversion isn't possible, so the page
        // just keeps showing the original USD amounts.
        [HttpGet]
        public async Task<IActionResult> Rate(string to)
        {
            to = (to ?? "").Trim().ToUpperInvariant();

            // Nothing to convert when the target is USD itself or missing.
            if (string.IsNullOrEmpty(to) || to == "USD")
                return Json(new { rate = 1.0 });

            // Serve from cache if it's still fresh.
            if (cache.TryGetValue(to, out var cached) && DateTime.UtcNow - cached.fetchedUtc < cacheLifetime)
                return Json(new { rate = cached.rate });

            double? rate = await FetchRate(to);
            if (rate.HasValue)
            {
                cache[to] = (rate.Value, DateTime.UtcNow);
                return Json(new { rate = rate.Value });
            }

            return Json(new { rate = (double?)null });
        }

        private async Task<double?> FetchRate(string to)
        {
            try
            {
                string host = config["RapidApi:CurrencyHost"] ?? "";
                string key = config["RapidApi:Key"] ?? "";
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(key))
                    return null;

                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://{host}/latest?base=USD&symbols={to}"),
                    Headers =
                    {
                        { "x-rapidapi-key", key },
                        { "x-rapidapi-host", host },
                    },
                };

                using (HttpResponseMessage response = await httpClient.SendAsync(request))
                {
                    if (!response.IsSuccessStatusCode)
                        return null;

                    string body = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        if (doc.RootElement.TryGetProperty("rates", out JsonElement rates)
                            && rates.TryGetProperty(to, out JsonElement rateElement)
                            && rateElement.TryGetDouble(out double rate))
                        {
                            return rate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Currency rate fetch failed: " + ex.Message);
            }
            return null;
        }
    }
}
