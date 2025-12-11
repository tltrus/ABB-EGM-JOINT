using System.Collections.ObjectModel;

namespace WpfApp
{
    public class RobotModel : Model
    {
        private ObservableCollection<RobotAxis> _axes;

        public RobotModel()
        {
            _axes = new ObservableCollection<RobotAxis>
            {
                new RobotAxis("Axis 1", -180, 180),
                new RobotAxis("Axis 2", -90, 90),
                new RobotAxis("Axis 3", -135, 135) { Angle = 0 }, // 0° считается нулевым (визуально будет -90°)
                new RobotAxis("Axis 4", -180, 180),
                new RobotAxis("Axis 5", -90, 90),
                new RobotAxis("Axis 6", -180, 180)
            };
        }

        public ObservableCollection<RobotAxis> Axes
        {
            get => _axes;
            set => SetProperty(ref _axes, value);
        }

        public void ResetAllAxes()
        {
            foreach (var axis in _axes)
            {
                axis.ResetToInitial();
            }
        }
    }
}

