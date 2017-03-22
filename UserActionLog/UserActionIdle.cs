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
                    _restartTimerInterval("set IdleSeconds");
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
                {
                    base.ReleaseEvents(_currentWin);
                    _timer.Enabled = false;     // stop timer
                }
                else
                {
                    base.AddHandler(_currentWin, "PreviewMouseMove");
                    base.AddHandler(_currentWin, "PreviewMouseDown");
                    base.AddHandler(_currentWin, "PreviewMouseWheel");
                    base.AddHandler(_currentWin, "TouchDown");
                    base.AddHandler(_currentWin, "TouchMove");
                    _restartTimerInterval("set CurrentWindow - " + _currentWin.GetType().Name + " (" + _currentWin.Name + ")");
                }
            } // set
        } // property

        private bool _pause;
        
        // ctor
        public UserActionIdle()
        {
            _timer = new Timer();
            _timer.AutoReset = false;
            _timer.Elapsed += _timer_Elapsed;
            base.ActionEventHandler += UserActionIdle_ActionEventHandler;
        }
        public UserActionIdle(int idleSeconds): this()
        {
            _idleSeconds = idleSeconds;
        }

        public void SetPause()
        {
            _pause = true;
        }

        // действия пользователя, которые рестартуют таймер
        private void UserActionIdle_ActionEventHandler(object sender, UserActionEventArgs e)
        {
            // в режиме паузы ожидаем только НАЖАТИЙ
            if ((_pause == false) || (_pause && e.EventName.EndsWith("Down")))
            {
                //Debug.Print(" *** ActionEventHandler, рестарт таймера по событию: " + e.EventName);
                if (_pause) _pause = false;  // сбросить паузу
                _restartTimerInterval("user action handler");
            }
        }

        // сработал таймер
        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Debug.Print(" *** сработал таймер, (" + _pause.ToString() + ")");

            if (_pause) return;
            if (IdleElapseEvent != null) IdleElapseEvent(e);
        }

        // restart timer
        private void _restartTimerInterval(string reason)
        {
            if (_currentWin != null)
            {
                //Debug.Print(" *** _restartTimerInterval, рестарт таймера по причине: " + reason);
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

            if (_currentWin != null)
            {
                base.ReleaseEvents(_currentWin);
            }
        }


    }  // class
}
