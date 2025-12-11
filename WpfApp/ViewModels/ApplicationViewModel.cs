using Microsoft.Win32;
using System.Globalization;
using System.Windows.Threading;
using WpfApp.Models;

namespace WpfApp.ViewModels
{
    public class ApplicationViewModel : ViewModel
    {
        private ModelMotion motion;
        private RobotController controller;

        public ModelMotion Motion
        {
            get => motion;
            set
            {
                motion = value;
                OnPropertyChanged();
            }
        }

        private JointLimits? _jointLimits; // null = лимиты не загружены

        #region Properties for button state management
        private bool _isConnected = false;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDisconnected));
                OnPropertyChanged(nameof(CanSendCommands));
            }
        }

        public bool IsDisconnected => !_isConnected;
        public bool CanSendCommands => _isConnected;
        #endregion

        #region Data binding properties
        public string ActTcpX => motion.ActTcpX;
        public string ActTcpY => motion.ActTcpY;
        public string ActTcpZ => motion.ActTcpZ;

        public string ActJoint1
        {
            get => motion.ActJoint1;
            private set
            {
                // Проверяем, изменилось ли значение перед обновлением
                if (motion.ActJoint1 != value)
                {
                    motion.ActJoint1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ActJoint2
        {
            get => motion.ActJoint2;
            private set
            {
                if (motion.ActJoint2 != value)
                {
                    motion.ActJoint2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ActJoint3
        {
            get => motion.ActJoint3;
            private set
            {
                if (motion.ActJoint3 != value)
                {
                    motion.ActJoint3 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ActJoint4
        {
            get => motion.ActJoint4;
            private set
            {
                if (motion.ActJoint4 != value)
                {
                    motion.ActJoint4 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ActJoint5
        {
            get => motion.ActJoint5;
            private set
            {
                if (motion.ActJoint5 != value)
                {
                    motion.ActJoint5 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ActJoint6
        {
            get => motion.ActJoint6;
            private set
            {
                if (motion.ActJoint6 != value)
                {
                    motion.ActJoint6 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SetJoint1
        {
            get => motion.SetJoint1;
            set
            {
                if (motion.SetJoint1 != value)
                {
                    motion.SetJoint1 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetJoint2
        {
            get => motion.SetJoint2;
            set
            {
                if (motion.SetJoint2 != value)
                {
                    motion.SetJoint2 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetJoint3
        {
            get => motion.SetJoint3;
            set
            {
                if (motion.SetJoint3 != value)
                {
                    motion.SetJoint3 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetJoint4
        {
            get => motion.SetJoint4;
            set
            {
                if (motion.SetJoint4 != value)
                {
                    motion.SetJoint4 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetJoint5
        {
            get => motion.SetJoint5;
            set
            {
                if (motion.SetJoint5 != value)
                {
                    motion.SetJoint5 = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SetJoint6
        {
            get => motion.SetJoint6;
            set
            {
                if (motion.SetJoint6 != value)
                {
                    motion.SetJoint6 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MinMaxJoint1 => motion.MinMaxJoint1;
        public string MinMaxJoint2 => motion.MinMaxJoint2;
        public string MinMaxJoint3 => motion.MinMaxJoint3;
        public string MinMaxJoint4 => motion.MinMaxJoint4;
        public string MinMaxJoint5 => motion.MinMaxJoint5;
        public string MinMaxJoint6 => motion.MinMaxJoint6;
        #endregion

        #region Commands with execution checks
        private RelayCommand connectCommand;
        public RelayCommand ConnectCommand
        {
            get
            {
                return connectCommand ?? (connectCommand = new RelayCommand(obj =>
                {
                    ConnectToRobot();
                },
                obj => IsDisconnected));
            }
        }

        private RelayCommand disconnectCommand;
        public RelayCommand DisconnectCommand
        {
            get
            {
                return disconnectCommand ?? (disconnectCommand = new RelayCommand(obj =>
                {
                    DisconnectFromRobot();
                },
                obj => IsConnected));
            }
        }

        private RelayCommand sendJointCommand;
        public RelayCommand SendJointCommand
        {
            get
            {
                return sendJointCommand ?? (sendJointCommand = new RelayCommand(obj =>
                {
                    SendJoints();
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand clearConsoleCommand;
        public RelayCommand ClearConsoleCommand
        {
            get
            {
                return clearConsoleCommand ?? (clearConsoleCommand = new RelayCommand(obj =>
                {
                    ClearConsole();
                },
                obj => true)); // Команда всегда доступна
            }
        }

        const double jointStep = 10.0;
        private RelayCommand sendJ1MinusCommand;
        public RelayCommand SendJ1MinusCommand
        {
            get
            {
                return sendJ1MinusCommand ?? (sendJ1MinusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(-jointStep, 0, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendJ1PlusCommand;
        public RelayCommand SendJ1PlusCommand
        {
            get
            {
                return sendJ1PlusCommand ?? (sendJ1PlusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(jointStep, 0, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand sendJ2MinusCommand;
        public RelayCommand SendJ2MinusCommand
        {
            get
            {
                return sendJ2MinusCommand ?? (sendJ2MinusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, -jointStep, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendJ2PlusCommand;
        public RelayCommand SendJ2PlusCommand
        {
            get
            {
                return sendJ2PlusCommand ?? (sendJ2PlusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, jointStep, 0, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand sendJ3MinusCommand;
        public RelayCommand SendJ3MinusCommand
        {
            get
            {
                return sendJ3MinusCommand ?? (sendJ3MinusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, -jointStep, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendJ3PlusCommand;
        public RelayCommand SendJ3PlusCommand
        {
            get
            {
                return sendJ3PlusCommand ?? (sendJ3PlusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, jointStep, 0, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand sendJ4MinusCommand;
        public RelayCommand SendJ4MinusCommand
        {
            get
            {
                return sendJ4MinusCommand ?? (sendJ4MinusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, 0, -jointStep, 0, 0);
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendJ4PlusCommand;
        public RelayCommand SendJ4PlusCommand
        {
            get
            {
                return sendJ4PlusCommand ?? (sendJ4PlusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, 0, jointStep, 0, 0);
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand sendJ5MinusCommand;
        public RelayCommand SendJ5MinusCommand
        {
            get
            {
                return sendJ5MinusCommand ?? (sendJ5MinusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, 0, 0, -jointStep, 0);
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendJ5PlusCommand;
        public RelayCommand SendJ5PlusCommand
        {
            get
            {
                return sendJ5PlusCommand ?? (sendJ5PlusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, 0, 0, jointStep, 0);
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand sendJ6MinusCommand;
        public RelayCommand SendJ6MinusCommand
        {
            get
            {
                return sendJ6MinusCommand ?? (sendJ6MinusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, 0, 0, 0, -jointStep);
                },
                obj => CanSendCommands));
            }
        }
        private RelayCommand sendJ6PlusCommand;
        public RelayCommand SendJ6PlusCommand
        {
            get
            {
                return sendJ6PlusCommand ?? (sendJ6PlusCommand = new RelayCommand(obj =>
                {
                    SendJointAdjusting(0, 0, 0, 0, 0, jointStep);
                },
                obj => CanSendCommands));
            }
        }

        private RelayCommand _loadMOCCommand;
        public RelayCommand LoadMOCCommand
        {
            get
            {
                return _loadMOCCommand ?? (_loadMOCCommand = new RelayCommand(obj =>
                {
                    LoadMocConfig();
                },
                obj => IsConnected));
            }
        }
        #endregion

        #region 3D Robot Simulator window
        private RelayCommand _showRobot3DCommand;
        public RelayCommand ShowRobot3DCommand
        {
            get
            {
                return _showRobot3DCommand ?? (_showRobot3DCommand = new RelayCommand(obj =>
                {
                    ShowRobot3DWindow();
                },
                obj => true));
            }
        }

        private RobotViewModel _robot3DViewModel;
        private RobotView _robot3DWindow;

        private void ShowRobot3DWindow()
        {
            if (_robot3DWindow == null || !_robot3DWindow.IsLoaded)
            {
                _robot3DViewModel = new RobotViewModel();

                // Sync initial joint angles
                if (double.TryParse(ActJoint1, out double j1) && _robot3DViewModel.Robot.Axes.Count > 0) 
                    _robot3DViewModel.Robot.Axes[0].Angle = j1;
                if (double.TryParse(ActJoint2, out double j2) && _robot3DViewModel.Robot.Axes.Count > 1) 
                    _robot3DViewModel.Robot.Axes[1].Angle = j2;
                if (double.TryParse(ActJoint3, out double j3) && _robot3DViewModel.Robot.Axes.Count > 2) 
                    _robot3DViewModel.Robot.Axes[2].Angle = j3;
                if (double.TryParse(ActJoint4, out double j4) && _robot3DViewModel.Robot.Axes.Count > 3) 
                    _robot3DViewModel.Robot.Axes[3].Angle = j4;
                if (double.TryParse(ActJoint5, out double j5) && _robot3DViewModel.Robot.Axes.Count > 4) 
                    _robot3DViewModel.Robot.Axes[4].Angle = j5;
                if (double.TryParse(ActJoint6, out double j6) && _robot3DViewModel.Robot.Axes.Count > 5) 
                    _robot3DViewModel.Robot.Axes[5].Angle = j6;

                _robot3DWindow = new RobotView();
                _robot3DWindow.DataContext = _robot3DViewModel;
                _robot3DWindow.Closed += (s, e) => _robot3DWindow = null;
                _robot3DWindow.Show();

                AddConsoleMessage("Robot 3D visualization window opened");
            }
            else
            {
                _robot3DWindow.Activate();
            }
        }
        #endregion

        private string _consoleOutput = string.Empty;
        public string ConsoleOutput
        {
            get => _consoleOutput;
            set
            {
                _consoleOutput = value;
                OnPropertyChanged();
            }
        }

        public ApplicationViewModel()
        {
            Motion = new ModelMotion();

            // Initialize display values
            motion.SetJoint1 = "10";
            motion.SetJoint2 = "0";
            motion.SetJoint3 = "0";
            motion.SetJoint4 = "0";
            motion.SetJoint5 = "30";
            motion.SetJoint6 = "0";

            InitializeController();

            // Initial state - disconnected
            IsConnected = false;
        }

        private void InitializeController()
        {
            // Create new controller instance
            controller = new RobotController("127.0.0.1", 6510, 6510);
            controller.PositionUpdated += OnPositionUpdated;
            controller.TcpPositionUpdated += OnTcpPositionUpdated;
            controller.StatusMessage += OnStatusMessage;
        }

        private void OnPositionUpdated(double[] position)
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                // Собираем все изменения в один вызов
                bool hasChanges = false;

                // Проверяем каждое значение на изменение
                var newActJoint1 = position[0].ToString("F2");
                if (motion.ActJoint1 != newActJoint1)
                {
                    motion.ActJoint1 = newActJoint1;
                    hasChanges = true;
                }
                var newActJoint2 = position[1].ToString("F2");
                if (motion.ActJoint2 != newActJoint2)
                {
                    motion.ActJoint2 = newActJoint2;
                    hasChanges = true;
                }
                var newActJoint3 = position[2].ToString("F2");
                if (motion.ActJoint3 != newActJoint3)
                {
                    motion.ActJoint3 = newActJoint3;
                    hasChanges = true;
                }
                var newActJoint4 = position[3].ToString("F2");
                if (motion.ActJoint4 != newActJoint4)
                {
                    motion.ActJoint4 = newActJoint4;
                    hasChanges = true;
                }
                var newActJoint5 = position[4].ToString("F2");
                if (motion.ActJoint5 != newActJoint5)
                {
                    motion.ActJoint5 = newActJoint5;
                    hasChanges = true;
                }
                var newActJoint6 = position[5].ToString("F2");
                if (motion.ActJoint6 != newActJoint6)
                {
                    motion.ActJoint6 = newActJoint6;
                    hasChanges = true;
                }

                // Если были изменения - уведомляем UI
                if (hasChanges)
                {
                    OnPropertyChanged(nameof(ActJoint1));
                    OnPropertyChanged(nameof(ActJoint2));
                    OnPropertyChanged(nameof(ActJoint3));
                    OnPropertyChanged(nameof(ActJoint4));
                    OnPropertyChanged(nameof(ActJoint5));
                    OnPropertyChanged(nameof(ActJoint6));
                }

                // Update 3D visualization if window is open
                if (_robot3DViewModel != null && _robot3DWindow != null && _robot3DWindow.IsLoaded)
                {
                    if (_robot3DViewModel.Robot.Axes.Count > 0) _robot3DViewModel.Robot.Axes[0].Angle = position[0];
                    if (_robot3DViewModel.Robot.Axes.Count > 1) _robot3DViewModel.Robot.Axes[1].Angle = position[1];
                    if (_robot3DViewModel.Robot.Axes.Count > 2) _robot3DViewModel.Robot.Axes[2].Angle = position[2];
                    if (_robot3DViewModel.Robot.Axes.Count > 3) _robot3DViewModel.Robot.Axes[3].Angle = position[3];
                    if (_robot3DViewModel.Robot.Axes.Count > 4) _robot3DViewModel.Robot.Axes[4].Angle = position[4];
                    if (_robot3DViewModel.Robot.Axes.Count > 5) _robot3DViewModel.Robot.Axes[5].Angle = position[5];
                }
            }, DispatcherPriority.Background);
        }
        private void OnTcpPositionUpdated(double[] tcpPosition)
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                motion.ActTcpX = tcpPosition[0].ToString("F2");
                motion.ActTcpY = tcpPosition[1].ToString("F2");
                motion.ActTcpZ = tcpPosition[2].ToString("F2");

                OnPropertyChanged(nameof(ActTcpX));
                OnPropertyChanged(nameof(ActTcpY));
                OnPropertyChanged(nameof(ActTcpZ));
            });
        }

        private void OnStatusMessage(string message) => AddConsoleMessage(message);

        public void AddConsoleMessage(string message)
        {
            ConsoleOutput += $"{DateTime.Now:HH:mm:ss} - {message}\n";
            OnPropertyChanged(nameof(ConsoleOutput));
        }
        private void ClearConsole()
        {
            ConsoleOutput = string.Empty;
            AddConsoleMessage("Console cleared");
        }

        private void ConnectToRobot()
        {
            try
            {
                // If controller was stopped, reinitialize it
                if (controller == null)
                {
                    InitializeController();
                }

                AddConsoleMessage("Load the MOC.cfg file to get the axis limits\n");

                controller.StartReading();
                IsConnected = true;
                AddConsoleMessage("Connected to robot - Reading data");
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Connection error: {ex.Message}");
                IsConnected = false;

                // Reset controller on error
                controller?.Stop();
                controller = null;
            }
        }
        private void DisconnectFromRobot()
        {
            try
            {
                if (controller != null)
                {
                    controller.Stop();
                    IsConnected = false;
                    AddConsoleMessage("Disconnected from robot");
                }
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Disconnection error: {ex.Message}");
                IsConnected = false;
                controller = null;
            }
        }

        private void SendJoints()
        {
            try
            {
                // Start control
                controller.StartControl();

                var culture = System.Globalization.CultureInfo.InvariantCulture;

                if (double.TryParse(SetJoint1, System.Globalization.NumberStyles.Any, culture, out double j1) &&
                    double.TryParse(SetJoint2, System.Globalization.NumberStyles.Any, culture, out double j2) &&
                    double.TryParse(SetJoint3, System.Globalization.NumberStyles.Any, culture, out double j3) &&
                    double.TryParse(SetJoint4, System.Globalization.NumberStyles.Any, culture, out double j4) &&
                    double.TryParse(SetJoint5, System.Globalization.NumberStyles.Any, culture, out double j5) &&
                    double.TryParse(SetJoint6, System.Globalization.NumberStyles.Any, culture, out double j6))
                {
                    double[] position = { j1, j2, j3, j4, j5, j6 };

                    if (!AreAllJointsWithinLimits(position))
                    {
                        AddConsoleMessage($"Movement is impossible. Approaching the limit");
                        return;
                    }

                    AddConsoleMessage($"Sending joint commands: {FormatPositions(position)}");

                    Task.Run(() =>
                    {
                        bool success = controller.MoveToPosition(position, 30000);
                        if (success)
                        {
                            AddConsoleMessage($"Successfully reached position: {FormatPositions(position)}");
                        }
                        else
                        {
                            AddConsoleMessage($"Failed to reach target position");
                        }
                    });
                }
                else
                {
                    AddConsoleMessage("Error: invalid data format for joints");
                }
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Error sending joint commands: {ex.Message}");
            }
        }

        private string FormatPositions(double[] positions)
        {
            string[] formatted = new string[positions.Length];
            for (int i = 0; i < positions.Length; i++)
            {
                formatted[i] = Math.Round(positions[i], 2).ToString("F2");
            }
            return $"[{string.Join(", ", formatted)}]";
        }

        private void SendJointAdjusting(double a1, double a2, double a3, double a4, double a5, double a6)
        {
            try
            {
                controller.StartControl();

                var culture = CultureInfo.CurrentCulture;

                // Текущие углы в градусах
                double j1 = double.Parse(motion.ActJoint1, culture);
                double j2 = double.Parse(motion.ActJoint2, culture);
                double j3 = double.Parse(motion.ActJoint3, culture);
                double j4 = double.Parse(motion.ActJoint4, culture);
                double j5 = double.Parse(motion.ActJoint5, culture);
                double j6 = double.Parse(motion.ActJoint6, culture);

                // Новые углы (в градусах)
                double[] newAnglesDeg = {
                    j1 + a1,
                    j2 + a2,
                    j3 + a3,
                    j4 + a4,
                    j5 + a5,
                    j6 + a6
                };

                if (!AreAllJointsWithinLimits(newAnglesDeg))
                {
                    AddConsoleMessage($"Movement is impossible. Approaching the limit");
                    return;
                }

                AddConsoleMessage($"Sending joint command: {FormatPositions(newAnglesDeg)}");

                Task.Run(() =>
                {
                    bool success = controller.MoveToPosition(newAnglesDeg, 5000);
                    if (success)
                    {
                        AddConsoleMessage($"Successfully reached position: {FormatPositions(newAnglesDeg)}");
                    }
                    else
                    {
                        AddConsoleMessage($"Failed to reach target position");
                    }
                });
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Error sending joint commands: {ex.Message}");
            }
        }
        private void LoadMocConfig()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Config files (*.cfg)|*.cfg|All files (*.*)|*.*";
                openFileDialog.Title = "Select MOC.cfg file";
                openFileDialog.DefaultExt = ".cfg";

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    AddConsoleMessage($"Loading MOC config from: {filePath}");

                    var parser = new MocConfigParser();
                    var limits = parser.ParseFile(filePath);
                    _jointLimits = limits; // сохраняем для последующих проверок

                    // Конвертируем радианы → градусы для отображения
                    var toDeg = new Func<double, double>(rad => rad * 180.0 / Math.PI);

                    // Обновляем значения в модели
                    motion.MinMaxJoint1 = $"{toDeg(limits.LowerBounds[0]):F2} / {toDeg(limits.UpperBounds[0]):F2}";
                    motion.MinMaxJoint2 = $"{toDeg(limits.LowerBounds[1]):F2} / {toDeg(limits.UpperBounds[1]):F2}";
                    motion.MinMaxJoint3 = $"{toDeg(limits.LowerBounds[2]):F2} / {toDeg(limits.UpperBounds[2]):F2}";
                    motion.MinMaxJoint4 = $"{toDeg(limits.LowerBounds[3]):F2} / {toDeg(limits.UpperBounds[3]):F2}";
                    motion.MinMaxJoint5 = $"{toDeg(limits.LowerBounds[4]):F2} / {toDeg(limits.UpperBounds[4]):F2}";
                    motion.MinMaxJoint6 = $"{toDeg(limits.LowerBounds[5]):F2} / {toDeg(limits.UpperBounds[5]):F2}";

                    // Уведомляем об изменении свойств
                    OnPropertyChanged(nameof(MinMaxJoint1));
                    OnPropertyChanged(nameof(MinMaxJoint2));
                    OnPropertyChanged(nameof(MinMaxJoint3));
                    OnPropertyChanged(nameof(MinMaxJoint4));
                    OnPropertyChanged(nameof(MinMaxJoint5));
                    OnPropertyChanged(nameof(MinMaxJoint6));

                    AddConsoleMessage($"Joint limits loaded successfully");
                    AddConsoleMessage($"Joint 1: {motion.MinMaxJoint1}°");
                    AddConsoleMessage($"Joint 2: {motion.MinMaxJoint2}°");
                    AddConsoleMessage($"Joint 3: {motion.MinMaxJoint3}°");
                    AddConsoleMessage($"Joint 4: {motion.MinMaxJoint4}°");
                    AddConsoleMessage($"Joint 5: {motion.MinMaxJoint5}°");
                    AddConsoleMessage($"Joint 6: {motion.MinMaxJoint6}°");
                }
            }
            catch (Exception ex)
            {
                AddConsoleMessage($"Error loading MOC config: {ex.Message}");
            }
        }

        private bool AreAllJointsWithinLimits(double[] jointAnglesDeg)
        {
            // Если лимиты не загружены — считаем, что все в пределах
            if (_jointLimits == null)
                return true;

            // Проверяем каждую ось
            for (int i = 0; i < 6; i++)
            {
                if (i >= jointAnglesDeg.Length) continue;

                double proposedAngleDeg = jointAnglesDeg[i];

                // jointIndex: 0 = J1, 1 = J2, ..., 5 = J6
                double lowerRad = _jointLimits.LowerBounds[i];
                double upperRad = _jointLimits.UpperBounds[i];

                // Конвертируем границы в градусы
                double lowerDeg = lowerRad * 180.0 / Math.PI;
                double upperDeg = upperRad * 180.0 / Math.PI;

                // Reduce limits to minimize risk of crush
                double safeLowerDeg = lowerDeg + 2;
                double safeUpperDeg = upperDeg - 2;

                // Проверяем, находится ли угол в пределах
                if (proposedAngleDeg < safeLowerDeg || proposedAngleDeg > safeUpperDeg)
                {
                    return false; // Ось выходит за пределы
                }
            }

            return true; // Все оси в пределах лимитов
        }

        // Метод для ограничения угла. НЕ ИСПОЛЬЗУЕТСЯ в программе
        private double ClampJointAngle(int jointIndex, double proposedAngleDeg)
        {
            // Если лимиты не загружены — разрешаем любое значение (как раньше)
            if (_jointLimits == null)
                return proposedAngleDeg;

            // jointIndex: 0 = J1, 1 = J2, ..., 5 = J6
            double lowerRad = _jointLimits.LowerBounds[jointIndex];
            double upperRad = _jointLimits.UpperBounds[jointIndex];

            // Конвертируем границы в градусы
            double lowerDeg = lowerRad * 180.0 / Math.PI;
            double upperDeg = upperRad * 180.0 / Math.PI;

            // Reduce limits to minimize risk of crush
            lowerDeg += 2;
            upperDeg -= 2;

            // Ограничиваем значение
            return Math.Max(lowerDeg, Math.Min(upperDeg, proposedAngleDeg));
        }
    }
}