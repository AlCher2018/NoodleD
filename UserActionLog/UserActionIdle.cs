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
    // использует внутренний таймер, который начинает работать когда устанавливается свойство CurrentWindow - окно, которое будет перехватывать некоторые действия пользователя
    // устанавливать это свойство надо в событии Activated и очищать (= null) в событии DeActivated
    public class UserActionIdle: UserAction
    {
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

        private Window _currentWin;
        public Window CurrentWindow
        {
            set
            {
                if (_currentWin != null) base.ReleaseEvents(_currentWin);

                _currentWin = value;

                if (_currentWin == null)
                    base.ReleaseEvents(_currentWin);
                else
                {
                    base.AddHandler(_currentWin, "PreviewMouseMove");
                    base.AddHandler(_currentWin, "PreviewMouseDown");
                    base.AddHandler(_currentWin, "PreviewMouseWheel");
                    base.AddHandler(_currentWin, "PreviewKeyDown");
                    base.AddHandler(_currentWin, "TouchDown");
                    base.AddHandler(_currentWin, "TouchMove");
                    // и запустить таймер
                    _restartTimerInterval();
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
        }

        private void UserActionIdle_ActionEventHandler(object sender, UserActionEventArgs e)
        {
            Debug.Print("UserActionIdle_ActionEventHandler");
            _restartTimerInterval();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Debug.Print("_timer_Elapsed");
            if (IdleElapseEvent != null) IdleElapseEvent(e);
        }

        // restart timer
        private void _restartTimerInterval()
        {
            if (_currentWin != null)
            {
                _timer.Stop();
                _timer.Start();
            }
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
