using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfLabelTicker
{
    public class ViewModel : INotifyPropertyChanged
    {

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
        #region Properties

        private ObservableCollection<Label> _labels;
        public ObservableCollection<Label> Labels
        {
            get => _labels;
            set => SetProperty(ref _labels, value);

        }

        #endregion

        #region Relay Commands
        #endregion

        #region Constructor
        public ViewModel()
        {
            Labels = new ObservableCollection<Label>();
            AddLabels();
        }

        private void AddLabels()
        {
            var lbl = new Label()
            {
                Content = "ONE",
                Foreground = new SolidColorBrush(Colors.Cyan),
                FontSize = 20,
                FlowDirection = System.Windows.FlowDirection.RightToLeft
            };
            Labels.Add(lbl);
             lbl = new Label()
            {
                Content = "TWO",
                Foreground = new SolidColorBrush(Colors.Cyan),
                FontSize = 20,
                FlowDirection = System.Windows.FlowDirection.RightToLeft
            };
            Labels.Add(lbl);

        }
        #endregion

        #region Functions
        #endregion

    }

    public class NegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return -doubleValue;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
