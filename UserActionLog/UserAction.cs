using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

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
            var list = objectType.GetEvents();
            EventInfo objectEventInfo = list.FirstOrDefault(ei => ei.Name == eventName);
            if (objectEventInfo != null)
            {
                Type objectEventHandlerType = objectEventInfo.EventHandlerType;
                Delegate del = Delegate.CreateDelegate(objectEventHandlerType, this, _handlerMethod);

                ControlEvent eHandler = new ControlEvent(control, objectEventInfo, del);
                eHandler.AddHandler();
                _listEvents.Add(eHandler);

            }  // if

        }  // method

        public void ReleaseEvents()
        {
            foreach (ControlEvent elEvent in _listEvents)
            {
                elEvent.RemoveHandler();
            }
        }

        public void ReleaseEvents(FrameworkElement element)
        {
            foreach (ControlEvent cEvent in _listEvents.Where(ce => ce.Control.Equals(element)))
            {
                cEvent.RemoveHandler();
            }
        }

        private void _eventHandler(object sender, RoutedEventArgs e)
        {
            if (ActionEventHandler != null)
            {
                ControlEvent ce = _listEvents.Find(c => c.Control.Equals(sender) && (c.EventInfo.Name == e.RoutedEvent.Name));

                string eName = "";
                eName = e.RoutedEvent.Name;

                ActionEventHandler(sender, 
                    new UserActionEventArgs()
                    {
                        ControlName = ce.Control.Name,
                        ControlType = ce.Control.GetType(),
                        EventName = ce.EventInfo.Name
                    });
            }
        }

        public virtual void Dispose()
        {
            ReleaseEvents();
            _listEvents.Clear(); _listEvents = null;
            GC.Collect();
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
    }

}
