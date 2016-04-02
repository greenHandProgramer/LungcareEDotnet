using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Files
{
    class FileDAO
    {
        public static String HumanReadableFilesize(long? size)
        {
            if (!size.HasValue)
            {
                return "";
            }

            double size1 = size.Value;
            String[] units = new String[] { "B", "KB", "MB", "GB", "TB", "PB" };
            double mod = 1024.0;
            int i = 0;
            while (size1 >= mod)
            {
                size1 /= mod;
                i++;
            }

            return size1.ToString("F1") + units[i];
            return Math.Round(size1) + units[i];
        }
        public static string zipFolder2ZipWithFolder(string sourceFolder ,string destZipFile)
        {

            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", "PatientName", "InstitutionName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            //string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, fileName);
            string args = string.Format("a -tzip \"{0}\" \"{1}\"", fileName, sourceFolder);
            var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
            psi.CreateNoWindow = true;
            Process.Start(psi).WaitForExit();
            System.Windows.Forms.MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", fileName));
            //Process.Start(new FileInfo(fileName).Directory.FullName);

            return fileName;
        }


        public static string zipFolderFiles2ZipWithoutFolder(string sourceFolder, string destZipFile)
        {

            string fileName = string.Format("[{0}][{3}][{1}][{2}].zip", "PatientName", "InstitutionName", System.Net.Dns.GetHostName(), DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));

            //string args = string.Format("a -tzip \"{0}\" \"{1}\" \"{2}\ss" \"{3}\"", fileName, SelectedPatient.GetFile(""), SelectedPatient.DicomFolder, fileName);
            string args = string.Format("a -tzip \"{0}\" \"{1}\"\\*", fileName, sourceFolder);
            var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
            psi.CreateNoWindow = true;
            Process.Start(psi).WaitForExit();
            System.Windows.Forms.MessageBox.Show(string.Format(@"压缩完毕。
文件保存到{0}。", fileName));
            //Process.Start(new FileInfo(fileName).Directory.FullName);

            return fileName;
        }

        public static void upZip(string zipFile , string destFolder)
        {
            if (File.Exists(zipFile))
            {
                if(!Directory.Exists(destFolder))
                {
                    Directory.CreateDirectory(destFolder);
                }
                string args = string.Format("x -o\"{0}\" -y -sn \"{1}\"", destFolder, zipFile);
                var psi = new ProcessStartInfo(System.Windows.Forms.Application.StartupPath + @"\HaoZipC.exe", args);
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi).WaitForExit();
            }
        }


     /// <summary>
        /// 递归拷贝所有子目录。
        /// </summary>
        /// <param name="sPath">源目录</param>
        /// <param name="dPath">目的目录</param>
        private void copyDirectory(string sPath,string dPath)
        {
            string[] directories = System.IO.Directory.GetDirectories(sPath);
            if(!System.IO.Directory.Exists(dPath))
                System.IO.Directory.CreateDirectory(dPath);
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(sPath);
            System.IO.DirectoryInfo[] dirs = dir.GetDirectories();
            CopyFile(dir,dPath);
            if(dirs.Length>0)
            {
                foreach(System.IO.DirectoryInfo temDirectoryInfo in dirs)
                {
                    string sourceDirectoryFullName = temDirectoryInfo.FullName;
                    string destDirectoryFullName = sourceDirectoryFullName.Replace(sPath,dPath);
                    if(!System.IO.Directory.Exists(destDirectoryFullName))
                    {
                        System.IO.Directory.CreateDirectory(destDirectoryFullName);
                    }
                    CopyFile(temDirectoryInfo,destDirectoryFullName);
                    copyDirectory(sourceDirectoryFullName,destDirectoryFullName);
                }
            }
            
        }
        
        /// <summary>
        /// 拷贝目录下的所有文件到目的目录。
        /// </summary>
        /// <param name="path">源路径</param>
        /// <param name="desPath">目的路径</param>
        private void CopyFile(System.IO.DirectoryInfo path, string desPath)
        {
            string sourcePath = path.FullName;
            System.IO.FileInfo[] files = path.GetFiles();
            foreach (System.IO.FileInfo file in files)
            {
                string sourceFileFullName = file.FullName;
                string destFileFullName = sourceFileFullName.Replace(sourcePath, desPath);
                file.CopyTo(destFileFullName, true);
            }
        }


        public static void DeleteFolder(string folder)
        {
            //Directory.Delete(folder, true);
            //return;
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder);
                foreach (var item in files)
                {
                    File.Delete(item);
                }
            }
            
            string[] dirs = Directory.GetDirectories(folder);
            if (dirs.Length == 0)
            {
                Directory.Delete(folder);
                return;
            }

            foreach (var item in dirs)
            {
                DeleteFolder(item);
            }
        }


        static int fileNum = 0;
        /// <summary>  
        /// 获取某目录下的所有文件(包括子目录下文件)的数量  
        /// </summary>  
        /// <param name="srcPath"></param>  
        /// <returns></returns>  
        public static int GetFileNum(string srcPath)
        {
            try
            {
                // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组  
                string[] fileList = System.IO.Directory.GetFileSystemEntries(srcPath);
                // 遍历所有的文件和目录  
                foreach (string file in fileList)
                {
                    // 先当作目录处理如果存在这个目录就重新调用GetFileNum(string srcPath)  
                    if (System.IO.Directory.Exists(file))
                        GetFileNum(file);
                    else
                        fileNum++;
                }

            }
            catch (Exception e)
            {

                return fileNum;
            }
            return fileNum;
        }



        public static int GetFilesCount(string pmFilePath)
        {
            int totalFile = 0;
            if (Directory.Exists(pmFilePath))
            {
                totalFile = Directory.GetFiles(pmFilePath).Length;

                string[] fileList = Directory.GetDirectories(pmFilePath);
                foreach (string fileChild in fileList)
                {
                    totalFile += GetFilesCount(fileChild);
                }
            }
            return totalFile;
        }
    }
}
