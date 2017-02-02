using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;

namespace UserActionLog
{
    public static class LibFuncs
    {
        public static string GetAppFileName()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.ManifestModule.Name;
        }

        public static string GetAppFullFile()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            return assembly.Location;
        }

        public static string GetAppDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string getLogFilesPath(LogFilesPathLocationEnum pathEnum)
        {
            string sysPath = string.Empty;

            switch (pathEnum)
            {
                case LogFilesPathLocationEnum.App:
                    sysPath = GetAppDirectory(); 
                    break;
                case LogFilesPathLocationEnum.App_Logs:
                    sysPath = GetAppDirectory() + "Logs\\";
                    break;
                case LogFilesPathLocationEnum.UserTemp:
                    sysPath = Path.GetTempPath();
                    break;
                case LogFilesPathLocationEnum.UserTemp_Logs:
                    sysPath = Path.GetTempPath() + "Logs\\";
                    break;
                default:
                    break;
            }
            if (!Directory.Exists(sysPath)) Directory.CreateDirectory(sysPath);

            return sysPath;
        }


    }  // class


}
