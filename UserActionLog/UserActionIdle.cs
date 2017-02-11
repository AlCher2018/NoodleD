using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Windows;

namespace UserActionLog
{
    // класс, ожидающий любого действия пользователя
    public class UserActionIdle: UserAction
    {
        private TimeSpan _currentTime;
        private Timer _timer;

        public event Action<ElapsedEventArgs> IdleElapseEvent;

        private int _idleSeconds;   // in sec
        public int IdleSeconds
        {
            get { return _idleSeconds; }
            set
            {
                _idleSeconds = value;
                if (_idleSeconds > 0)
                {
                    _timer.Interval = 1000 * _idleSeconds;     // convert sec to msec
                    _restartTimerInterval();
                }
                else
                    _timer.Enabled = false;     // stop timer
            }
        }

        private FrameworkElement _anyActionWin;
        public FrameworkElement AnyActionWindow
        {
            set
            {
                if (_anyActionWin != null)
                {
                    base.ReleaseEvents(_anyActionWin);
                }

                _anyActionWin = value;

                if (_anyActionWin != null)
                {
                    base.AddHandler(_anyActionWin, "PreviewMouseMove");
                    base.AddHandler(_anyActionWin, "PreviewMouseDown");
                    base.AddHandler(_anyActionWin, "PreviewMouseUp");
                    base.AddHandler(_anyActionWin, "PreviewMouseWheel");
                    base.AddHandler(_anyActionWin, "PreviewKeyDown");
                    base.AddHandler(_anyActionWin, "PreviewTouchDown");
                    base.AddHandler(_anyActionWin, "PreviewTouchUp");
                    base.AddHandler(_anyActionWin, "PreviewTouchUp");
                }
            } // set
        } // property

        public UserActionIdle()
        {
            _timer = new Timer();
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            base.ActionEventHandler += UserActionIdle_ActionEventHandler;
        }
        public UserActionIdle(int idleSeconds): this()
        {
            _idleSeconds = idleSeconds;
            if (_idleSeconds > 0)
            {
                _timer.Interval = 1000 * _idleSeconds;     // convert sec to msec
                _restartTimerInterval();
            }
        }

        private void UserActionIdle_ActionEventHandler(object sender, UserActionEventArgs e)
        {
            _restartTimerInterval();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IdleElapseEvent != null) IdleElapseEvent(e);
        }

        // restart timer
        private void _restartTimerInterval()
        {
            _timer.Stop();
            _timer.Start();
        }

        public override void Dispose()
        {
            if (_timer != null)
            {
                _timer.Stop(); _timer.Close();
            }
        }


    }  // class
}
