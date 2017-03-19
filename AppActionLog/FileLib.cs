using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AppActionNS
{
    public static class FileLib
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

    public enum LogFilesPathLocationEnum
    {
        App, App_Logs, UserTemp, UserTemp_Logs
    }

}
