using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpgLongGant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow :Window
    {
        private mvvm DC;
        public MainWindow()
        {
            InitializeComponent();
            DC = new mvvm();
            this.DataContext = DC;
            GenerateWeekStyles();
            AddColumnsToDataGrid();

        }

        private void AddColumnsToDataGrid()
        {
            var start = DC.StartDateSchedule;
            var startDateCoefficient = (int)Math.Ceiling(DC.StartDateSchedule.DayOfYear / 7.0);

            for (int i = 0; i < 53; i++)
            {
                var weekNum = i + startDateCoefficient;
                if (weekNum > 53)
                    weekNum = weekNum - 53;
                var startweek = start.AddDays(i * 7);
                var end = start.AddDays(((i + 1) * 7) - 1);
                var headerDate = $"{startweek.ToString("dd/MM")}-{end.ToString("dd/MM")}";
                //var headerTemplate = DC.CreateHeaderTemplate($"שבוע {i + 1}", headerDate);
                var headerTemplate = DC.CreateHeaderTemplate($"שבוע {weekNum}", headerDate);

                var column = new DataGridTextColumn
                {
                    Binding = new Binding($"WeeksBusy[{i}]"),
                    CellStyle = (Style)FindResource($"WEEK{i}"),
                    HeaderTemplate = headerTemplate
                };
                dgvEvents.Columns.Add(column);
            }
        }



        private void GenerateWeekStyles()
        {

            for (int i = 0; i <= 53; i++)
            {
                Style style = new Style(typeof(DataGridCell));
                DataTrigger busyTrigger = new DataTrigger
                {
                    Binding = new Binding($"WeeksBusy[{i}]"),
                    Value = "1"
                };
                busyTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, DC.GetColorByIndex(i)));
                busyTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, DC.GetColorByIndex(i)));
                busyTrigger.Setters.Add(new Setter(DataGridCell.ToolTipProperty, $"שבוע {i + 1}"));
                style.Triggers.Add(busyTrigger);
                DataTrigger freeTrigger = new DataTrigger
                {
                    Binding = new Binding($"WeeksBusy[{i}]"),
                    Value = "0"
                };
                freeTrigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.White));
                freeTrigger.Setters.Add(new Setter(DataGridCell.ForegroundProperty, Brushes.White));
                style.Triggers.Add(freeTrigger);
                Resources.Add($"WEEK{i}", style);
            }
        }


    }
}
