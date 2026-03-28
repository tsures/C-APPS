using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WEATHER_CLOCK.Models
{
    public sealed class OpenMeteoResponse
    {
        [JsonProperty("daily")]
        public DailyWeather Daily { get; set; }

        [JsonProperty("current_weather")]
        public CurrentWeather CurrentWeather { get; set; }

        [JsonProperty("daily_units")]
        public DailyUnits DailyUnits { get; set; }
    }

    public sealed class DailyWeather
    {
        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("weathercode")]
        public List<int> WeatherCode { get; set; }

        [JsonProperty("temperature_2m_max")]
        public List<double> TemperatureMax { get; set; }

        [JsonProperty("temperature_2m_min")]
        public List<double> TemperatureMin { get; set; }
    }

    public sealed class CurrentWeather
    {
        [JsonProperty("temperature")]
        public double Temperature { get; set; }

        [JsonProperty("weathercode")]
        public int WeatherCode { get; set; }
    }
}
