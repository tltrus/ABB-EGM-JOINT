namespace WpfApp
{
    public class RobotAxis : Model
    {
        private string _name;
        private double _angle;
        private double _minAngle;
        private double _maxAngle;

        public RobotAxis(string name, double minAngle = -180, double maxAngle = 180)
        {
            _name = name;
            _minAngle = minAngle;
            _maxAngle = maxAngle;
            _angle = 0;
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public double Angle
        {
            get => _angle;
            set
            {
                var clampedValue = System.Math.Max(_minAngle, System.Math.Min(_maxAngle, value));
                SetProperty(ref _angle, clampedValue);
            }
        }

        public double MinAngle
        {
            get => _minAngle;
            set => SetProperty(ref _minAngle, value);
        }

        public double MaxAngle
        {
            get => _maxAngle;
            set => SetProperty(ref _maxAngle, value);
        }

        public void Reset()
        {
            Angle = 0;
        }

        public void ResetToInitial()
        {
            // Для третьей оси нулевое значение 0 (визуально будет -90°), для остальных - 0
            if (Name.Contains("Ось 3"))
            {
                Angle = 0; // 0° - это нулевое положение для третьей оси
            }
            else
            {
                Angle = 0;
            }
        }
    }
}

