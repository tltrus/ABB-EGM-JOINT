using System.Windows;
using System.Windows.Media.Media3D;
using WpfApp.ViewModels;
using WpfApp;
using System.Linq;
using System.Windows.Input;

namespace WpfApp
{
    public partial class RobotView : Window
    {
        private RobotViewModel? _viewModel;
        private Model3DGroup? _axis1Group;
        private Model3DGroup? _axis2Group;
        private Model3DGroup? _axis3Group;
        private Model3DGroup? _axis4Group;
        private Model3DGroup? _axis5Group;
        private Model3DGroup? _axis6Group;
        private bool _isLeftMouseDown = false;
        private bool _isRightMouseDown = false;
        private System.Windows.Point _lastMousePosition;

        public RobotView()
        {
            InitializeComponent();
            Loaded += RobotView_Loaded;
        }

        private void RobotView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as RobotViewModel;
            if (_viewModel != null)
            {
                FindModelGroups();
                SubscribeToAxisChanges();
                SubscribeToCameraChanges();
                UpdateCameraFromViewModel();
            }
        }

        private void SubscribeToCameraChanges()
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(RobotViewModel.CameraDistance) ||
                        e.PropertyName == nameof(RobotViewModel.CameraAngleX) ||
                        e.PropertyName == nameof(RobotViewModel.CameraAngleY))
                    {
                        UpdateCameraFromViewModel();
                    }
                };
            }
        }

        private void UpdateCameraFromViewModel()
        {
            if (_viewModel != null && Camera is PerspectiveCamera perspectiveCamera)
            {
                perspectiveCamera.Position = _viewModel.Camera.Position;
                perspectiveCamera.LookDirection = _viewModel.Camera.LookDirection;
                perspectiveCamera.UpDirection = new Vector3D(0, 0, 1);
            }
        }

        private void Viewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isLeftMouseDown = true;
            _lastMousePosition = e.GetPosition(Viewport3D);
            Viewport3D.CaptureMouse();
        }

        private void Viewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isRightMouseDown = true;
            _lastMousePosition = e.GetPosition(Viewport3D);
            Viewport3D.CaptureMouse();
        }

        private void Viewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isLeftMouseDown = false;
            if (!_isRightMouseDown)
            {
                Viewport3D.ReleaseMouseCapture();
            }
        }

        private void Viewport_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isRightMouseDown = false;
            if (!_isLeftMouseDown)
            {
                Viewport3D.ReleaseMouseCapture();
            }
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel == null)
                return;

            var currentPosition = e.GetPosition(Viewport3D);
            var deltaX = currentPosition.X - _lastMousePosition.X;
            var deltaY = currentPosition.Y - _lastMousePosition.Y;

            // Исправлено: добавлены правильные скобки
            if (_isRightMouseDown && (deltaX != 0 || deltaY != 0)) // Вращение камеры (правая кнопка)
            {
                _viewModel.RotateCamera(deltaX, deltaY);
            }
            else if (_isLeftMouseDown && (deltaX != 0 || deltaY != 0)) // Панорамирование камеры (левая кнопка)
            {
                // Панорамирование камеры
                _viewModel.PanCamera(deltaX, deltaY);
            }

            _lastMousePosition = currentPosition;
        }

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_viewModel != null)
            {
                // Приближение при прокрутке вверх (Delta > 0), отдаление при прокрутке вниз (Delta < 0)
                var zoomFactor = e.Delta > 0 ? 0.9 : 1.1;
                _viewModel.ZoomCamera(zoomFactor);
            }
        }

        private void FindModelGroups()
        {
            var robotModel = RobotModel.Content as Model3DGroup;
            if (robotModel != null && robotModel.Children.Count > 0)
            {
                // Axis1Group - первый дочерний элемент корневой группы
                _axis1Group = robotModel.Children[0] as Model3DGroup;

                if (_axis1Group != null)
                {
                    // Axis2Group - последний Model3DGroup в Axis1Group (после GeometryModel3D)
                    var axis1Children = _axis1Group.Children.OfType<Model3DGroup>().ToList();
                    _axis2Group = axis1Children.LastOrDefault();

                    if (_axis2Group != null)
                    {
                        var axis2Children = _axis2Group.Children.OfType<Model3DGroup>().ToList();
                        _axis3Group = axis2Children.LastOrDefault();

                        if (_axis3Group != null)
                        {
                            var axis3Children = _axis3Group.Children.OfType<Model3DGroup>().ToList();
                            _axis4Group = axis3Children.LastOrDefault();

                            if (_axis4Group != null)
                            {
                                var axis4Children = _axis4Group.Children.OfType<Model3DGroup>().ToList();
                                _axis5Group = axis4Children.LastOrDefault();

                                if (_axis5Group != null)
                                {
                                    var axis5Children = _axis5Group.Children.OfType<Model3DGroup>().ToList();
                                    _axis6Group = axis5Children.LastOrDefault();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SubscribeToAxisChanges()
        {
            if (_viewModel?.Robot?.Axes != null)
            {
                for (int i = 0; i < _viewModel.Robot.Axes.Count; i++)
                {
                    var axis = _viewModel.Robot.Axes[i];
                    var axisIndex = i; // Захватываем индекс в замыкании
                    axis.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "Angle")
                        {
                            UpdateTransform(axisIndex, axis.Angle);
                        }
                    };
                    // Инициализируем начальное положение
                    UpdateTransform(axisIndex, axis.Angle);
                }
            }
        }

        private void UpdateTransform(int axisIndex, double angle)
        {
            RotateTransform3D? transform = null;

            switch (axisIndex)
            {
                case 0: // Ось 1 - при увеличении угла поворот влево (положительное вращение вокруг Z)
                    if (_axis1Group != null)
                    {
                        transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle),
                            new Point3D(0, 0, 0));
                        _axis1Group.Transform = transform;
                    }
                    break;
                case 1: // Ось 2 - при увеличении угла наклон вперед (отрицательное вращение вокруг X)
                    if (_axis2Group != null)
                    {
                        transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), -angle),
                            new Point3D(0, 0, 0.2));
                        _axis2Group.Transform = transform;
                    }
                    break;
                case 2: // Ось 3 - при увеличении угла наклон вниз (отрицательное вращение вокруг X)
                    // Ось 3 всегда расположена под 90 градусов к оси 2 - это ее нулевое положение
                    if (_axis3Group != null)
                    {
                        var visualAngle = -angle - 90; // Отрицательное вращение с смещением на -90, чтобы 0° отображалось как -90° (90° к оси 2)
                        transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), visualAngle),
                            new Point3D(0, 0, 1.4));
                        _axis3Group.Transform = transform;
                    }
                    break;
                case 3: // Ось 4 - вращение вокруг Z (центр в 0,0,2.6 - соединение с запястьем)
                    if (_axis4Group != null)
                    {
                        transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle),
                            new Point3D(0, 0, 2.6));
                        _axis4Group.Transform = transform;
                    }
                    break;
                case 4: // Ось 5 - при увеличении угла подъем вверх (отрицательное вращение вокруг X)
                    if (_axis5Group != null)
                    {
                        transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(1, 0, 0), -angle),
                            new Point3D(0, 0, 3.0));
                        _axis5Group.Transform = transform;
                    }
                    break;
                case 5: // Ось 6 - вращение вокруг Z (центр в 0,0,3.3 - соединение с захватом)
                    if (_axis6Group != null)
                    {
                        transform = new RotateTransform3D(
                            new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle),
                            new Point3D(0, 0, 3.3));
                        _axis6Group.Transform = transform;
                    }
                    break;
            }
        }

        // УДАЛЕН метод Window_KeyDown - обработка клавиш A/S/D/W больше не нужна
    }
}