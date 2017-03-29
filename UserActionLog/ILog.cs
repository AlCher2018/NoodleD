using System;

namespace UserActionLog
{
    public interface ILog
    {
        void LogAction(string frmName, string ctrlName, string eventName, string value);
        void LogAction(DateTime timeStamp, string frmName, string ctrlName, string eventName, string value);
        string GetLogFilePath { get; }
        string GetLogFileName();
        void Close();
    }
}
