using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace MyTimer
{
    public partial class MainWindow : Window
    {
        TimerViewModel _timerViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _timerViewModel = new TimerViewModel();
            DataContext = _timerViewModel;
        }

        void _HandleKeyPress(object sender, KeyEventArgs e)
        {
            _timerViewModel.HandleKeyPress(e);
        }
    }

    public class TimerViewModel : INotifyPropertyChanged
    {
        TimerState _state;
        TaskbarManager _taskbarManager;
        DateTime _endTime;
        DateTime _pausedTime;
        TimeSpan _duration;
        Timer _timer;
        int _minutes = 40;
        bool _alerted;

        string _text = "My Timer";
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged("Text");
            }
        }

        int _percent;
        public int Percent
        {
            get { return _percent; }
            set
            {
                _percent = value;
                OnPropertyChanged("Percent");
            }
        }

        public string ToolTip
        {
            get { return "S:Start/Reset, P:Pause"; }
        }

        public void HandleKeyPress(KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.S)
            {
                if (DateTime.UtcNow.Subtract(_lastNumericInput).TotalSeconds < 5)
                    _minutes = int.Parse(_numericInput);
                _ResetTimer();
                _numericInput = "";
            }
            else if (keyEventArgs.Key == Key.P)
            {
                _PauseOrUnpause();
            }
            else if (keyEventArgs.IsNumber())
            {
                var charDigit = keyEventArgs.ToCharDigit();
                _UpdateNumericInput(charDigit);
            }
        }

        string _numericInput = "";
        DateTime _lastNumericInput = DateTime.MinValue;
        void _UpdateNumericInput(char charDigit)
        {
            if (DateTime.UtcNow.Subtract(_lastNumericInput).TotalSeconds > 5)
                _numericInput = "";
            _numericInput += charDigit;
            _lastNumericInput = DateTime.UtcNow;
        }

        void _PauseOrUnpause()
        {
            if (_state == TimerState.Running)
            {
                _pausedTime = DateTime.Now;
                _timer.Stop();
                _state = TimerState.Paused;
            }
            else if (_state == TimerState.Paused)
            {
                _endTime = DateTime.Now.Add(_endTime - _pausedTime);
                _timer.Start();
                _state = TimerState.Running;
            }
        }

        void _ResetTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
            }
            _duration = TimeSpan.FromMinutes(_minutes);
            _endTime = DateTime.Now.Add(_duration);
            _timer = new Timer(1000);
            _timer.Elapsed += (sender, args) => _UpdateText();
            _timer.Start();
            _alerted = false;
            _state = TimerState.Running;
            _UpdateText();
        }

        void _UpdateText()
        {
            var remainingTime = (_endTime - DateTime.Now);
            if (remainingTime.TotalSeconds <= 0)
            {
                Text = "00:00";
                if (!_alerted)
                {
                    Console.Beep();
                    Console.Beep();
                    Console.Beep();
                }
                _alerted = true;
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                TaskbarManager.Instance.SetProgressValue(100, 100);
            }
            else
            {
                if (remainingTime.Hours >= 1)
                {
                    Text = remainingTime.Hours + ":" + remainingTime.Minutes.ToString("D2") + ":" + remainingTime.Seconds.ToString("D2");
                }
                else
                {
                    Text = remainingTime.Minutes.ToString("D2") + ":" + remainingTime.Seconds.ToString("D2");
                }
                Percent = (int)(100*(_duration - remainingTime).TotalSeconds / _duration.TotalSeconds);

                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
                TaskbarManager.Instance.SetProgressValue(Percent, 100);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum TimerState
    {
        Running,
        Paused
    }
}
