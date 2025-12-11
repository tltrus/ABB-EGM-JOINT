namespace WpfApp
{
    public class ModelMotion : Model
    {
        // ----------------------------
        // Actual TCP axis position
        // ----------------------------

        private string _actTcpX = "0";
        private string _actTcpY = "0";
        private string _actTcpZ = "0";

        public string ActTcpX
        {
            get => _actTcpX;
            set
            {
                if (_actTcpX != value)
                {
                    _actTcpX = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActTcpY
        {
            get => _actTcpY;
            set
            {
                if (_actTcpY != value)
                {
                    _actTcpY = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActTcpZ
        {
            get => _actTcpZ;
            set
            {
                if (_actTcpZ != value)
                {
                    _actTcpZ = value;
                    OnPropertyChanged();
                }
            }
        }

        // ----------------------------
        // Actual Joint axis position
        // ----------------------------

        private string _actJoint1 = "0";
        private string _actJoint2 = "0";
        private string _actJoint3 = "0";
        private string _actJoint4 = "0";
        private string _actJoint5 = "0";
        private string _actJoint6 = "0";

        public string ActJoint1
        {
            get => _actJoint1;
            set
            {
                if (_actJoint1 != value)
                {
                    _actJoint1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActJoint2
        {
            get => _actJoint2;
            set
            {
                if (_actJoint2 != value)
                {
                    _actJoint2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActJoint3
        {
            get => _actJoint3;
            set
            {
                if (_actJoint3 != value)
                {
                    _actJoint3 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActJoint4
        {
            get => _actJoint4;
            set
            {
                if (_actJoint4 != value)
                {
                    _actJoint4 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActJoint5
        {
            get => _actJoint5;
            set
            {
                if (_actJoint5 != value)
                {
                    _actJoint5 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActJoint6
        {
            get => _actJoint6;
            set
            {
                if (_actJoint6 != value)
                {
                    _actJoint6 = value;
                    OnPropertyChanged();
                }
            }
        }

        // ----------------------------
        // Set Joint axis position
        // ----------------------------

        private string _setJoint1 = "0";
        private string _setJoint2 = "0";
        private string _setJoint3 = "0";
        private string _setJoint4 = "0";
        private string _setJoint5 = "0";
        private string _setJoint6 = "0";

        public string SetJoint1
        {
            get => _setJoint1;
            set
            {
                if (_setJoint1 != value)
                {
                    _setJoint1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SetJoint2
        {
            get => _setJoint2;
            set
            {
                if (_setJoint2 != value)
                {
                    _setJoint2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SetJoint3
        {
            get => _setJoint3;
            set
            {
                if (_setJoint3 != value)
                {
                    _setJoint3 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SetJoint4
        {
            get => _setJoint4;
            set
            {
                if (_setJoint4 != value)
                {
                    _setJoint4 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SetJoint5
        {
            get => _setJoint5;
            set
            {
                if (_setJoint5 != value)
                {
                    _setJoint5 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SetJoint6
        {
            get => _setJoint6;
            set
            {
                if (_setJoint6 != value)
                {
                    _setJoint6 = value;
                    OnPropertyChanged();
                }
            }
        }

        // ----------------------------
        // Joint boundaries (min/max)
        // ----------------------------

        private string _minMaxJoint1 = "0 / 0";
        private string _minMaxJoint2 = "0 / 0";
        private string _minMaxJoint3 = "0 / 0";
        private string _minMaxJoint4 = "0 / 0";
        private string _minMaxJoint5 = "0 / 0";
        private string _minMaxJoint6 = "0 / 0";

        public string MinMaxJoint1
        {
            get => _minMaxJoint1;
            set
            {
                if (_minMaxJoint1 != value)
                {
                    _minMaxJoint1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MinMaxJoint2
        {
            get => _minMaxJoint2;
            set
            {
                if (_minMaxJoint2 != value)
                {
                    _minMaxJoint2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MinMaxJoint3
        {
            get => _minMaxJoint3;
            set
            {
                if (_minMaxJoint3 != value)
                {
                    _minMaxJoint3 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MinMaxJoint4
        {
            get => _minMaxJoint4;
            set
            {
                if (_minMaxJoint4 != value)
                {
                    _minMaxJoint4 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MinMaxJoint5
        {
            get => _minMaxJoint5;
            set
            {
                if (_minMaxJoint5 != value)
                {
                    _minMaxJoint5 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MinMaxJoint6
        {
            get => _minMaxJoint6;
            set
            {
                if (_minMaxJoint6 != value)
                {
                    _minMaxJoint6 = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}