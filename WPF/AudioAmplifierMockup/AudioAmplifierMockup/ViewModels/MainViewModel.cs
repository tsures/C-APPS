using AudioAmplifierMockup.Audio;
using AudioAmplifierMockup.Models;
using AudioAmplifierMockup.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Linq;
using AudioAmplifierMockup.Audio;

namespace AudioAmplifierMockup.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        #region Equalizer
        private readonly AudioAnalyzerHelper _audioAnalyzer = new AudioAnalyzerHelper();

        public ObservableCollection<float> EqualizerValues { get; } =
            new ObservableCollection<float>(Enumerable.Repeat(5f, 32));

        //private void StartAnalyzer()
        //{
        //    _audioAnalyzer.EqualizerUpdated += values =>
        //    {
        //        App.Current.Dispatcher.Invoke(() =>
        //        {
        //            for (int i = 0; i < values.Length; i++)
        //                EqualizerValues[i] = values[i];
        //        });
        //    };

        //    _audioAnalyzer.Start();
        //}

        private bool _isClosing;

        private void StartAnalyzer()
        {
            _audioAnalyzer.EqualizerUpdated += AudioAnalyzer_EqualizerUpdated;
            _audioAnalyzer.Start();
        }

        private void AudioAnalyzer_EqualizerUpdated(float[] values)
        {
            if (_isClosing || values == null)
                return;

            var dispatcher = App.Current?.Dispatcher;

            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                return;

            dispatcher.BeginInvoke(new Action(() =>
            {
                if (_isClosing)
                    return;

                int count = Math.Min(values.Length, EqualizerValues.Count);

                for (int i = 0; i < count; i++)
                    EqualizerValues[i] = values[i];
            }));
        }

        private void StopAnalyzer()
        {
            _isClosing = true;

            _audioAnalyzer.EqualizerUpdated -= AudioAnalyzer_EqualizerUpdated;
            _audioAnalyzer.Stop();
        }
        #endregion
        public double VolumeKnobAngle
        {
            get
            {
                // Volume 1-100 mapped to -135 to +135 degrees
                return -135 + ((Volume - 1) / 99.0 * 270.0);
            }
        }
        private readonly IAudioDeviceService _audioDeviceService;
        private readonly DispatcherTimer _timer;

        private AudioDeviceInfo _selectedDevice;
        private string _currentDeviceName;
        private string _dateTimeText;
        private int _volume;
        private string _statusText;
        private bool _isBusy;

        private ICommand _refreshCommand;
        private ICommand _setDefaultDeviceCommand;
        private ICommand _volumeUpCommand;
        private ICommand _volumeDownCommand;
        private ICommand _setSelectedDeviceCommand;
        public Action CloseApp;
        private ICommand _close;
        private ICommand _nextDeviceCommand;
        private bool _isMuted;
        private int _previousVolume = 50;
        private ICommand _muteCommand;

        public ICommand MuteCommand =>
            _muteCommand ??= new RelayCommand(ToggleMute);

        private void ToggleMute()
        {
            // mute
            if (!_isMuted)
            {
                _previousVolume = Volume;

                Volume = 0;

                _isMuted = true;

                StatusText = "Muted";
            }
            // unmute
            else
            {
                Volume = _previousVolume;

                _isMuted = false;

                StatusText = "Unmuted";
            }

            OnPropertyChanged(nameof(Volume));
            OnPropertyChanged(nameof(StatusText));
        }

        public ICommand NextDeviceCommand =>
            _nextDeviceCommand ??= new RelayCommand(SetNextDeviceAsDefault);

        private void SetNextDeviceAsDefault()
        {
            if (Devices == null || Devices.Count == 0)
                return;

            // current selected device index
            int currentIndex = Devices.IndexOf(SelectedDevice);

            // if nothing selected start from first
            if (currentIndex < 0)
                currentIndex = 0;

            // next device with wrap-around
            int nextIndex = (currentIndex + 1) % Devices.Count;

            // select next device
            SelectedDevice = Devices[nextIndex];

            // set as default
            SetDefaultDeviceCommand.Execute(null);
        }

        public ICommand Close => _close ?? new RelayCommand(CloseCommand);

        private void CloseCommand()
        {
            StopAnalyzer();
            CloseApp?.Invoke();
        }

        public ICommand SetSelectedDeviceCommand =>
            _setSelectedDeviceCommand ??= new RelayCommand<AudioDeviceInfo>(device =>
            {
                if (device == null)
                    return;

                SelectedDevice = device;
                SetDefaultDeviceCommand.Execute(null);
            });

        public MainViewModel()
            : this(new AudioDeviceService())
        {
        }

        public MainViewModel(IAudioDeviceService audioDeviceService)
        {
            if (audioDeviceService == null)
                throw new ArgumentNullException(nameof(audioDeviceService));

            _audioDeviceService = audioDeviceService;
           
            Devices = new ObservableCollection<AudioDeviceInfo>();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
          
            Refresh();

            StartAnalyzer();
        }

        public ObservableCollection<AudioDeviceInfo> Devices { get; private set; }

        public AudioDeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                    RaiseCommandStates();
            }
        }

        public string CurrentDeviceName
        {
            get => _currentDeviceName;
            private set => SetProperty(ref _currentDeviceName, value);
        }

        public string DateTimeText
        {
            get => _dateTimeText;
            private set => SetProperty(ref _dateTimeText, value);
        }

        private string _timeText;
        public string TimeText
        {
            get => _timeText;
            private set => SetProperty(ref _timeText, value);
        }

        public int Volume
        {
            get => _volume;
            set
            {
                int safeValue = Clamp(value);

                if (SetProperty(ref _volume, safeValue))
                {
                    TrySetVolume(safeValue);
                    OnPropertyChanged(nameof(VolumeDisplay));
                    OnPropertyChanged(nameof(VolumeKnobAngle));
                }
            }
        }

        public string VolumeDisplay
        {
            get { return Volume.ToString("000"); }
        }

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    RaiseCommandStates();
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return _refreshCommand ??
                       (_refreshCommand = new RelayCommand(Refresh, () => !IsBusy));
            }
        }

        public ICommand SetDefaultDeviceCommand
        {
            get
            {
                return _setDefaultDeviceCommand ??
                       (_setDefaultDeviceCommand = new RelayCommand(
                           SetDefaultDevice,
                           () => !IsBusy && SelectedDevice != null));
            }
        }

        public ICommand VolumeUpCommand
        {
            get
            {
                return _volumeUpCommand ??
                       (_volumeUpCommand = new RelayCommand(
                           () => Volume += 1,
                           () => !IsBusy && Volume < 100));
            }
        }

     private ICommand _semiMuteCommand;

public ICommand SemiMute
{
    get
    {
        return _semiMuteCommand ??
               (_semiMuteCommand = new RelayCommand(
                   () => Volume = 20,
                   () => !IsBusy && Volume != 20));
    }
}

        public ICommand VolumeDownCommand
        {
            get
            {
                return _volumeDownCommand ??
                       (_volumeDownCommand = new RelayCommand(
                           () => Volume -= 1,
                           () => !IsBusy && Volume > 1));
            }
        }

     








        private void Refresh()
        {
            try
            {
                IsBusy = true;
                StatusText = "Scanning audio outputs...";

                Devices.Clear();

                foreach (AudioDeviceInfo device in _audioDeviceService.GetOutputDevices())
                    Devices.Add(device);

                AudioDeviceInfo defaultDevice = _audioDeviceService.GetDefaultOutputDevice();

                CurrentDeviceName = defaultDevice.Name;
                SelectedDevice = Devices.FirstOrDefault(x => x.Id == defaultDevice.Id);

                _volume = _audioDeviceService.GetMasterVolume();
                OnPropertyChanged(nameof(Volume));
                OnPropertyChanged(nameof(VolumeDisplay));

                UpdateDateTime();

                StatusText = "Ready";
            }
            catch (Exception ex)
            {
                StatusText = "Audio error: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        //private void SetDefaultDevice()
        //{
        //    if (SelectedDevice == null)
        //        return;

        //    try
        //    {
        //        IsBusy = true;
        //        StatusText = "Switching output...";

        //        _audioDeviceService.SetDefaultOutputDevice(SelectedDevice.Id);

        //        CurrentDeviceName = SelectedDevice.Name;
        //        StatusText = "Default output changed";
        //    }
        //    catch (Exception ex)
        //    {
        //        StatusText = "Could not change output: " + ex.Message;
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //    }
        //}

        private void SetDefaultDevice()
        {
            if (SelectedDevice == null)
                return;

            try
            {
                IsBusy = true;
                StatusText = "Switching output...";

                _audioDeviceService.SetDefaultOutputDevice(SelectedDevice.Id);

                CurrentDeviceName = SelectedDevice.Name;

                // restart analyzer on new device
               // _audioAnalyzer.Stop();
                //_audioAnalyzer.Start();

                StatusText = "Default output changed";
            }
            catch (Exception ex)
            {
                StatusText = "Could not change output: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void TrySetVolume(int volume)
        {
            try
            {
                _audioDeviceService.SetMasterVolume(volume);
                StatusText = "Volume set to " + volume;
            }
            catch (Exception ex)
            {
                StatusText = "Could not set volume: " + ex.Message;
            }
            finally
            {
                RaiseCommandStates();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            DateTimeText = DateTime.Now.ToString("dd/MM/yyyy");
            TimeText = DateTime.Now.ToString("HH:mm:ss");
        }

        private static int Clamp(int value)
        {
            if (value < 1)
                return 1;

            if (value > 100)
                return 100;

            return value;
        }

        private void RaiseCommandStates()
        {
            RaiseCommandState(_refreshCommand);
            RaiseCommandState(_setDefaultDeviceCommand);
            RaiseCommandState(_volumeUpCommand);
            RaiseCommandState(_volumeDownCommand);
        }

        private static void RaiseCommandState(ICommand command)
        {
            RelayCommand relayCommand = command as RelayCommand;

            if (relayCommand != null)
                relayCommand.RaiseCanExecuteChanged();
        }
    }
}