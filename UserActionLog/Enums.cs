using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UserActionLog
{
    // location saving file
    public enum LogFilesPathLocationEnum
    {
        App, App_Logs, UserTemp, UserTemp_Logs
    }

    public enum EventsMouseEnum
    {
        None    = 0x0,
        Bubble  = 0x1,
        Tunnel  = 0x10,
        Move    = 0x100,
        Wheel   = 0x1000,
        Buttons = 0x10000,
        All     = 0x11111 
    }

    public enum EventsKeyboardEnum
    {
        None    = 0x0,
        Bubble  = 0x1,
        Tunnel  = 0x10, 
        All     = 0x11
    }

    public enum EventsTouchEnum
    {
        None    = 0x0,
        Bubble  = 0x1,
        Tunnel  = 0x10,
        Move    = 0x100,
        All     = 0x111
    }

    public static class EventEnumExtension
    {
        public static bool IsSetBit(this EventsMouseEnum eValue, EventsMouseEnum eCheckingBit)
        {
            return _isBit((int)eValue, (int)eCheckingBit);
        }
        public static bool IsSetBit(this EventsKeyboardEnum eValue, EventsKeyboardEnum eCheckingBit)
        {
            return _isBit((int)eValue, (int)eCheckingBit);
        }
        public static bool IsSetBit(this EventsTouchEnum eValue, EventsTouchEnum eCheckingBit)
        {
            return _isBit((int)eValue, (int)eCheckingBit);
        }

        private static bool _isBit(int whereCheck, int whatCheck)
        {
            return ((whereCheck & whatCheck) == whatCheck);
        }
    }


}
