using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using WEATHER_CLOCK.Infrastructure;
using WEATHER_CLOCK.Models;
using WEATHER_CLOCK.Services;

namespace WEATHER_CLOCK.ViewModels
{
    public sealed class MainViewModel : ViewModelBase
    {
        private readonly WeatherService _weatherService;
        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _weatherTimer;
        private readonly IReverseGeocodingService _reverseGeocodingService;
        private readonly double lat = double.Parse(ConfigurationManager.AppSettings["Latitude"].ToString());
        private readonly double longitude = double.Parse(ConfigurationManager.AppSettings["Longitude"].ToString());

        private string _timeText;
        public string TimeText
        {
            get { return _timeText; }
            set { SetProperty(ref _timeText, value); }
        }

        private string _dateText;
        public string DateText
        {
            get { return _dateText; }
            set { SetProperty(ref _dateText, value); }
        }

        private string _currentTemperature;
        public string CurrentTemperature
        {
            get { return _currentTemperature; }
            set { SetProperty(ref _currentTemperature, value); }
        }

        private string _highLowText;
        public string HighLowText
        {
            get { return _highLowText; }
            set { SetProperty(ref _highLowText, value); }
        }

        private string _weatherIcon;
        public string WeatherIcon
        {
            get { return _weatherIcon; }
            set { SetProperty(ref _weatherIcon, value); }
        }

        private string _locationText;
        public string LocationText
        {
            get { return _locationText; }
            set { SetProperty(ref _locationText, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public ObservableCollection<ForecastDay> Forecast { get; private set; }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ?? (_refreshCommand = new RelayCommand(async () => await RefreshWeatherAsync(), () => !IsBusy));
            }
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new RelayCommand(() =>
                {
                    CloseAction?.Invoke();
                }));
            }
        }

        public Action CloseAction { get; set; }

        public MainViewModel()
        {
            _weatherService = new WeatherService();
            Forecast = new ObservableCollection<ForecastDay>();
            LocationText = "Auto-updating internet weather";

            UpdateClock();

            _clockTimer = new DispatcherTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) => UpdateClock();
            _clockTimer.Start();

            _weatherTimer = new DispatcherTimer();
            _weatherTimer.Interval = TimeSpan.FromMinutes(15);
            _weatherTimer.Tick += async (s, e) => await RefreshWeatherAsync();
            _weatherTimer.Start();

            _ = RefreshWeatherAsync();
            _reverseGeocodingService = new ReverseGeocodingService();
            _ = InitializeAsync();


        }
        private async Task InitializeAsync()
        {          
            LocationText = await _reverseGeocodingService
                .GetSettlementAsync(lat, longitude);
        }


        private void UpdateClock()
        {
            DateTime now = DateTime.Now;
            TimeText = now.ToString("HH:mm", CultureInfo.InvariantCulture);
            DateText = now.ToString("dd MMMM yy", CultureInfo.InvariantCulture);
        }

        private async Task RefreshWeatherAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;
                RaiseRefreshCanExecute();

                var result = await _weatherService.GetWeatherAsync();

                if (result == null || result.Daily == null || result.CurrentWeather == null)
                    return;

                CurrentTemperature = Math.Round(result.CurrentWeather.Temperature).ToString(CultureInfo.InvariantCulture) + "°";
                WeatherIcon = WeatherService.GetWeatherIcon(result.CurrentWeather.WeatherCode, DateTime.Now.Hour >= 19 || DateTime.Now.Hour < 6);

                if (result.Daily.TemperatureMax != null &&
                    result.Daily.TemperatureMin != null &&
                    result.Daily.TemperatureMax.Count > 0 &&
                    result.Daily.TemperatureMin.Count > 0)
                {
                    HighLowText =
                        "H" + Math.Round(result.Daily.TemperatureMax[0]).ToString(CultureInfo.InvariantCulture) + "°  " +
                        "L" + Math.Round(result.Daily.TemperatureMin[0]).ToString(CultureInfo.InvariantCulture) + "°";
                }

                Forecast.Clear();

                int count = Math.Min(5, result.Daily.Time.Count);
                for (int i = 0; i < count; i++)
                {
                    DateTime parsedDate;
                    if (!DateTime.TryParse(result.Daily.Time[i], out parsedDate))
                        parsedDate = DateTime.Today.AddDays(i);

                    Forecast.Add(new ForecastDay
                    {
                        DayName = parsedDate.ToString("ddd", CultureInfo.InvariantCulture),
                        Icon = WeatherService.GetWeatherIcon(result.Daily.WeatherCode[i]),
                        TemperatureText =
                            Math.Round(result.Daily.TemperatureMax[i]).ToString(CultureInfo.InvariantCulture) + "° / " +
                            Math.Round(result.Daily.TemperatureMin[i]).ToString(CultureInfo.InvariantCulture) + "°"
                    });
                }
            }
            catch
            {
                LocationText = "Weather update failed";
            }
            finally
            {
                IsBusy = false;
                RaiseRefreshCanExecute();
            }
        }

        private void RaiseRefreshCanExecute()
        {
            var relay = _refreshCommand as RelayCommand;
            if (relay != null)
                relay.RaiseCanExecuteChanged();
        }
    }
}
