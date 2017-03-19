﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserActionLog
{
    public class Logger : ILog
    {
        #region "Private Member Variables"
        
        const string ACTIONLOGFILEIDENTIFIER = "ActionLog_";
        private static int _maxNumerOfLogsInMemory = 512;
        private static List<string> _theUserActions = new List<string>();
        private static string _actionLoggerDirectory = string.Empty;
        public string GetLogFilePath
        {
            get { return _actionLoggerDirectory; }
        }

        #endregion


        public Logger(int maxNumerOfLogsInMemory = 512, LogFilesPathLocationEnum logPathEnum = LogFilesPathLocationEnum.App_Logs)
        {
            _actionLoggerDirectory = LibFuncs.getLogFilesPath(logPathEnum);
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
                _theUserActions.Add(string.Format("{0}: {1}!{2} {3}", timeStamp.ToString("H:mm:ss.fff"), frmName, ctrlName, eventName));
                if (_theUserActions.Count > _maxNumerOfLogsInMemory) WriteLogActionsToFile();
            }
        }


        public string GetLogFileName()
        {
            //Check if the current file is > 1 MB and create another
             string[] existingFileList = System.IO.Directory.GetFiles(_actionLoggerDirectory, ACTIONLOGFILEIDENTIFIER +  DateTime.Now.ToString("yyyyMMdd") + "*.log");

            string filePath = _actionLoggerDirectory + ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-0.log";
            if (existingFileList.Count() > 0)
            {
                filePath = _actionLoggerDirectory + ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-" + (existingFileList.Count() - 1).ToString() + ".log";
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length / 1024 > 1000) //Over a MB (ie > 1000 KBs)
                {
                    filePath = _actionLoggerDirectory + ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-" + existingFileList.Count().ToString() + ".log";
                }
            }
            
            return filePath;
        }

        public string[] GetTodaysLogFileNames()
        {
            string[] existingFileList = System.IO.Directory.GetFiles(_actionLoggerDirectory, ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "*.log");
            return existingFileList;
        }

        public void WriteLogActionsToFile()
        {
            string logFilePath = GetLogFileName();
            if (File.Exists(logFilePath))
            {
                //If this fails its because as a Admin you need to run the app as Admin - wierd problem with GPO's
                try
                {
                    File.AppendAllLines(logFilePath, _theUserActions);
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
                    File.WriteAllLines(logFilePath, _theUserActions);
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show("If you see this message its because you're not an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
            _theUserActions = new List<string>();
        }

        #endregion
    }
}
