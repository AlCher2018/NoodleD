using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace UserActionLog
{
    // базовый класс, в котором сохраняются контролы с событиями,
    // и обработчик событий вызывает callback-метод
    public class UserAction: IDisposable
    {
        private List<ControlEvent> _listEvents;
        private MethodInfo _handlerMethod;
        public event EventHandler<UserActionEventArgs> ActionEventHandler;

        public UserAction()
        {
            _listEvents = new List<ControlEvent>();
            _handlerMethod = typeof(UserAction).GetMethod("_eventHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public void AddHandler(FrameworkElement control, string eventName)
        {
            Type objectType = control.GetType();

            // get EventInfo to wanted event of the objectType
            EventInfo[] list = objectType.GetEvents();
            EventInfo eventInfo = list.FirstOrDefault(ei => ei.Name == eventName);
            if (eventInfo == null) return;

            Type eventHandlerType = eventInfo.EventHandlerType;
            Type argsType = eventHandlerType.ReflectedType;
            Action<object, EventArgs> action = (o, args) => _eventHandler(eventInfo.Name, o, args);
            Delegate del=null;

            //var ttt = Activator.CreateInstance(eventHandlerType, action);
            // MouseButtonEventHandler
            if (eventHandlerType.Name == "RoutedEventHandler") del = new RoutedEventHandler(action);
            else if (eventHandlerType.Name == "EventHandler")  del = new EventHandler(action);
            else if (eventHandlerType.Name == "MouseButtonEventHandler")  del = new System.Windows.Input.MouseButtonEventHandler(action);
            else if (eventHandlerType.Name == "MouseEventHandler") del = new System.Windows.Input.MouseEventHandler(action);
            else if (eventHandlerType.Name == "MouseWheelEventHandler") del = new System.Windows.Input.MouseWheelEventHandler(action);
            else if (eventHandlerType.Name == "KeyEventHandler")  del = new System.Windows.Input.KeyEventHandler(action);
            else if (eventHandlerType.Name == "InputEventHandler")  del = new System.Windows.Input.InputEventHandler(action);
            else if (eventHandlerType.Name == "KeyboardEventHandler")  del = new System.Windows.Input.KeyboardEventHandler(action);
            else if (eventHandlerType.Name == "KeyEventHandler")  del = new System.Windows.Input.KeyEventHandler(action);
            else if (eventHandlerType.Name == "TouchFrameEventHandler")  del = new System.Windows.Input.TouchFrameEventHandler(action);
            else if (eventHandlerType.Name == "SizeChangedEventHandler")  del = new SizeChangedEventHandler(action);
            else if (eventHandlerType.Name == "DragEventHandler")  del = new DragEventHandler(action);
            else if (eventHandlerType.Name == "CancelEventHandler") del = new System.ComponentModel.CancelEventHandler(action);

            if (del != null)
            {
                eventInfo.AddEventHandler(control, del);
                ControlEvent eHandler = new ControlEvent(control, eventInfo, del);
                //eHandler.AddHandler();
                _listEvents.Add(eHandler);
            }
            else
            {
               // Debug.Print("-- handlerType-- " + eventHandlerType.Name);
            }

        }  // method


        private void _eventHandler(string eventName, object sender, EventArgs e)
        {
            //Debug.Print(string.Format("*** sender-{0}, eventName-{1}", sender, eventName));
            if (ActionEventHandler != null)
            {
                ControlEvent ce = _listEvents.Find(c => c.Control.Equals(sender) && (c.EventInfo.Name == eventName));
                if (ce != null)
                {
                    UserActionEventArgs args = new UserActionEventArgs()
                    {
                        ControlName = ce.Control.Name,
                        ControlType = ce.Control.GetType(),
                        EventName = ce.EventInfo.Name
                    };
                    if ((eventName.Contains("Mouse")) && (sender is IInputElement))
                        args.Tag = (e as MouseEventArgs).GetPosition((IInputElement)sender);
                    if ((eventName.Contains("Touch")) && (sender is IInputElement))
                        args.Tag = (e as TouchEventArgs).GetTouchPoint((IInputElement)sender).Position;

                    ActionEventHandler(sender, args);
                }
            }
        }  // method

        public virtual void Dispose()
        {
            ReleaseEvents();
            _listEvents.Clear(); _listEvents = null;
            GC.Collect();
        }
        public void ReleaseEvents()
        {
            foreach (ControlEvent elEvent in _listEvents)
            {
                elEvent.RemoveHandler();
            }
        }

        protected void ReleaseEvents(FrameworkElement element)
        {
            foreach (ControlEvent cEvent in _listEvents.Where(ce => ce.Control.Equals(element)))
            {
                cEvent.RemoveHandler();
            }
        }

    }  // class

    public class ControlEvent
    {
        public FrameworkElement Control { get; set; }
        public EventInfo EventInfo { get; set; }
        public Delegate MethoDelegate { get; set; }

        public ControlEvent(FrameworkElement control, EventInfo eInfo, Delegate del)
        {
            this.Control = control;
            this.EventInfo = eInfo;
            this.MethoDelegate = del;
        }

        public void AddHandler()
        {
            this.EventInfo.AddEventHandler(this.Control, this.MethoDelegate);
        }

        public void RemoveHandler()
        {
            this.EventInfo.RemoveEventHandler(this.Control, this.MethoDelegate);
        }

    }

    public class UserActionEventArgs: RoutedEventArgs
    {
        public string ControlName { get; set; }
        public Type ControlType { get; set; }
        public string EventName { get; set; }
        public object Value { get; set; }
        public object Tag { get; set; }
    }

}
