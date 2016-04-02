/*=========================================================================

  Program:   集翔多维导航服务
  Language:  C#

  Copyright (c) 北京集翔多维信息技术有限公司. All rights reserved.

=========================================================================*/
using System;
using Microsoft.Win32;
using System.IO;

namespace AirPatientForm
{
    class RegistryDAO
    {
        public static string AirwayBaseFolder
        {
            get { return RegistryDAO.GetRegistData("AirwayPatientsRootPath"); }
        }

        public static void Validate()
        {
            return;

            if (!AirwayBaseFolderConfigured)
            {
                throw new Exception("AirwayPatientsRootPath in reg has not been configured.");
            }
            if (!Directory.Exists(AirwayBaseFolder))
            {
                throw new Exception("AirwayPatientsRootPath does not exist : " + AirwayBaseFolder);
            }
        }

        public static bool AirwayBaseFolderConfigured
        {
            get { return RegistryDAO.IsRegeditExit("AirwayPatientsRootPath"); }
        }

        public static string GetRegistData(string name)
        {
            if (!IsRegeditExit(name))
            {
                return "";
                // CreateRegedit(@"d:\Airway\AirwayVE\Db\");
            }

            string registData ="";
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE");
            RegistryKey aimdir = software.OpenSubKey("LungCare").OpenSubKey("AirwayNav");
            registData = (string)aimdir.GetValue(name);
            return registData;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">AirwayPatientsRootPath</param>
        /// <returns></returns>
        public static bool IsRegeditExit(string name)
        {
            bool _exit = false;
            string[] subkeyNames;
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            subkeyNames = software.GetSubKeyNames();
            bool flag = false;
            foreach (string keyName in subkeyNames)
            {
                if (keyName == "LungCare")
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                RegistryKey LungCare = software.OpenSubKey("LungCare", true);
                subkeyNames = LungCare.GetSubKeyNames();
                bool flag1 = false;
                foreach (string keyName in subkeyNames)
                {
                    if (keyName == "AirwayNav")
                    {
                        flag1 = true;
                        break;
                    }
                }
                if (flag1)
                {
                    RegistryKey AirwayNav = LungCare.OpenSubKey("AirwayNav", true);
                    subkeyNames = AirwayNav.GetValueNames();
                    bool flag2 = false;
                    foreach (string keyName in subkeyNames)
                    {
                        if (keyName == name)
                        {
                            string temp = AirwayNav.GetValue(keyName).ToString();
                            if (AirwayNav.GetValue(keyName).ToString() != "")
                            {
                                flag2 = true;
                                _exit = true;
                                return _exit;
                            }
                            else
                            {
                                flag2 = false;
                                _exit = false;
                                return _exit;
                            }
                        }
                    }
                    if (!flag2)
                    {
                        _exit = false;
                        return _exit;
                    }
                }
                else
                {
                    _exit = false;
                }
            }
            else
            {
                _exit = false;
            }
           
            return _exit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public static void CreateRegedit(string name , string value)
        {
            RegistryKey hkml = Registry.CurrentUser;
            RegistryKey software = hkml.OpenSubKey("SOFTWARE", true);
            RegistryKey LungCare =  software.CreateSubKey("LungCare");
            RegistryKey AirwayNav =  LungCare.CreateSubKey("AirwayNav");
            AirwayNav.SetValue(name, value, RegistryValueKind.String); ;
            //AirwayNav.SetValue("AirwayPatientsRootPath", value, RegistryValueKind.String);
        }
    }
}
