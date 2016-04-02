using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LungCare.SupportPlatform.SupportPlatformDAO.Utils
{
    class Network
    {
        public static bool isConn()
        {
            System.Net.NetworkInformation.Ping ping;
            System.Net.NetworkInformation.PingReply res;
            ping = new System.Net.NetworkInformation.Ping();
            try
            {
                res = ping.Send("116.11.253.243");
                if (res.Status != System.Net.NetworkInformation.IPStatus.Success)
                    return false;
                else
                    return true;
            }
            catch (Exception er)
            {
                return false;
            }
        }
    }
}
