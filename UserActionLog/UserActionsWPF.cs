using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Timers;

namespace UserActionLog
{
    /// <summary>
    /// UserActions class for WPF
    /// </summary>
    public class UserActionsWPF: IDisposable
    {
        //Choose your preferred Logging , Log4Net, Elmah, etc TODO do this a Config switch
        private ILog _logger;
        private List<FrameworkElement> _controlsList;
        private Window _win;
        private TimeSpan _currentTime;
        private Timer _timer;

        #region Properties
        private EventsMouseEnum _mouseEvents = EventsMouseEnum.Bubble;
        public EventsMouseEnum MouseEvents
        {
            get { return _mouseEvents; }
        }

        private EventsKeyboardEnum _keyboardEvents = EventsKeyboardEnum.None;
        public EventsKeyboardEnum KeyboardEvents
        {
            get { return _keyboardEvents; }
        }

        private EventsTouchEnum _touchEvents = EventsTouchEnum.None;
        public EventsTouchEnum TouchdEvents
        {
            get { return _touchEvents; }
        }

        private ClassActionEnum _classAction = ClassActionEnum.WriteToLog;
        public ClassActionEnum ClassAction
        {
            get { return _classAction; }
        }

        private int _idleSeconds;   // in sec
        public int IdleSeconds
        {
            get { return _idleSeconds; }
            set {
                _idleSeconds = value;
                if (_classAction.IsSetBit(ClassActionEnum.IdleEvent) == true)
                {
                    if (_idleSeconds > 0)
                    {
                        _setTimerInterval();
                        _timer.Enabled = true;     // start timer
                    }
                    else
                        _timer.Enabled = false;     // stop timer
                }
            }
        }

        LogFilesPathLocationEnum _logFilePathLocation = LogFilesPathLocationEnum.App_Logs;
        public LogFilesPathLocationEnum LogFilesPathLocation
        {
            get { return _logFilePathLocation; }
        }

        private Window _anyActionWin;
        public Window AnyActionWindow
        {
            set {
                if (_anyActionWin != null)
                {
                    _anyActionWin.PreviewMouseMove -= _anyActionWin_Event;
                    _anyActionWin.PreviewMouseUp -= _anyActionWin_Event;
                    _anyActionWin.PreviewMouseWheel -= _anyActionWin_Event;
                    _anyActionWin.PreviewKeyDown -= _anyActionWin_Event;
                    _anyActionWin.PreviewTouchDown -= _anyActionWin_Event;
                }
                _anyActionWin = value;
                _anyActionWin.PreviewMouseMove += _anyActionWin_Event;
                _anyActionWin.PreviewMouseUp += _anyActionWin_Event;
                _anyActionWin.PreviewMouseWheel += _anyActionWin_Event;
                _anyActionWin.PreviewKeyDown += _anyActionWin_Event;
                _anyActionWin.PreviewTouchDown += _anyActionWin_Event;
            }
        }

        private void _anyActionWin_Event(object sender, System.Windows.Input.InputEventArgs e)
        {
            if (_classAction.IsSetBit(ClassActionEnum.IdleEvent) == true)
            {
                _setTimerInterval();
            }
        }

        public event Action<string> UserActionEvent;
        public event Action<ElapsedEventArgs> IdleElapseEvent;

        #endregion

        #region Constructors
        // basic constructor
        private UserActionsWPF(EventsMouseEnum mouseEvents, EventsKeyboardEnum keyboardEvents, EventsTouchEnum touchEvent, ClassActionEnum classAction, LogFilesPathLocationEnum pathLocation)
        {
            if (_mouseEvents != mouseEvents) _mouseEvents = mouseEvents;
            if (_keyboardEvents != keyboardEvents) _keyboardEvents = keyboardEvents;
            if (_touchEvents != touchEvent) _touchEvents = touchEvent;
            if (_classAction != classAction) _classAction = classAction;

            _controlsList = new List<FrameworkElement>();

            if (classAction.IsSetBit(ClassActionEnum.WriteToLog) == true)
            {
                _logger = new Logger(logPathEnum: pathLocation);
                _logger.LogAction("userActionClass", "INSTANCE", "CREATED", "log files path: " + _logger.GetLogFilePath);
            }

            if (classAction.IsSetBit(ClassActionEnum.IdleEvent) == true)
            {
                _timer = new Timer();
                _timer.Elapsed += _timer_Elapsed;
            }
        }   // base constructor

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (IdleElapseEvent != null) IdleElapseEvent(e);
        }

        /// <summary>
        /// Ctor Optimal way of hooking up control events to listen for user actions. 
        /// </summary>
        public UserActionsWPF(FrameworkElement[] ctrls, EventsMouseEnum mouseEvents = EventsMouseEnum.Bubble, EventsKeyboardEnum keyboardEvents = EventsKeyboardEnum.None, EventsTouchEnum touchEvent = EventsTouchEnum.None, ClassActionEnum classAction = ClassActionEnum.WriteToLog, LogFilesPathLocationEnum pathMode = LogFilesPathLocationEnum.App_Logs) : this(mouseEvents, keyboardEvents, touchEvent, classAction, pathMode)
        {
            if (ctrls == null) return;

            foreach (var ctrl in ctrls) hookUpEvents(ctrl);
        }

        public UserActionsWPF(ContentControl contentCtrl, EventsMouseEnum mouseEvents = EventsMouseEnum.Bubble, EventsKeyboardEnum keyboardEvents = EventsKeyboardEnum.None, EventsTouchEnum touchEvent = EventsTouchEnum.None, ClassActionEnum classAction = ClassActionEnum.WriteToLog, LogFilesPathLocationEnum pathMode = LogFilesPathLocationEnum.App_Logs) : this(mouseEvents, keyboardEvents, touchEvent, classAction, pathMode)
        {
            if (contentCtrl == null) return;

            recurseHookUp(contentCtrl);
        }

        //Recursively hook up control events
        private void recurseHookUp(FrameworkElement element)
        {
            hookUpEvents(element);
            foreach (FrameworkElement ctrl in LogicalTreeHelper.GetChildren(element))
            {
                if (ctrl is ContentControl) recurseHookUp(ctrl); 
            }

        }

        #endregion

        #region "Methods"

        public void FinishLoggingUserActions()
        {
            if (_classAction.IsSetBit(ClassActionEnum.WriteToLog) == true)
            {
                _logger.LogAction("userActionsClass", "INSTANCE", "DESTROY", "log files path: " + _logger.GetLogFilePath);
                _logger.WriteLogActionsToFile();
            }

            this.releaseEvents();
            this._controlsList.RemoveAll(i => true);
            this._controlsList = null;
        }

        /// <summary>
        /// Hooks up the event(s) to get the steps to reproduce problems.
        /// </summary>
        /// <param name="ctrl">The control whose events we're suspicious of causing problems.</param>
        private void hookUpEvents(FrameworkElement ctrl)
        {
            eventsMode(ctrl, true);

            if (_win == null) _win = getWindow(ctrl);
            _controlsList.Add(ctrl);
        }

        private Window getWindow(FrameworkElement ctrl)
        {
            if (ctrl is Window) return (Window)ctrl;

            FrameworkElement curEl = (FrameworkElement)ctrl.Parent;

            while ((curEl != null) && (curEl is Window) == false) curEl = (FrameworkElement)curEl.Parent;

            return (Window)curEl;
        }

        // Add or Remove events from control
        private void eventsMode(FrameworkElement ctrl, bool isAddEvent)
        {
            if ((ctrl is ButtonBase) && (_mouseEvents != EventsMouseEnum.None))
            { //ButtonBase stands for Buttons, RepeatButtons, ToggleButtons, CheckBoxes and RadioButtons.
                ButtonBase btn = ((ButtonBase)ctrl);
                if (isAddEvent == true) btn.Click += LogAction; else btn.Click -= LogAction;
            }

            if (ctrl is Window)
            {
                Window wnd = ((Window)ctrl);
                if (isAddEvent == true) wnd.Initialized += LogAction; else wnd.Initialized -= LogAction;
                if (isAddEvent == true) wnd.Loaded += LogAction; else wnd.Loaded -= LogAction;
                if (isAddEvent == true) wnd.Closing += LogAction; else wnd.Closing -= LogAction;
                if (isAddEvent == true) wnd.Closed += LogAction; else wnd.Closed -= LogAction;
            }

            FrameworkElement el = ((FrameworkElement)ctrl);

            // ***************
            // mouse events
            //     tunnel events
            if (_mouseEvents.IsSetBit(EventsMouseEnum.Tunnel) == true)
            {
                if (isAddEvent == true) el.PreviewMouseDown += LogAction; else el.PreviewMouseDown -= LogAction;
                if (isAddEvent == true) el.PreviewMouseUp += LogAction; else el.PreviewMouseUp -= LogAction;
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Buttons) == true)
                {
                    if (isAddEvent == true) el.PreviewMouseLeftButtonDown += LogAction; else el.PreviewMouseLeftButtonDown -= LogAction;
                    if (isAddEvent == true) el.PreviewMouseRightButtonDown += LogAction; else el.PreviewMouseRightButtonDown -= LogAction;
                    if (isAddEvent == true) el.PreviewMouseLeftButtonUp += LogAction; else el.PreviewMouseLeftButtonUp -= LogAction;
                    if (isAddEvent == true) el.PreviewMouseRightButtonUp += LogAction; else el.PreviewMouseRightButtonUp -= LogAction;
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Move) == true)
                {
                    if (isAddEvent == true) el.PreviewMouseMove += LogAction; else el.PreviewMouseMove -= LogAction;
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Wheel) == true)
                {
                    if (isAddEvent == true) el.PreviewMouseWheel += LogAction; else el.PreviewMouseWheel -= LogAction;
                }
            }
            //   bubble events
            if (_mouseEvents.IsSetBit(EventsMouseEnum.Bubble) == true)
            {
                if (isAddEvent == true) el.MouseDown += LogAction; else el.MouseDown -= LogAction;
                if (isAddEvent == true) el.MouseUp += LogAction; else el.MouseUp -= LogAction;
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Buttons) == true)
                {
                    if (isAddEvent == true) el.MouseLeftButtonDown += LogAction; else el.MouseLeftButtonDown -= LogAction;
                    if (isAddEvent == true) el.MouseRightButtonDown += LogAction; else el.MouseRightButtonDown -= LogAction;
                    if (isAddEvent == true) el.MouseLeftButtonUp += LogAction; else el.MouseLeftButtonUp -= LogAction;
                    if (isAddEvent == true) el.MouseRightButtonUp += LogAction; else el.MouseRightButtonUp -= LogAction;
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Move) == true)
                {
                    if (isAddEvent == true) el.MouseMove += LogAction; else el.MouseMove -= LogAction;
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Wheel) == true)
                {
                    if (isAddEvent == true) el.MouseWheel += LogAction; else el.MouseWheel -= LogAction;
                }
            }


            // ********************
            //  keyboard events
            //    tunnel events
            if (_keyboardEvents.IsSetBit(EventsKeyboardEnum.Tunnel) == true)
            {
                if (isAddEvent == true) el.PreviewKeyDown += LogAction; else el.PreviewKeyDown -= LogAction;
                if (isAddEvent == true) el.PreviewKeyUp += LogAction; else el.PreviewKeyUp -= LogAction;
            }
            //   bubble events
            else if (_keyboardEvents.IsSetBit(EventsKeyboardEnum.Bubble) == true)
            {
                if (isAddEvent == true) el.KeyDown += LogAction; else el.KeyDown -= LogAction;
                if (isAddEvent == true) el.KeyUp += LogAction; else el.KeyUp -= LogAction;
            }

            // ********************
            //  touch events
            //    tunnel events
            if (_touchEvents.IsSetBit(EventsTouchEnum.Tunnel) == true)
            {
                if (isAddEvent == true) el.PreviewTouchDown += LogAction; else el.PreviewTouchDown -= LogAction;
                if (isAddEvent == true) el.PreviewTouchUp += LogAction; else el.PreviewTouchUp -= LogAction;
                if (_touchEvents.IsSetBit(EventsTouchEnum.Move) == true)
                {
                    if (isAddEvent == true) el.PreviewTouchMove += LogAction; else el.PreviewTouchMove -= LogAction;
                }
            }
            //   bubble events
            else if (_touchEvents.IsSetBit(EventsTouchEnum.Bubble) == true)
            {
                if (isAddEvent == true) el.TouchDown += LogAction; else el.TouchDown -= LogAction;
                if (isAddEvent == true) el.TouchUp += LogAction; else el.TouchUp -= LogAction;
                if (_touchEvents.IsSetBit(EventsTouchEnum.Move) == true)
                {
                    if (isAddEvent == true) el.TouchMove += LogAction; else el.TouchMove -= LogAction;
                }
            }

        }  // eventsMode method

        /// <summary>
        /// Releases the hooked up events (avoiding memory leaks).
        /// </summary>
        private void releaseEvents()
        {
            foreach (FrameworkElement item in _controlsList)
            {
                eventsMode(item, false);
            }
        }

        /// <summary>
        /// Log the Control that made the call and its value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LogAction(object sender, EventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            string ctrlLogString = fe.Name + ": " + fe.GetType().Name;
            string eventName = string.Empty;
            if (e is RoutedEventArgs)
                eventName = (e as RoutedEventArgs).RoutedEvent.Name;
            else
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame[] stackFrames = stackTrace.GetFrames();
                eventName = stackFrames[1].GetMethod().Name;
            }
            string valueLogString = GetSendingCtrlValue(fe, eventName, e);


            // raise callback event
            if (_classAction.IsSetBit(ClassActionEnum.RaiseEvent) && (UserActionEvent != null))
            {
                UserActionEvent(ctrlLogString + " --> " + eventName + " --> " + valueLogString);
            }
            // write to log
            if (_classAction.IsSetBit(ClassActionEnum.WriteToLog))
            {
                string winLogString = _win.Name + ": " + _win.GetType().Name;
                _logger.LogAction(winLogString, ctrlLogString, eventName, valueLogString);
            }

            if (_classAction.IsSetBit(ClassActionEnum.IdleEvent) == true)
            {
                _currentTime = new TimeSpan(DateTime.Now.Millisecond);
                _setTimerInterval();
            }
        }

        private string GetSendingCtrlValue(FrameworkElement ctrl, string eventName, EventArgs e)
        {
            string retVal = null;

            if (ctrl is TextBox)
            {
                return ((TextBox)ctrl).Text;
            }
            if (ctrl is TextBlock)
            {
                return ((TextBlock)ctrl).Text;
            }
            else if (ctrl is ButtonBase)
            {
                ButtonBase bb = (ButtonBase)ctrl;
                return (bb.Content == null)?"":bb.Content.ToString();
            }
            else if (ctrl is Border)
            {
                Border bb = (Border)ctrl;

                return (bb.Child == null) ? "" : bb.Child.ToString();
            }
            else if (ctrl is ListBox)
            {
                ListBox lb = (ListBox)ctrl;
                retVal = (lb.SelectedValue == null)?"":(lb.SelectedValue as ListBoxItem).Content.ToString();

                return retVal;
            }
            else if (ctrl is TreeView)
            {
                TreeView tv = (TreeView)ctrl;
                return (tv.SelectedValue == null)?"":tv.SelectedValue.ToString();
            }

            else if (ctrl is Window)
            {
                // get clicked control
                if ((eventName.Contains("Mouse") == true) && (eventName.EndsWith("Down") || (eventName.EndsWith("Up"))))
                {
                    RoutedEventArgs re = (e as RoutedEventArgs);
                    return (re.OriginalSource as FrameworkElement).Name;
                }

                return ((Window)ctrl).Name;
            }
            else
            {
                return ctrl.ToString();
            }
        }

        private void _setTimerInterval()
        {
            _timer.Interval = 1000 * _idleSeconds;     // convert sec to msec
        }

        public string[] GetTodaysLogFileNames()
        {
            return _logger.GetTodaysLogFileNames();
        }

        public void Dispose()
        {
            this.releaseEvents();
            this._controlsList.RemoveAll(i => true);
            this._controlsList = null;

            if (_timer != null)
            {
                _timer.Stop(); _timer.Close();
            }
        }

        #endregion


    }  // class

}