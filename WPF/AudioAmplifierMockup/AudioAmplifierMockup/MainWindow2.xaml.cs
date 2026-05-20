using AudioAmplifierMockup.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace AudioAmplifierMockup
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private MainViewModel _viewModel;
        private bool _isDraggingVolumeKnob;

        public MainWindow2()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            _viewModel.CloseApp = () => {
                this.Close();
            };
            DataContext = _viewModel;
        }

        private void VolumeKnob_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingVolumeKnob = true;
            VolumeKnob.CaptureMouse();
            SetVolumeFromMousePosition(e);
        }

        private void VolumeKnob_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingVolumeKnob = false;
            VolumeKnob.ReleaseMouseCapture();
        }

        private void VolumeKnob_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDraggingVolumeKnob)
                return;

            SetVolumeFromMousePosition(e);
        }

        private void VolumeKnob_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_viewModel == null)
                return;

            _viewModel.Volume += e.Delta > 0 ? 1 : -1;
        }

        private void SetVolumeFromMousePosition(MouseEventArgs e)
        {
            if (_viewModel == null)
                return;

            Point position = e.GetPosition(VolumeKnob);

            double centerX = VolumeKnob.ActualWidth / 2.0;
            double centerY = VolumeKnob.ActualHeight / 2.0;

            double dx = position.X - centerX;
            double dy = centerY - position.Y;

            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;

            // Convert normal math angle to knob angle:
            // bottom-left = low, top = middle, bottom-right = high
            double knobAngle = 90.0 - angle;

            if (knobAngle > 180)
                knobAngle -= 360;

            if (knobAngle < -135)
                knobAngle = -135;

            if (knobAngle > 135)
                knobAngle = 135;

            int volume = (int)Math.Round(((knobAngle + 135.0) / 270.0 * 99.0) + 1.0);

            _viewModel.Volume = volume;
        }
    }
}
