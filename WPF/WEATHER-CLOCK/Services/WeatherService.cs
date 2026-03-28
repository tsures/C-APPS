using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WEATHER_CLOCK.Models;

namespace WEATHER_CLOCK.Services
{
    public sealed class WeatherService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Example location: New York
        // Replace with your own coordinates.
        private  double Latitude =double.Parse(ConfigurationManager.AppSettings["Latitude"].ToString());
        private  double Longitude = double.Parse(ConfigurationManager.AppSettings["Longitude"].ToString());

        public async Task<OpenMeteoResponse> GetWeatherAsync()
        {
            string url =
                "https://api.open-meteo.com/v1/forecast" +
                "?latitude=" + Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "&longitude=" + Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                "&current_weather=true" +
                "&daily=weathercode,temperature_2m_max,temperature_2m_min" +
                "&temperature_unit=celsius" +
                "&timezone=auto";

            using (var response = await _httpClient.GetAsync(url).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<OpenMeteoResponse>(json);
            }
        }

        public static string GetWeatherIcon(int weatherCode, bool isNight = false)
        {
            switch (weatherCode)
            {
                case 0:
                    return isNight ? "☾" : "☀";
                case 1:
                case 2:
                    return isNight ? "☾" : "⛅";
                case 3:
                    return "☁";
                case 45:
                case 48:
                    return "〰";
                case 51:
                case 53:
                case 55:
                case 61:
                case 63:
                case 65:
                case 80:
                case 81:
                case 82:
                    return "🌧";
                case 71:
                case 73:
                case 75:
                case 77:
                case 85:
                case 86:
                    return "❄";
                case 95:
                case 96:
                case 99:
                    return "⛈";
                default:
                    return "☁";
            }
        }
    }
}
