using LungCare.SupportPlatform.SupportPlatformDAO.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Utils
{
    class UserCacheUtil
    {

        private static string file = "user.txt";
        public static void SaveUserNameAndPassword(string userName, string password)
        {
            try
            {
                StringBuilder content = new StringBuilder();
                content.AppendLine(userName);
                content.AppendLine(password);
                File.WriteAllText(file, content.ToString());
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                Logger.Log(ex.ToString());
            }
            
        }

        public static string[] LoadUserNameAndPassword()
        {
            try
            {
                if (File.Exists(file))
                {
                    return File.ReadAllLines(file);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                Logger.Log(ex.ToString());
            }

            return null;

        }
    }
}
