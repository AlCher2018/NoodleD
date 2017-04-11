using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserActionLog
{
    public class Logger : ILog, IDisposable
    {
        #region "Private Member Variables"
        
        private const string EVENTLOGFILEIDENTIFIER = "EventLog_";
        private static int _maxNumerOfLogsInMemory;
        private static List<string> _eventsList = new List<string>();
        private static string _loggerDirectory = string.Empty;

        public string GetLogFilePath
        {
            get { return _loggerDirectory; }
        }

        private bool _useWriteBuffer;
        private string _loggerFile;
        #endregion


        public Logger(int maxNumerOfLogsInMemory = 512, LogFilesPathLocationEnum logPathEnum = LogFilesPathLocationEnum.App_Logs)
        {
            _loggerDirectory = LibFuncs.getLogFilesPath(logPathEnum);

            _maxNumerOfLogsInMemory = maxNumerOfLogsInMemory;
            _useWriteBuffer = (maxNumerOfLogsInMemory > 0);
            if (_useWriteBuffer == false) _loggerFile = GetLogFileName();
        }

        #region "Methods"

        public void LogAction(string frmName, string ctrlName, string eventName, string value)
        {
            LogAction(DateTime.Now, frmName,ctrlName, eventName, value);
        }

        public void LogAction(DateTime timeStamp, string frmName, string ctrlName, string eventName, string value)
        {
            lock (this)
            {
                string msg = string.Format("{0}: {1} {2}", timeStamp.ToString("yyyy/MM/dd H:mm:ss.fff"), string.IsNullOrEmpty(frmName)? ctrlName: frmName + "." + ctrlName, eventName);
                if (string.IsNullOrEmpty(value) == false) msg += ";" + value;

                if (_useWriteBuffer)
                {
                    _eventsList.Add(msg);
                    if (_eventsList.Count > _maxNumerOfLogsInMemory) WriteLogActionsToFile();
                }
                else
                    WriteLogActionToFile(msg);
            }
        }


        public string GetLogFileName()
        {
            //Check if the current file is > 1 MB and create another
             string[] existingFileList = System.IO.Directory.GetFiles(_loggerDirectory, EVENTLOGFILEIDENTIFIER +  DateTime.Now.ToString("yyyyMMdd") + "*.log");

            string filePath = _loggerDirectory + EVENTLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-0.log";
            if (existingFileList.Count() > 0)
            {
                filePath = _loggerDirectory + EVENTLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-" + (existingFileList.Count() - 1).ToString() + ".log";
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length / 1024 > 1000) //Over a MB (ie > 1000 KBs)
                {
                    filePath = _loggerDirectory + EVENTLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-" + existingFileList.Count().ToString() + ".log";
                }
            }
            
            return filePath;
        }


        private void WriteLogActionsToFile()
        {
            if ((_eventsList == null) || (_eventsList.Count == 0)) return;

            string logFilePath = GetLogFileName();
            if (File.Exists(logFilePath))
            {
                //If this fails its because as a Admin you need to run the app as Admin - wierd problem with GPO's
                try
                {
                    File.AppendAllLines(logFilePath, _eventsList);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("If you see this message its because you're an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    File.WriteAllLines(logFilePath, _eventsList);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("If you see this message its because you're not an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
            _eventsList.Clear();
        }

        private void WriteLogActionToFile(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            if (File.Exists(_loggerFile))
            {
                //If this fails its because as a Admin you need to run the app as Admin - wierd problem with GPO's
                try
                {
                    File.AppendAllText(_loggerFile, "\r\n" + msg);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("If you see this message its because you're an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(_loggerFile, "\r\n" + msg);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("If you see this message its because you're not an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            if (_useWriteBuffer)
            {
                WriteLogActionsToFile();
            }
        }

        #endregion
    }
}
