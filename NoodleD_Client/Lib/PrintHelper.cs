using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WpfClient.Lib
{
    public static class PrintHelper
    {

        // вывод на печать документа
        public static bool PrintFlowDocument(FlowDocument doc, string prnTaskName, string printerName, out string errMsg)
        {
            bool retVal = true; errMsg = "";

            // проверить статус принтера перед печетью
            string result = GetPrinterStatus(printerName);
            if ((result == null) || (result.ToUpper() != "OK"))
            {
                errMsg = string.Format("Ошибка печати чека: принтер \"{0}\" находится в состоянии {1}", printerName, result);
                return false;
            }

            PrintQueue prnQueue = GetPrintQueueByName(printerName);

            PrintDialog printDialog = new PrintDialog();
            printDialog.PageRange = new PageRange(1, 1);
            printDialog.PrintQueue = prnQueue;
            IDocumentPaginatorSource ipag = (doc as IDocumentPaginatorSource);
            try
            {
                printDialog.PrintDocument(ipag.DocumentPaginator, prnTaskName);
                retVal = true;
            }
            catch (Exception e)
            {
                errMsg = string.Format("Ошибка печати чека: {0}\n\tSource: {1}\n\t{2}", e.Message, e.Source, e.StackTrace);
                retVal = false;
            }

            // проверить статус принтера после печати
            if (retVal == true)
            {
                //result = GetPrinterStatus(printerName);
                //if (result.ToUpper() != "OK")
                //{
                //    errMsg = string.Format("Ошибка печати чека: принтер \"{0}\" ПОСЛЕ печати находится в состоянии {1}", printerName, result);
                //    retVal = false;
                //}
            }

            return retVal;
        }


        // получить статус принтера
        public static string GetPrinterStatus(string printerName)
        {
            PrintQueue printer = GetPrintQueueByName(printerName);
            if (printer == null) return string.Concat("The printer \"", printerName, "\" is not installed.");

            PrintQueueStatus status = printer.QueueStatus;
            if (status == PrintQueueStatus.None)
                return "Ok";
            else
                return status.ToString();
        }

        // вернуть принтер (объект очереди печати) по его имени
        public static PrintQueue GetPrintQueueByName(string printerName)
        {
            if (string.IsNullOrEmpty(printerName)) return null;

            List<PrintQueue> printers = getPrintersList();
            if (printers == null) return null;

            return printers.FirstOrDefault<PrintQueue>(p => p.Name == printerName);
        }

        // проверить наличие принтера (по его имени) в системе
        public static bool IsExistPrinterByName(string printerName)
        {
            if (string.IsNullOrEmpty(printerName)) return false;

            List<PrintQueue> printers = getPrintersList();
            if (printers == null) return false;

            foreach (PrintQueue item in printers)
            {
                if (item.Name == printerName) return true;
            }
            return false;
        }

        public static List<PrintQueue> getPrintersList()
        {
            List<PrintQueue> retVal = null;
            try
            {
                LocalPrintServer _printServer = new LocalPrintServer();
                retVal = _printServer.GetPrintQueues().ToList<PrintQueue>();
            }
            catch (Exception)
            {
            }

            return retVal;
        }

        // получить список заданий принтера
        public static List<PrintSystemJobInfo> GetPrinterJobsList(string printerName)
        {
            PrintQueue printer = GetPrintQueueByName(printerName);
            if (printer == null) return null;

            return printer.GetPrintJobInfoCollection().ToList<PrintSystemJobInfo>();
        }

        // получить статус задания
        public static string GetPrinterJobStatus(string prnName, string jobName)
        {
            string prnStatus = GetPrinterStatus(prnName);
            if (prnStatus == null) return null;

            if (prnStatus.ToUpper() == "OK")
            {
                PrintQueue printer = GetPrintQueueByName(prnName);
                if (printer == null) return null;

                PrintSystemJobInfo job = printer.GetPrintJobInfoCollection().FirstOrDefault<PrintSystemJobInfo>(j => j.JobName == jobName);
                if (job == null) return null;
                else return job.JobStatus.ToString();
            }
            else
                return prnStatus;
        }

        public static PrintQueue GetDefaultPrinter()
        {
            PrintQueue retVal = LocalPrintServer.GetDefaultPrintQueue();
            return retVal;
        }

        public static PrintQueue GetFirstPrinter()
        {
            PrintQueue retVal = null;
            List<PrintQueue> prnList = PrintHelper.getPrintersList();
            if ((prnList != null) && (prnList.Count != 0))
            {
                retVal = prnList[0];
            }
            return retVal;
        }

    }  // class
}
