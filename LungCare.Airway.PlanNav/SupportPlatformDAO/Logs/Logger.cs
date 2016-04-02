using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Logs
{
    public class Logger
    {
        static string logFilePath = @"../Log/" + string.Format("{0:yyyy-MM-dd hh-mm-ss}", DateTime.Now) + ".txt";
        public static void Log(string message)
        {
            if (!Directory.Exists(new FileInfo(logFilePath).Directory.FullName))
            {
                Directory.CreateDirectory(new FileInfo(logFilePath).Directory.FullName);
            }
            string time = string.Format("{0:yyyy-MM-dd hh-mm-ss}", DateTime.Now);
            time += "\r\n";
            time += message + "\r\n";
            File.AppendAllText(logFilePath, time);
        }
    }
}
