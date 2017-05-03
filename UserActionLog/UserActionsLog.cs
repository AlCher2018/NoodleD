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
    /// Класс, который будет логировать в файл большинство действий пользователя (InputDevice)
    /// </summary>
    public class UserActionsLog: UserAction
    {
        //Choose your preferred Logging , Log4Net, Elmah, etc TODO do this a Config switch
        private ILog _logger;

        #region Properties
        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set {
                _enabled = value;
            }
        }

        // имя родителя, которое будет передаваться в callback
        private string _parentName;
        public string ParentName
        {
            get { return _parentName; }
            set { _parentName = value; }
        }

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

        LogFilesPathLocationEnum _logFilePathLocation = LogFilesPathLocationEnum.App_Logs;
        public LogFilesPathLocationEnum LogFilesPathLocation
        {
            get { return _logFilePathLocation; }
        }

        #endregion

        // callback to the calling procedure
        public new event EventHandler<UserActionEventArgs> ActionEventHandler;

        #region Constructors

        // basic constructor
        private UserActionsLog(EventsMouseEnum mouseEvents, EventsKeyboardEnum keyboardEvents, EventsTouchEnum touchEvent,  LogFilesPathLocationEnum pathLocation, bool enabled, bool useWriteBuffer)
        {
            if (_mouseEvents != mouseEvents) _mouseEvents = mouseEvents;
            if (_keyboardEvents != keyboardEvents) _keyboardEvents = keyboardEvents;
            if (_touchEvents != touchEvent) _touchEvents = touchEvent;
            _enabled = enabled;

            _logger = new Logger(useWriteBuffer?512:0, pathLocation);
            base.ActionEventHandler += UserActionsLog_InnerHandler;

            //if (_enabled)
            //{
            //    _logger.LogAction("EventsLog class", "INSTANCE", "CREATED", "log files path: " + _logger.GetLogFilePath);
            //}

        }   // base constructor

        /// <summary>
        /// Ctor Optimal way of hooking up control events to listen for user actions. 
        /// </summary>
        public UserActionsLog(FrameworkElement[] ctrls, EventsMouseEnum mouseEvents = EventsMouseEnum.Bubble, EventsKeyboardEnum keyboardEvents = EventsKeyboardEnum.None, EventsTouchEnum touchEvent = EventsTouchEnum.None, LogFilesPathLocationEnum pathMode = LogFilesPathLocationEnum.App_Logs, bool enabled = true, bool useWriteBuffer = true) : this(mouseEvents, keyboardEvents, touchEvent, pathMode, enabled, useWriteBuffer)
        {
            if (ctrls == null) return;

            foreach (FrameworkElement ctrl in ctrls) hookUpEvents(ctrl);
        }

        public UserActionsLog(FrameworkElement ctrl, EventsMouseEnum mouseEvents = EventsMouseEnum.Bubble, EventsKeyboardEnum keyboardEvents = EventsKeyboardEnum.None, EventsTouchEnum touchEvent = EventsTouchEnum.None, LogFilesPathLocationEnum pathMode = LogFilesPathLocationEnum.App_Logs, bool enabled = true, bool useWriteBuffer = true) : this(mouseEvents, keyboardEvents, touchEvent, pathMode, enabled, useWriteBuffer)
        {
            if (ctrl == null) return;

            hookUpEvents(ctrl);
        }

        public override void Dispose()
        {
            base.Dispose();

            FinishLoggingUserActions();
        }

        #endregion

        #region "Methods"

        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Hooks up the event(s) to get the steps to reproduce problems.
        /// </summary>
        /// <param name="ctrl">The control whose events we're suspicious of causing problems.</param>
        private void hookUpEvents(FrameworkElement ctrl)
        {
            if (ctrl is Window)
            {
               _parentName = (ctrl as Window).Name;
                _logger.LogAction("** Create EventLogger for window " + _parentName, "<" + (ctrl as Window).GetType().Name + ">", null, null);
            }

            // уникальные события для некоторых элементов
            //    кнопка: Click
            if ((ctrl is ButtonBase) && (_mouseEvents != EventsMouseEnum.None))
            { //ButtonBase stands for Buttons, RepeatButtons, ToggleButtons, CheckBoxes and RadioButtons.
                base.AddHandler(ctrl, "Click");
                base.AddHandler(ctrl, "MouseDoubleClick");
            }

            //    окно: Loaded, Closing, Closed
            if (ctrl is Window)
            {
                base.AddHandler(ctrl, "Loaded");
                base.AddHandler(ctrl, "Closing");
                base.AddHandler(ctrl, "Closed");
                base.AddHandler(ctrl, "Activated");
                base.AddHandler(ctrl, "Deactivated");
                base.AddHandler(ctrl, "Drop");
                base.AddHandler(ctrl, "GotFocus");
                base.AddHandler(ctrl, "LostFocus");
                base.AddHandler(ctrl, "Unloaded");
                base.AddHandler(ctrl, "SourceUpdated");
                base.AddHandler(ctrl, "StateChanged");
            }

            //** СОБЫТИЯ ДЛЯ ЛЮБОГО FRAMEWORK-ЭЛЕМЕНТА

            base.AddHandler(ctrl, "SizeChanged");

            // ***************
            // mouse events
            //     tunnel events
            if (_mouseEvents.IsSetBit(EventsMouseEnum.Tunnel) == true)
            {
                base.AddHandler(ctrl, "PreviewMouseDown");
                base.AddHandler(ctrl, "PreviewMouseUp");
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Buttons) == true)
                {
                    base.AddHandler(ctrl, "PreviewMouseLeftButtonDown");
                    base.AddHandler(ctrl, "PreviewMouseRightButtonDown");
                    base.AddHandler(ctrl, "PreviewMouseLeftButtonUp");
                    base.AddHandler(ctrl, "PreviewMouseRightButtonUp");
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Move) == true)
                {
                    base.AddHandler(ctrl, "PreviewMouseMove");
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Wheel) == true)
                {
                    base.AddHandler(ctrl, "PreviewMouseWheel");
                }
            }
            //   bubble events
            if (_mouseEvents.IsSetBit(EventsMouseEnum.Bubble) == true)
            {
                base.AddHandler(ctrl, "MouseDown");
                base.AddHandler(ctrl, "MouseUp");
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Buttons) == true)
                {
                    base.AddHandler(ctrl, "MouseLeftButtonDown");
                    base.AddHandler(ctrl, "MouseRightButtonDown");
                    base.AddHandler(ctrl, "MouseLeftButtonUp");
                    base.AddHandler(ctrl, "MouseRightButtonUp");
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Move) == true)
                {
                    base.AddHandler(ctrl, "MouseMove");
                }
                if (_mouseEvents.IsSetBit(EventsMouseEnum.Wheel) == true)
                {
                    base.AddHandler(ctrl, "MouseWheel");
                }
            }

            // ********************
            //  keyboard events
            //    tunnel events
            if (_keyboardEvents.IsSetBit(EventsKeyboardEnum.Tunnel) == true)
            {
                base.AddHandler(ctrl, "PreviewKeyDown");
                base.AddHandler(ctrl, "PreviewKeyUp");
            }
            //   bubble events
            else if (_keyboardEvents.IsSetBit(EventsKeyboardEnum.Bubble) == true)
            {
                base.AddHandler(ctrl, "KeyDown");
                base.AddHandler(ctrl, "KeyUp");
            }

            // ********************
            //  touch events
            //    tunnel events
            if (_touchEvents.IsSetBit(EventsTouchEnum.Tunnel) == true)
            {
                base.AddHandler(ctrl, "PreviewTouchDown");
                base.AddHandler(ctrl, "PreviewTouchUp");
                if (_touchEvents.IsSetBit(EventsTouchEnum.Move) == true)
                {
                    base.AddHandler(ctrl, "PreviewTouchMove");
                }
            }
            //   bubble events
            else if (_touchEvents.IsSetBit(EventsTouchEnum.Bubble) == true)
            {
                base.AddHandler(ctrl, "TouchDown");
                base.AddHandler(ctrl, "TouchUp");
                if (_touchEvents.IsSetBit(EventsTouchEnum.Move) == true)
                {
                    base.AddHandler(ctrl, "TouchMove");
                }
            }

        } // method hookUpEvents

        public void FinishLoggingUserActions()
        {
            if (_enabled == true)
            {
                //_logger.LogAction("UserActionsLog class", "INSTANCE", "RELEASE", "log files path: " + _logger.GetLogFilePath);
                _logger.Close(); _logger = null;
            }
        }


        private void UserActionsLog_InnerHandler(object sender, UserActionEventArgs e)
        {
            // найти значение контрола
            FrameworkElement fe = (FrameworkElement)sender;
            if (e.ControlType == null) e.ControlType = fe.GetType();
            if (string.IsNullOrEmpty(e.ControlName)) e.ControlName = fe.GetType().Name;

            string valueLogString = GetSendingCtrlValue(fe, e.EventName, e);
            e.Value = valueLogString;

            if (this.ActionEventHandler != null)
            {
                ActionEventHandler(sender, e);
            }

            // write to log
            if (_enabled == true)
            {
                string sVal = null;
                if (e.EventName.Contains("Mouse") || (e.EventName.Contains("Touch")))
                {
                    if (string.IsNullOrEmpty(valueLogString)) sVal = "{" + e.Tag.ToString() + "}";
                    else sVal = valueLogString += ": {" + e.Tag.ToString() + "}";
                }
                _logger.LogAction(_parentName, e.ControlName, e.EventName, sVal);
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
                if (lb.SelectedItem != null)
                {
                    retVal = lb.SelectedItem.ToString();
                }

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
                    return (re.OriginalSource == null)?null:(re.OriginalSource as FrameworkElement).Name;
                }

                return ((Window)ctrl).Name;
            }
            else
            {
                return ctrl.ToString();
            }
        }

        #endregion


    }  // class

}