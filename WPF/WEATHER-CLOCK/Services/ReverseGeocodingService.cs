using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WEATHER_CLOCK.Services
{
    public interface IReverseGeocodingService
    {
        Task<string> GetSettlementAsync(double latitude, double longitude);
    }

    public sealed class ReverseGeocodingService : IReverseGeocodingService
    {
        private static readonly HttpClient _httpClient = CreateHttpClient();

        private double? _lastLatitude;
        private double? _lastLongitude;
        private string _lastSettlement;

        static ReverseGeocodingService()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public async Task<string> GetSettlementAsync(double latitude, double longitude)
        {
            if (_lastLatitude.HasValue &&
                _lastLongitude.HasValue &&
                Math.Abs(_lastLatitude.Value - latitude) < 0.0001 &&
                Math.Abs(_lastLongitude.Value - longitude) < 0.0001 &&
                !string.IsNullOrWhiteSpace(_lastSettlement))
            {
                return _lastSettlement;
            }

            string url =
                "https://nominatim.openstreetmap.org/reverse" +
                "?format=jsonv2" +
                "&lat=" + latitude.ToString(CultureInfo.InvariantCulture) +
                "&lon=" + longitude.ToString(CultureInfo.InvariantCulture) +
                "&addressdetails=1" +
                "&zoom=10";

            using (var response = await _httpClient.GetAsync(url).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                string json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                ReverseGeocodeResponse result = JsonConvert.DeserializeObject<ReverseGeocodeResponse>(json);

                string settlement = ExtractSettlement(result);

                _lastLatitude = latitude;
                _lastLongitude = longitude;
                _lastSettlement = settlement;

                return settlement;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            client.DefaultRequestHeaders.UserAgent.ParseAdd("WEATHER_CLOCK/1.0");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");

            return client;
        }

        private static string ExtractSettlement(ReverseGeocodeResponse result)
        {
            if (result == null || result.Address == null)
                return "Unknown location";

            AddressParts address = result.Address;

            if (!string.IsNullOrWhiteSpace(address.City))
                return address.City;

            if (!string.IsNullOrWhiteSpace(address.Town))
                return address.Town;

            if (!string.IsNullOrWhiteSpace(address.Village))
                return address.Village;

            if (!string.IsNullOrWhiteSpace(address.Municipality))
                return address.Municipality;

            if (!string.IsNullOrWhiteSpace(address.Suburb))
                return address.Suburb;

            if (!string.IsNullOrWhiteSpace(address.County))
                return address.County;

            return "Unknown location";
        }

        private sealed class ReverseGeocodeResponse
        {
            [JsonProperty("address")]
            public AddressParts Address { get; set; }
        }

        private sealed class AddressParts
        {
            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("town")]
            public string Town { get; set; }

            [JsonProperty("village")]
            public string Village { get; set; }

            [JsonProperty("municipality")]
            public string Municipality { get; set; }

            [JsonProperty("suburb")]
            public string Suburb { get; set; }

            [JsonProperty("county")]
            public string County { get; set; }
        }
    }
}
