using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppActionNS
{
    public class AppActionLogger
    {
        const int MAXCOUNTOFRECORDS = 200;

        private int _recCounter;
        private List<List<UICAction>> _actionBuffer;
        List<UICAction> _curBuffer;

        public AppActionLogger()
        {
            _recCounter = 0;

            // создать буферы для хранения действий пользователя
            _curBuffer = new List<UICAction>();
            _actionBuffer = new List<List<UICAction>>();
            _actionBuffer.Add(_curBuffer);
        }

        public void AddAction(UICAction action)
        {
            action.nubmer = ++_recCounter;
            action.dateTime = DateTime.Now;

            _curBuffer.Add(action);

            // достигнуто максимальное количество записей в текущем буфере
            if (_recCounter == MAXCOUNTOFRECORDS)
            {
                // сохранить ссылку на заполненных буфер для его сброса в БД
                List<UICAction> saveBuf = _curBuffer;
                // запись в БД - в отдельном потоке
                ParameterizedThreadStart tDlg = new ParameterizedThreadStart(writeBufferToDB);
                Thread tSave = new Thread(tDlg);
                tSave.Start(saveBuf);

                // найти или создать следующий буфер для записей
                List<UICAction> tmpBuf = _actionBuffer.FirstOrDefault(b => !b.Equals(_curBuffer) && (b.Count == 0));
                if (tmpBuf == null)
                {
                    tmpBuf = new List<UICAction>(); 
                    _actionBuffer.Add(tmpBuf);
                    _curBuffer = tmpBuf;
                }
            }
        }  // method

        // сохранить и очистить текущий буфер
        private void writeBufferToDB(object data)
        {
            List<UICAction> listActions = (List<UICAction>)data;


        }

        public void ResetRecCounter() { _recCounter = 0; }

    } // class

    // класс, описывающий действие пользовательского контрола
    // UIC - UserInterfaceControl
    public class UICAction
    {
        public string deviceId { get; set; }
        public string orderNumber { get; set; }
        public int nubmer { get; set; }
        public DateTime dateTime { get; set; }
        public string formName { get; set; }
        public UICActionType actionType { get; set; }
        public string controlName { get; set; }
    }

    public enum UICActionType
    {
        Click, Drag
    }

}
