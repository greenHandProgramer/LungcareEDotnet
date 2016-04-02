using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AirwayEMT配准结果生成器.NDIServer
{
    internal class AuroraSocketClient
    {
        internal event EventHandler<IncomingAuroraSensorDataEntity> IncomingAuroraSensorDataEvent;

        public void Connect(string ip)
        {
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ip), 1333);
            Socket clientSocket = new Socket(iep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine(@"Connecting " + iep);
            clientSocket.Connect(iep);
            Console.WriteLine(iep + @" connected");

            AuroraSensorData AuroraSensorData = new AuroraSensorData();
            byte[] bytes = AuroraSensorData.FromByteArray();

            while (true)
            {
                while (clientSocket.Available < bytes.Length)
                {
                    //Console.WriteLine("clientSocket.Available = " + clientSocket.Available);
                }

                int size = clientSocket.Receive(bytes);
                //Console.WriteLine(string.Format("Data Received. Size = {0}", size));
                AuroraSensorData = AuroraSensorData.FromByteArray(bytes);
                //Console.WriteLine(string.Format(AuroraSensorData.TimeStamp.ToLongTimeString()));

                if (IncomingAuroraSensorDataEvent != null)
                {
                    IncomingAuroraSensorDataEvent(this, new IncomingAuroraSensorDataEntity(AuroraSensorData));
                    //Thread.Sleep(100);
                }
            }
        }
    }

    internal class IncomingAuroraSensorDataEntity : EventArgs
    {
        public IncomingAuroraSensorDataEntity(AuroraSensorData auroraSensorData)
        {
            this.AuroraSensorData = auroraSensorData;
        }

        internal AuroraSensorData AuroraSensorData { get; set; }
    }
}
