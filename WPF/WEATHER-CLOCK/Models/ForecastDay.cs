using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEATHER_CLOCK.Models
{
    public sealed class ForecastDay
    {
        public string DayName { get; set; }
        public string Icon { get; set; }
        public string TemperatureText { get; set; }
    }
}
