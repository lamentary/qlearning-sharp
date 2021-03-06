﻿namespace QLearningMaze.Ui.Wpf.ViewModels
{
    using Commands;

    using System;
    using System.Windows;

    public class ObservationSpaceViewModel : ViewModelBase
    {   
        private bool _isActive;
        private object _visibilityLock = new object();

        public event EventHandler<WallClickedEventArgs> WallClicked;

        public bool IsActive
        {
            get { return _isActive; }
            set 
            { 
                SetProperty(ref _isActive, value);
            }
        }

        private string _activeImageSource;

        public string ActiveImageSource
        {
            get { return _activeImageSource; }
            set { SetProperty(ref _activeImageSource, value); }
        }

        private Visibility _activeVisibility = Visibility.Hidden;

        public Visibility ActiveVisibility
        {
            get { return _activeVisibility; }
            set 
            { 
                SetProperty(ref _activeVisibility, value); 
            }
        }

        private RelayCommand<string> _wallMouseDownCommand;

        public RelayCommand<string> WallMouseDownCommand
        {
            get
            {
                if (_wallMouseDownCommand == null)
                {
                    _wallMouseDownCommand = new RelayCommand<string>((parms) => SetWallOpacity(parms));
                }

                return _wallMouseDownCommand;
            }
        }

        private bool _isStart;

        public bool IsStart
        {
            get { return _isStart; }
            set 
            {
                if (value != _isStart)
                {
                    SetProperty(ref _isStart, value);
                    OnPropertyChanged(nameof(ExtrasMessage));
                    OnPropertyChanged(nameof(ExtrasVisibility));
                }
            }
        }

        private bool _isGoal;

        public bool IsGoal
        {
            get { return _isGoal; }
            set 
            {
                if (value != _isGoal)
                {
                    SetProperty(ref _isGoal, value);
                    OnPropertyChanged(nameof(ExtrasMessage));
                    OnPropertyChanged(nameof(ExtrasVisibility));
                }
            }
        }

        public string ExtrasMessage
        {
            get
            {
                if (IsStart)
                {
                    return "Start";
                }

                if (IsGoal)
                {
                    return "Goal";
                }
                
                if (Reward > 0)
                {
                    return $"Reward: {Reward}";
                }

                if (Reward < 0)
                {
                    return $"Punish: {Reward}";
                }

                return null;
            }
        }

        public Visibility ExtrasVisibility
        {
            get
            {
                return string.IsNullOrEmpty(ExtrasMessage) ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private int _position;

        public int Position
        {
            get { return _position; }
            set { SetProperty(ref _position, value); }
        }

        private double _reward;

        public double Reward
        {
            get { return _reward; }
            set 
            { 
                SetProperty(ref _reward, value);
                OnPropertyChanged(nameof(ExtrasMessage));
                OnPropertyChanged(nameof(ExtrasVisibility));
            }
        }

        private int _leftWallVisibility = 0;

        public int LeftWallVisibility
        {
            get { return _leftWallVisibility; }
            set { SetProperty(ref _leftWallVisibility, value); }
        }

        private int _rightWallVisibility = 0;

        public int RightWallVisibility
        {
            get { return _rightWallVisibility; }
            set { SetProperty(ref _rightWallVisibility, value); }
        }

        private int _topWallVisibility = 0;

        public int TopWallVisibility
        {
            get { return _topWallVisibility; }
            set { SetProperty(ref _topWallVisibility, value); }
        }


        private int _bottomWallVisibility = 0;

        public int BottomWallVisibility
        {
            get { return _bottomWallVisibility; }
            set { SetProperty(ref _bottomWallVisibility, value); }
        }

        public void SetActive(bool isPrimaryAgent)
        {
            if (isPrimaryAgent)
            {
                ActiveImageSource = "/assets/ActiveDot.bmp";
            }
            else
            {
                ActiveImageSource = "/assets/ActiveDotSecondary.bmp";
            }

            lock (_visibilityLock)
            {
                ActiveVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(ActiveVisibility));
            }
        }

        public void SetInactive()
        {
            lock (_visibilityLock)
            {
                ActiveVisibility = Visibility.Collapsed;
                OnPropertyChanged(nameof(ActiveVisibility));
            }
        }

        public void SetWallOpacity(string wall)
        {
            switch (wall.ToLowerInvariant())
            {
                case "left":
                    LeftWallVisibility = 100 - LeftWallVisibility;
                    OnWallClicked(new WallClickedEventArgs(wall, Position, LeftWallVisibility == 100));
                    break;
                case "right":
                    RightWallVisibility = 100 - RightWallVisibility;
                    OnWallClicked(new WallClickedEventArgs(wall, Position, RightWallVisibility == 100));
                    break;
                case "top":
                    TopWallVisibility = 100 - TopWallVisibility;
                    OnWallClicked(new WallClickedEventArgs(wall, Position, TopWallVisibility == 100));
                    break;
                case "bottom":
                    BottomWallVisibility = 100 - BottomWallVisibility;
                    OnWallClicked(new WallClickedEventArgs(wall, Position, BottomWallVisibility == 100));
                    break;
                default:
                    break;
            }

        }

        protected virtual void OnWallClicked(WallClickedEventArgs e)
        {
            WallClicked?.Invoke(this, e);
        }
    }

    public class WallClickedEventArgs : EventArgs
    {
        public WallClickedEventArgs(string wallName, int position, bool isVisible)
        {
            WallName = wallName;
            Position = position;
            IsVisible = isVisible;
        }

        public string WallName { get; set; }
        public int Position { get; set; }
        public bool IsVisible { get; set; }
    }
}
