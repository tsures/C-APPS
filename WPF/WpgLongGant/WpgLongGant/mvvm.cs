using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace WpgLongGant
{
    public class mvvm :INotifyPropertyChanged
    {
        #region Variables
        #region Property Changed Section
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
        private ObservableCollection<MaintenanceEvent> _machine_events;
        public ObservableCollection<MaintenanceEvent> Machine_Events
        {
            get => _machine_events;
            set => SetProperty(ref _machine_events, value);
        }
        private ObservableCollection<WeekHeader> _ganttHeaders;
        public ObservableCollection<WeekHeader> GanettHeaders
        {
            get => _ganttHeaders;
            set => SetProperty(ref _ganttHeaders, value);

        }
        private ObservableCollection<string> _machineNames;
        public ObservableCollection<string> MachineNames
        {
            get => _machineNames;
            set => SetProperty(ref _machineNames, value);

        }
        private ObservableCollection<GridEvent> _ganttEvents;
        public ObservableCollection<GridEvent> GanttEvents
        {
            get => _ganttEvents;
            set => SetProperty(ref _ganttEvents, value);

        }
        private DateTime _startDateSchedule;

        public DateTime StartDateSchedule
        {
            get => _startDateSchedule;
            set => SetProperty(ref _startDateSchedule, value);
        }



        #endregion

        #region Constractor
        public mvvm()
        {
            StartDateSchedule = new DateTime(2024, 1, 1);
            populate_machines();
            GetEvents();
        }

        #endregion

        #region Functions   

        /// <summary>
        /// This function takes all the events and sort them as dataset for the gant view
        /// </summary>
        private void GetEvents()
        {
            GanttEvents = new ObservableCollection<GridEvent>();
            // Group events by machine name
            var groupedEvents = Machine_Events.GroupBy(e => e.MACHINE_NAME);
            // Iterate through each machine group
            foreach (var machineGroup in groupedEvents)
            {
                // Get the machine name
                var machineName = machineGroup.Key;
                // Initialize a new GridEvent for the current machine
                var newEvent = new GridEvent(machineName, -1, -1);
                // Iterate through each event for the current machine
                foreach (var maintenance in machineGroup)
                {
                    // Loop through the event weeks
                    for (int i = maintenance.START_WEEK_NUM - 1; i < maintenance.END_WEEK_NUM; i++)
                    {
                        newEvent.WeeksBusy[i] = 1;
                    }
                }
                // Add the event to the Gantt dataset
                if (newEvent != null)
                    GanttEvents.Add(newEvent);
            }
        }

        private void GetEvents2()
        {
            GanttEvents = new ObservableCollection<GridEvent>();

            // Group events by machine name
            var groupedEvents = Machine_Events.GroupBy(e => e.MACHINE_NAME);

            // Calculate the start date coefficient
            var startDateCoefficient = (int)Math.Ceiling(StartDateSchedule.DayOfYear / 7.0) - 1;

            // Iterate through each machine group
            foreach (var machineGroup in groupedEvents)
            {
                // Get the machine name
                var machineName = machineGroup.Key;

                // Initialize a new GridEvent for the current machine
                var newEvent = new GridEvent(machineName, -1, -1);

                // Iterate through each event for the current machine
                foreach (var maintenance in machineGroup)
                {
                    if (maintenance.START_WEEK_NUM < startDateCoefficient)
                    {
                        continue;
                    }

                    // Loop through the event weeks
                    for (int i = maintenance.START_WEEK_NUM - 1; i < maintenance.END_WEEK_NUM; i++)
                    {
                        int weekIndex = i - startDateCoefficient;
                        if (weekIndex >= 0 && weekIndex < newEvent.WeeksBusy.Count)
                        {
                            newEvent.WeeksBusy[weekIndex] = 1;
                        }
                    }
                }

                // Add the event to the Gantt dataset
                GanttEvents.Add(newEvent);
            }
        }


        private DateTime RandomDate(Random rand, int year)
        {
            int month = rand.Next(1, 13);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int day = rand.Next(1, daysInMonth + 1);
            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Populates the machine events data set with random data
        /// </summary>
        private void populate_machines()
        {
            Machine_Events = new ObservableCollection<MaintenanceEvent>();
            Random rand = new Random();

            for (int machineId = 1; machineId <= 5; machineId++)
            {
                string machineName = "Machine_" + machineId;

                for (int eventCount = 0; eventCount < 2; eventCount++)
                {
                    DateTime startDate = RandomDate(rand, StartDateSchedule.Year);
                    DateTime endDate = startDate.AddDays(rand.Next(1, 21)); // Maintenance can last between 1 and 14 days

                    MaintenanceEvent newEvent = new MaintenanceEvent
                    {
                        MACHINE_NAME = machineName,
                        MACHINE_ID = machineId,
                        START_DATE = startDate,
                        END_DATE = endDate,
                        INIT_DATE_COEFFICIENT = StartDateSchedule
                    };

                    Machine_Events.Add(newEvent);
                }
            }
        }

        public Brush GetColorByIndex(int index)
        {
            if (index < 0 || index >= Colors.Count)
            {
                return new SolidColorBrush(Colors[0]);
            }
            return new SolidColorBrush(Colors[index]);
        }

        private readonly List<Color> Colors = new List<Color>
        {
          Color.FromRgb(255, 0, 0),    // Red
          Color.FromRgb(0, 255, 0),    // Green
          Color.FromRgb(0, 0, 255),    // Blue
          Color.FromRgb(255, 255, 0),  // Yellow
          Color.FromRgb(255, 0, 255),  // Magenta
          Color.FromRgb(0, 255, 255),  // Cyan
          Color.FromRgb(128, 0, 0),    // Maroon
          Color.FromRgb(128, 128, 0),  // Olive
          Color.FromRgb(0, 128, 0),    // Dark Green
          Color.FromRgb(128, 0, 128),  // Purple
          Color.FromRgb(0, 128, 128),  // Teal
          Color.FromRgb(0, 0, 128),    // Navy
          Color.FromRgb(255, 165, 0),  // Orange
          Color.FromRgb(165, 42, 42),  // Brown
          Color.FromRgb(95, 158, 160), // Cadet Blue
          Color.FromRgb(127, 255, 0),  // Chartreuse
          Color.FromRgb(210, 105, 30), // Chocolate
          Color.FromRgb(255, 127, 80), // Coral
          Color.FromRgb(100, 149, 237),// Cornflower Blue
          Color.FromRgb(220, 20, 60),  // Crimson
          Color.FromRgb(0, 206, 209),  // Dark Turquoise
          Color.FromRgb(148, 0, 211),  // Dark Violet
          Color.FromRgb(255, 20, 147), // Deep Pink
          Color.FromRgb(0, 191, 255),  // Deep Sky Blue
          Color.FromRgb(105, 105, 105),// Dim Gray
          Color.FromRgb(30, 144, 255), // Dodger Blue
          Color.FromRgb(178, 34, 34),  // Firebrick
          Color.FromRgb(34, 139, 34),  // Forest Green
          Color.FromRgb(255, 0, 255),  // Fuchsia
          Color.FromRgb(255, 215, 0),  // Gold
          Color.FromRgb(218, 165, 32), // Goldenrod
          Color.FromRgb(173, 255, 47), // Green Yellow
          Color.FromRgb(255, 105, 180),// Hot Pink
          Color.FromRgb(205, 92, 92),  // Indian Red
          Color.FromRgb(75, 0, 130),   // Indigo
          Color.FromRgb(124, 252, 0),  // Lawn Green
          Color.FromRgb(240, 128, 128),// Light Coral
          Color.FromRgb(144, 238, 144),// Light Green
          Color.FromRgb(255, 182, 193),// Light Pink
          Color.FromRgb(255, 160, 122) // Light Salmon

        };
        public DataTemplate CreateHeaderTemplate(string weekNum, string weekDate)
        {
            string xaml = $@"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <StackPanel>
                    <TextBlock Text='{weekNum}' HorizontalAlignment='Center' FontSize='18' />
                    <TextBlock Text='{weekDate}' HorizontalAlignment='Center' FontSize='18' />
                </StackPanel>
            </DataTemplate>";

            return (DataTemplate)XamlReader.Parse(xaml);
        }
        #endregion
    }

    public class MaintenanceEvent
    {
        private string _machine_name;
        private int _machine_id;
        private DateTime _start_date;
        private DateTime _end_date;
        private DateTime _init_date_coefficient;

        public string MACHINE_NAME
        {
            get => _machine_name; set => _machine_name = value;
        }

        public int MACHINE_ID
        {
            get => _machine_id; set => _machine_id = value;
        }

        public DateTime START_DATE
        {
            get => _start_date; set => _start_date = value;
        }

        public DateTime END_DATE
        {
            get => _end_date; set => _end_date = value;
        }

        public DateTime INIT_DATE_COEFFICIENT
        {
            get => _init_date_coefficient;
            set => _init_date_coefficient = value;
        }

        public int INIT_DATE_COEFFICIENT_NUM => GetWeekNum(INIT_DATE_COEFFICIENT);

        private int GetWeekNum(DateTime input)
        {
            // Get the day of the year
            int dayOfYear = input.DayOfYear;
            // Calculate the week number
            int weekNumber = (int)Math.Ceiling(dayOfYear / 7.0);
            return weekNumber;
        }

        private int GetWeekNumRelative(DateTime input)
        {
            var ts = input - INIT_DATE_COEFFICIENT;
            return (int)Math.Ceiling(ts.TotalDays / 7.0);
        }


        public int START_WEEK_NUM => GetWeekNum(START_DATE);


        public int END_WEEK_NUM => GetWeekNum(END_DATE);

    }

    public class GridEvent
    {
        private string _machineName;
        private List<int> _weeksBusy;

        public GridEvent(string machineName, int startWeek, int endWeek)
        {
            MachineName = machineName;
            WeeksBusy = new List<int>();
            for (int i = 0; i < 54; i++)
            {
                if (i >= startWeek && i <= endWeek)
                    WeeksBusy.Add(1);
                else
                    WeeksBusy.Add(0);
            }
        }

        public string MachineName
        {
            get => _machineName; set => _machineName = value;
        }

        public List<int> WeeksBusy
        {
            get => _weeksBusy; set => _weeksBusy = value;
        }
    }

    public class WeekHeader
    {
        private string _weekTitle;
        private string _weekDates;

        public WeekHeader(string weekTitle, DateTime startWeek)
        {
            WeekTitle = weekTitle;
            WeekDates = $"{startWeek.ToString("dd/MM")}-{startWeek.AddDays(7).ToString("dd/MM")}";
        }

        public string WeekTitle
        {
            get => _weekTitle; set => _weekTitle = value;
        }

        public string WeekDates
        {
            get => _weekDates; set => _weekDates = value;
        }
    }



}
