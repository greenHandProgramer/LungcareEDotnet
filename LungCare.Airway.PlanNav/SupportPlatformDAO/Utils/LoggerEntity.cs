using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO;
namespace LungCare.SupportPlatform.SupportPlatformDAO.Utils
{
    class LoggerEntity
    {
        private static log4net.ILog log;
        private static string fileError;
        private static string fileInfo;
        public static void Info(Type type , string message)
        {
            fileInfo = "c:\\Logs\\Info\\"+string.Format("{0:yyyy-MM-dd}", DateTime.Now)+"_Info.txt";
            if (!Directory.Exists(Path.GetDirectoryName(fileInfo)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileInfo));
            }
            string datetime = string.Format("{0:yyyy-MM-dd  HH:mm:ss}", DateTime.Now);
            File.AppendAllText(fileInfo , datetime+"  ");
            File.AppendAllText(fileInfo , message+"\r\n");
            //if (log == null)
            //{
            //    log = log4net.LogManager.GetLogger(type);
            //    string datetime = string.Format("{0:yyyy-MM-dd  HH:mm:ss}", DateTime.Now);
            //    log.Info(datetime);
            //    log.Info(message);
            //}
        }


        public static void Error(Type type, string message)
        {
            fileError = "c:\\Logs\\Error\\" + string.Format("{0:yyyy-MM-dd}", DateTime.Now) + "_Error.txt";
            if (!Directory.Exists(Path.GetDirectoryName(fileError)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileError));
            }

            string datetime = string.Format("{0:yyyy-MM-dd  HH:mm:ss}", DateTime.Now);
            File.AppendAllText(fileError, datetime+"  ");
            File.AppendAllText(fileError, message+"\r\n");

            //if (log == null)
            //{
            //    log = log4net.LogManager.GetLogger(type);
            //    string datetime = string.Format("{yyyy:MM:dd  HH:mm:ss}", DateTime.Now);
            //    log.Error(datetime);
            //    log.Error(message);
            //}
        }



    }
}
