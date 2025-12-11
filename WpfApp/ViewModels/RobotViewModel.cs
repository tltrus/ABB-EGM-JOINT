using WpfApp;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace WpfApp.ViewModels
{
    public class RobotViewModel : ViewModel
    {
        private RobotModel _robot;
        private PerspectiveCamera _camera;
        private double _cameraDistance = 5.0;
        private double _cameraAngleX = 45;
        private double _cameraAngleY = 45;
        private Point3D _cameraTarget = new Point3D(0, 0, -1.8); // Точка фокуса на центре основания оси 1
        private readonly Point3D _rotationCenter = new Point3D(0, 0, -1.8); // Центр вращения (центр основания оси 1)

        public RobotViewModel()
        {
            _robot = new RobotModel();

            // Инициализация камеры с видом сверху-сбоку для лучшего обзора робота
            _cameraAngleX = 45;
            _cameraAngleY = 30;

            // Сначала создаем камеру с минимальными настройками
            _camera = new PerspectiveCamera
            {
                UpDirection = new Vector3D(0, 0, 1),
                FieldOfView = 60
            };

            // Затем рассчитываем и устанавливаем позицию и направление
            UpdateCameraFromAngles();

            ResetAllAxesCommand = new RelayCommand(_ => ResetAllAxes());
        }

        public RobotModel Robot
        {
            get => _robot;
            set => SetProperty(ref _robot, value);
        }

        public PerspectiveCamera Camera
        {
            get => _camera;
            set => SetProperty(ref _camera, value);
        }

        public double CameraDistance
        {
            get => _cameraDistance;
            set
            {
                if (SetProperty(ref _cameraDistance, value))
                {
                    UpdateCameraFromAngles();
                }
            }
        }

        public double CameraAngleX
        {
            get => _cameraAngleX;
            set
            {
                if (SetProperty(ref _cameraAngleX, value))
                {
                    UpdateCameraFromAngles();
                }
            }
        }

        public double CameraAngleY
        {
            get => _cameraAngleY;
            set
            {
                if (SetProperty(ref _cameraAngleY, value))
                {
                    UpdateCameraFromAngles();
                }
            }
        }

        public Point3D CameraTarget
        {
            get => _cameraTarget;
            set => SetProperty(ref _cameraTarget, value);
        }

        public ICommand ResetAllAxesCommand { get; }

        private void ResetAllAxes()
        {
            _robot.ResetAllAxes();
        }

        private Point3D CalculateCameraPosition()
        {
            var angleXRad = _cameraAngleX * System.Math.PI / 180.0;
            var angleYRad = _cameraAngleY * System.Math.PI / 180.0;

            var x = _cameraDistance * System.Math.Cos(angleXRad) * System.Math.Cos(angleYRad);
            var y = _cameraDistance * System.Math.Sin(angleXRad) * System.Math.Cos(angleYRad);
            var z = _cameraDistance * System.Math.Sin(angleYRad);

            // Возвращаем позицию камеры относительно центра вращения
            return new Point3D(x, y, z) + new Vector3D(_rotationCenter.X, _rotationCenter.Y, _rotationCenter.Z);
        }

        private void UpdateCameraFromAngles()
        {
            // Рассчитываем новую позицию камеры
            var newPosition = CalculateCameraPosition();

            // Устанавливаем позицию камеры и направление взгляда на центр вращения
            if (_camera != null)
            {
                _camera.Position = newPosition;
                _camera.LookDirection = _rotationCenter - newPosition;
            }

            // Обновляем точку фокуса (теперь она всегда совпадает с центром вращения)
            _cameraTarget = _rotationCenter;

            // Уведомляем об изменении
            OnPropertyChanged(nameof(Camera));
        }

        public void UpdateCameraFromPositionAndTarget(Point3D newPosition, Point3D newTarget)
        {
            if (_camera == null) return;

            // Обновляем позицию камеры и точку фокуса
            _camera.Position = newPosition;
            _cameraTarget = newTarget;
            _camera.LookDirection = _cameraTarget - newPosition;

            // Вычисляем новые углы и расстояние на основе позиции камеры относительно центра вращения
            var delta = newPosition - _rotationCenter;
            _cameraDistance = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z);

            if (_cameraDistance > 0)
            {
                // Вычисляем угол Y (вертикальный)
                _cameraAngleY = Math.Asin(delta.Z / _cameraDistance) * 180.0 / Math.PI;

                // Вычисляем угол X (горизонтальный)
                var horizontalDistance = Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

                if (horizontalDistance > 0.001)
                {
                    var angleXRad = Math.Atan2(delta.Y, delta.X);
                    _cameraAngleX = angleXRad * 180.0 / Math.PI;
                }

                // Ограничиваем угол Y для предотвращения переворота камеры
                _cameraAngleY = Math.Max(-89, Math.Min(89, _cameraAngleY));

                OnPropertyChanged(nameof(CameraDistance));
                OnPropertyChanged(nameof(CameraAngleX));
                OnPropertyChanged(nameof(CameraAngleY));
                OnPropertyChanged(nameof(CameraTarget));
            }
        }

        public void PanCamera(double deltaX, double deltaY)
        {
            if (_camera == null) return;

            // Панорамирование камеры - двигаем и камеру, и точку фокуса
            // deltaX: положительное - движение вправо (камера смещается влево)
            // deltaY: положительное - движение вверх (камера поднимается вверх)

            // Получаем текущие направления
            var lookDirection = _camera.LookDirection;
            lookDirection.Normalize();

            var upDirection = new Vector3D(0, 0, 1); // Вертикальная ось (вверх)
            var rightDirection = Vector3D.CrossProduct(lookDirection, upDirection);
            rightDirection.Normalize();

            // Вычисляем вектор перемещения
            // deltaX: движение мыши вправо -> камера смещается влево (противоположное направление)
            // deltaY: движение мыши вверх -> камера поднимается вверх
            var moveVector = -rightDirection * deltaX + upDirection * deltaY;

            // Масштабируем скорость перемещения
            var moveSpeed = 0.002 * _cameraDistance;
            moveVector *= moveSpeed;

            // Двигаем камеру и точку фокуса
            var newPosition = _camera.Position + moveVector;
            var newTarget = _cameraTarget + moveVector;

            UpdateCameraFromPositionAndTarget(newPosition, newTarget);
        }

        public void RotateCamera(double deltaX, double deltaY)
        {
            // Вращение камеры вокруг центра основания оси 1
            // Инвертированы оба направления вращения
            _cameraAngleX -= deltaX * 0.5; // Было +deltaX, стало -deltaX
            _cameraAngleY = Math.Max(-89, Math.Min(89, _cameraAngleY + deltaY * 0.5)); // Изменено: -deltaY на +deltaY
            UpdateCameraFromAngles();

            OnPropertyChanged(nameof(CameraAngleX));
            OnPropertyChanged(nameof(CameraAngleY));
        }

        public void ZoomCamera(double zoomFactor)
        {
            _cameraDistance = Math.Max(2.0, Math.Min(15.0, _cameraDistance * zoomFactor));
            UpdateCameraFromAngles();

            OnPropertyChanged(nameof(CameraDistance));
        }
    }
}