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
            if (result.ToUpper() != "OK")
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
            return getPrintersList().FirstOrDefault<PrintQueue>(p => p.Name == printerName);
        }

        // проверить наличие принтера (по его имени) в системе
        public static bool IsExistPrinterByName(string printerName)
        {
            foreach (PrintQueue item in getPrintersList())
            {
                if (item.Name == printerName) return true;
            }
            return false;
        }

        public static List<PrintQueue> getPrintersList()
        {
            LocalPrintServer _printServer = new LocalPrintServer();

            return _printServer.GetPrintQueues().ToList<PrintQueue>();
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
            if (prnStatus.ToUpper() == "OK")
            {
                PrintQueue printer = GetPrintQueueByName(prnName);
                PrintSystemJobInfo job = printer.GetPrintJobInfoCollection().FirstOrDefault<PrintSystemJobInfo>(j => j.JobName == jobName);
                if (job == null) return null;
                else return job.JobStatus.ToString();
            }
            else
                return prnStatus;
        }


    }
}
