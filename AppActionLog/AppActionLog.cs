using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AppActionNS
{
    public class AppActionLogger: IDisposable
    {
        const int MAXCOUNTOFRECORDS = 200;
        const string ACTIONLOGFILEIDENTIFIER = "actionLog";
        const string ACTIONLOGFILEEXTENSION = ".txt";

        private int _recCounter;
        private List<List<UICAction>> _actionBuffer;
        List<UICAction> _curBuffer;

        private string _singleLogFile = null;

        public AppActionLogger()
        {
            _recCounter = 0;

            // создать буферы для хранения действий пользователя
            _curBuffer = new List<UICAction>();
            _actionBuffer = new List<List<UICAction>>();
            _actionBuffer.Add(_curBuffer);

            _singleLogFile = getLogFileName();
        }

        public void AddAction(UICAction action)
        {
            action.nubmer = ++_recCounter;
            action.dateTime = DateTime.Now;

            string msg = string.Format("\r\n{0};{1};{2};{3};{4};{5};{6}", action.nubmer, action.dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), action.deviceId, action.orderNumber, action.formName, action.actionType.ToString(), action.value);

            writeLogActionsToFile(msg);

            //_curBuffer.Add(action);

            //// достигнуто максимальное количество записей в текущем буфере
            //if (_recCounter == MAXCOUNTOFRECORDS)
            //{
            //    // сохранить ссылку на заполненный буфер для его сброса в БД
            //    List<UICAction> saveBuf = _curBuffer;
            //    // запись в БД - в отдельном потоке
            //    ParameterizedThreadStart tDlg = new ParameterizedThreadStart(writeBufferToDB);
            //    Thread tSave = new Thread(tDlg);
            //    tSave.Start(saveBuf);

            //    // найти или создать следующий буфер для записей
            //    List<UICAction> tmpBuf = _actionBuffer.FirstOrDefault(b => !b.Equals(_curBuffer) && (b.Count == 0));
            //    if (tmpBuf == null)
            //    {
            //        tmpBuf = new List<UICAction>(); 
            //        _actionBuffer.Add(tmpBuf);
            //        _curBuffer = tmpBuf;
            //    }
            //}
        }  // method

        // сохранить и очистить текущий буфер
        private void writeBufferToDB(object data)
        {
            lock (this)
            {
                List<UICAction> listActions = (List<UICAction>)data;
                string[] aMsg = new string[listActions.Count];
                int i = 0;
                foreach (UICAction item in listActions)
                {
                    string msg;
                    //msg = string.Format("{0};{1};{2};{3};{4};{5};{6}", item.nubmer, item.dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), item.deviceId, item.orderNumber, item.formName, item.actionType.ToString(), item.controlName);
                    msg = string.Format("{0};{1};{2};{3};{4};{5};{6}", item.nubmer, item.dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"), item.deviceId, item.orderNumber, item.formName, item.actionType.ToString(), item.value);

                    aMsg[i] = msg; i++;
                }

                writeLogActionsToFile(aMsg);
            }  // lock

        }  // method

        private void writeLogActionsToFile(string[] aMsg)
        {
            string logFilePath = getLogFileName();

            if (File.Exists(logFilePath))
            {
                //If this fails its because as a Admin you need to run the app as Admin - wierd problem with GPO's
                try
                {
                    File.AppendAllLines(logFilePath, aMsg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("If you see this message its because you're an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    File.WriteAllLines(logFilePath, aMsg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("If you see this message its because you're not an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
        }  // method

        private void writeLogActionsToFile(string msg)
        {
            string logFilePath;
            if (_singleLogFile == null) logFilePath = getLogFileName();
            else logFilePath = _singleLogFile;

            if (File.Exists(logFilePath))
            {
                //If this fails its because as a Admin you need to run the app as Admin - wierd problem with GPO's
                try
                {
                    File.AppendAllText(logFilePath, msg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("If you see this message its because you're an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(logFilePath, msg);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("If you see this message its because you're not an Admin you need to run as Admin - wierd problem with GPO's" + ex.Message + ex.StackTrace);
                }
            }
        }  // method


        private string getLogFileName()
        {
            string actionLoggerDirectory = FileLib.getLogFilesPath(LogFilesPathLocationEnum.App_Logs);

            //Check if the current file is > 1 MB and create another
            string[] existingFileList = System.IO.Directory.GetFiles(actionLoggerDirectory, ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "*" + ACTIONLOGFILEEXTENSION);

            string filePath = actionLoggerDirectory + ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-0" + ACTIONLOGFILEEXTENSION;
            if (existingFileList.Count() > 0)
            {
                filePath = actionLoggerDirectory + ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-" + (existingFileList.Count() - 1).ToString() + ACTIONLOGFILEEXTENSION;
                FileInfo fi = new FileInfo(filePath);
                if (fi.Length / 1024 > 1000) //Over a MB (ie > 1000 KBs)
                {
                    filePath = actionLoggerDirectory + ACTIONLOGFILEIDENTIFIER + DateTime.Now.ToString("yyyyMMdd") + "-" + existingFileList.Count().ToString() + ACTIONLOGFILEEXTENSION;
                }
            }

            return filePath;
        }
        public void ResetRecCounter() { _recCounter = 0; }


        public void Close()
        {
            Dispose();
        }
        public void Dispose()
        {
            writeBufferToDB(_curBuffer);
            _curBuffer.Clear();
        }
    } // class

    // класс, описывающий действие пользовательского контрола
    // UIC - UserInterfaceControl
    public class UICAction
    {
        internal int nubmer { get; set; }
        internal DateTime dateTime { get; set; }


        public string deviceId { get; set; }

        public string orderNumber { get; set; }

        public string formName { get; set; }

        public AppActionsEnum actionType { get; set; }

        public string value { get; set; }
    }

}
