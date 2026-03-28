using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WEATHER_CLOCK.Models
{
    public sealed class DailyUnits
    {
        [JsonProperty("temperature_2m_max")]
        public string TemperatureMax { get; set; }

        [JsonProperty("temperature_2m_min")]
        public string TemperatureMin { get; set; }
    }
}
