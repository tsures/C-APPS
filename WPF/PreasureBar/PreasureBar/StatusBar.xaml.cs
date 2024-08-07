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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PreasureBar
{
    /// <summary>
    /// Interaction logic for StatusBar.xaml
    /// </summary>
    public partial class StatusBar :UserControl
    {
        public StatusBar()
        {
            InitializeComponent();
            Loaded += GradientBarControl_Loaded;
        }

       

        private void GradientBarControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFill();
        }

        public double FillPercentage
        {
            get
            {
                return (double)GetValue(FillPercentageProperty);
            }
            set
            {
                SetValue(FillPercentageProperty, value);
            }
        }

        public static readonly DependencyProperty FillPercentageProperty =
            DependencyProperty.Register("FillPercentage", typeof(double), typeof(StatusBar),
                new PropertyMetadata(0.0, OnFillPercentageChanged));

        private static void OnFillPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatusBar control)
            {
                control.UpdateFill();
            }
        }

        private void UpdateFill()
        {
            double percentage = FillPercentage / 100.0;
            GradientRectangle.Width = ActualWidth * percentage;
        }


    }
}
