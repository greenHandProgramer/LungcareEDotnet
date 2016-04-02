using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;
using System.Diagnostics;
using System.Threading;

namespace LungCare.SupportPlatform.Network
{
    public partial class MESPDownloadUpload
    {
        private static bool _cancelDownload;
        private string Now
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss.fff");
            }
        }

        public void SendText(WebSocket ws, string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Now + " Sending(Text): " + msg);
            ws.Send(msg);
        }

        public void SendBytes(WebSocket ws, byte[] bytes)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(Now + " Sending(Binary). Length = " + bytes.Length);
            ws.Send(bytes);
            Console.WriteLine(Now + " Sent(Binary). Length = " + bytes.Length);
        }

        public static string UserId { get; set; }
        public static string OrderId { get; set; }
        public static string OrderNo { get; set; }

        // ws://124.115.22.152:11028
        public static string ServerUrl { get; set; }

        static MESPDownloadUpload()
        {
            //ServerUrl = @"ws://124.115.22.152:11028";
            ServerUrl = @"ws://116.11.253.243:11888";

            UserId = "user";
            OrderId = "dbfbd374-3606-4635-9e25-8dd3d56fc987";
            OrderNo = "70778143-22ce-4940-aef1-cc0c08c0ff30";
        }

        public void CreateOrderAsync(
            EventHandler<ExceptionArgs> onExceptionOccurred,
            EventHandler<CreateOrderArgs> onFinished)
        {
            CreateOrderAsync(UserId, onExceptionOccurred, onFinished);
        }

        public void CreateOrderAsync(
            string userId,
            EventHandler<ExceptionArgs> onExceptionOccurred,
            EventHandler<CreateOrderArgs> onFinished)
        {
            Console.ResetColor();

            try
            {
                var ws = new WebSocket(ServerUrl + "/lungcare/FileUpDown");

                ws.OnOpen += delegate
                {
                    Console.WriteLine("Connected: " + ws.Url);
                };

                ws.OnError += delegate (object sender, WebSocketSharp.ErrorEventArgs e)
                {
                    if (_cancelUpload)
                    {
                        return;
                    }

                    Console.WriteLine("ws says(OnError): " + e.Message);
                    if (onExceptionOccurred != null) onExceptionOccurred(this, new ExceptionArgs(e.Exception));

                };

                ws.OnMessage += delegate (object sender, MessageEventArgs e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ws says(OnMessage): " + e.Data);

                    string orderid = e.Data.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    string orderno = e.Data.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[2];

                    MESPDownloadUpload.OrderId = orderid;
                    MESPDownloadUpload.OrderNo = orderno;

                    //string sizeStr = e.Data.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[2];

                    //string[] filesname = filesStr.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                    //string[] sizesname = sizeStr.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    //FileListFinishedArgs flfa = new FileListFinishedArgs();
                    //IEnumerable<FileListItem> items = filesname.Select(
                    //    (item, index) =>
                    //    new FileListItem()
                    //    {
                    //        FileName = filesname[index],
                    //        FileSize = long.Parse(sizesname[index]),
                    //        OrderId = orderId,
                    //        OrderNo = orderNo,
                    //        UserId = userId
                    //    });

                    //flfa.Result = new List<FileListItem>(items);

                    CreateOrderArgs coa = new CreateOrderArgs();
                    coa.OrderCreated = new Order();
                    coa.OrderCreated.OrderId = orderid;
                    coa.OrderCreated.OrderNo = orderno;

                    onFinished(this, coa);
                };

                ws.Connect();

                Console.ForegroundColor = ConsoleColor.Yellow;
                //string msg = "G##orderid1##orderno1##user1";
                string msg = "1##" + userId;
                SendText(ws, msg);
            }
            catch (Exception ex)
            {
                if (onExceptionOccurred != null)
                    onExceptionOccurred(this, new ExceptionArgs(ex));
            }
        }

        public void FetchFileListAsync(
            云数据类型Enum 云数据类型,
            string userId,
            string orderId,
            string orderNo,
            EventHandler<ExceptionArgs> onExceptionOccurred,
            EventHandler<FileListFinishedArgs> onFinished)
        {
            if (onExceptionOccurred == null)
            {
                throw new ArgumentNullException("onExceptionOccurred");
            }
            if (onFinished == null)
            {
                throw new ArgumentNullException("onFinished");
            }

            Console.ResetColor();

            try
            {
                var urlPath = 云数据类型 == 云数据类型Enum.Dicom数据压缩包 ? "/lungcare/DealFileUpDown" : "/lungcare/FileUpDown";

                var ws = new WebSocket(ServerUrl + urlPath);

                ws.OnOpen += delegate
                {
                    Console.WriteLine("Connected: " + ws.Url);
                };

                ws.OnError += delegate(object sender, WebSocketSharp.ErrorEventArgs e)
                {
                    Console.WriteLine("ws says(OnError): " + e.Message);
                    onExceptionOccurred(this, new ExceptionArgs(e.Exception));
                };

                ws.OnMessage += delegate(object sender, MessageEventArgs e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ws says(OnMessage): " + e.Data);

                    string filesStr = e.Data.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    string sizeStr = e.Data.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[2];

                    string[] filesname = filesStr.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                    string[] sizesname = sizeStr.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

                    FileListFinishedArgs flfa = new FileListFinishedArgs();
                    try
                    {
                        IEnumerable<FileListItem> items = filesname.Select(
                            (item, index) =>
                            new FileListItem()
                            {
                                FileName = filesname[index],
                                FileSize = long.Parse(sizesname[index]),
                                OrderId = orderId,
                                OrderNo = orderNo,
                                UserId = userId
                            });

                        flfa.Result = new List<FileListItem>(items);
                    }
                    catch (Exception)
                    {

                        //throw;
                    }
                    onFinished(this, flfa);
                };

                ws.Connect();

                Console.ForegroundColor = ConsoleColor.Yellow;
                //string msg = "G##orderid1##orderno1##user1";
                string msg = "G##" + orderId + "##" + orderNo + "##" + userId;
                SendText(ws, msg);
            }
            catch (Exception ex)
            {
                onExceptionOccurred(this, new ExceptionArgs(ex));
            }
        }

        public void FetchFileListAsync(云数据类型Enum 云数据类型, EventHandler<ExceptionArgs> onExceptionOccurred, EventHandler<FileListFinishedArgs> onFinished)
        {
            FetchFileListAsync(云数据类型, UserId, OrderId, OrderNo, onExceptionOccurred, onFinished);
        }

        public void DownloadFileAsync(
            云数据类型Enum 云数据类型,
            string filename,
            int filesize,
            string localDstFullName,
            EventHandler<ExceptionArgs> onExceptionOccurred,
            EventHandler<FileDownloadFinishedArgs> onFinished,
            EventHandler<ProgressArgs> onProgress)
        {
            DownloadFileAsync(云数据类型, filename: filename, filesize: filesize, localDstFullName: localDstFullName, userId: UserId, orderId: OrderId, orderNo: OrderNo,
                onExceptionOccurred: onExceptionOccurred,
                onFinished: onFinished,
                onProgress: onProgress);
        }

        private void FileDownload(string orderId, string filename, WebSocket ws, int beginPos, Action<Exception> errorCallback)
        {
            // 05c64d09d6b44278a0bac084283ef9d1
            // 173软件下载.txt
            string msg = "D##" + filename + "##" + beginPos;
            SendText(ws, msg);

            _filedownloadEvt = new ManualResetEvent(true);
            _filedownloadEvt.Reset();

            ThreadPool.QueueUserWorkItem(delegate
            {
                Console.WriteLine("start _filedownloadEvt.WaitOne");
                bool waitRet = _filedownloadEvt.WaitOne(new TimeSpan(0, 0, 1, 25));
                Console.WriteLine("end _filedownloadEvt.WaitOne: " + waitRet);
                if (!waitRet)
                {
                    if (!_cancelDownload)
                    {
                        _cancelDownload = true;
                        errorCallback(new Exception("下载文件时服务器返回超时。"));
                    }
                    try { ws.Close(); } catch { }
                    _cancelDownload = true;
                }
            });
        }

        ManualResetEvent _filedownloadEvt;

        private static List<FileStream> _openFileStreams = new List<FileStream>();


        public void DownloadFileAsync(
            云数据类型Enum 云数据类型,
            string filename,
            int filesize,
            string localDstFullName,
            string userId,
            string orderId,
            string orderNo,
            EventHandler<ExceptionArgs> onExceptionOccurred,
            EventHandler<FileDownloadFinishedArgs> onFinished,
            EventHandler<ProgressArgs> onProgress)
        {
            FileStream fs = null;

            var urlPath = 云数据类型 == 云数据类型Enum.Dicom数据压缩包 ? "/lungcare/DealFileUpDown" : "/lungcare/FileUpDown";

            var ws = new WebSocket(ServerUrl + urlPath);

            DateTime startUploadDateTime = DateTime.Now;
            try
            {
                Console.ResetColor();

                string filename2Download = filename;
                _cancelDownload = false;

                int beginPos = 1;


                Stopwatch sw = Stopwatch.StartNew();

                bool downloading = false;
                if (!Directory.Exists(new FileInfo(localDstFullName).Directory.FullName))
                {
                    Directory.CreateDirectory(new FileInfo(localDstFullName).Directory.FullName);
                }

                foreach (var item in _openFileStreams)
                {
                    try
                    {
                        item.Close();
                        item.Dispose();
                    }
                    catch (Exception)
                    {
                    }
                }
                _openFileStreams.Clear();

                if (File.Exists(localDstFullName))
                {
                    beginPos = 1 + (int)new FileInfo(localDstFullName).Length;
                    Console.WriteLine("Open fs for " + localDstFullName);
                    fs = new FileStream(localDstFullName, FileMode.Append);
                }
                else
                {
                    Console.WriteLine("Open fs for " + localDstFullName);
                    fs = new FileStream(localDstFullName, FileMode.Create);
                }
                Console.WriteLine("fs Hash = " + fs.GetHashCode());

                _openFileStreams.Add(fs);

                ws.OnOpen += delegate
                {
                    Console.WriteLine("Connected: " + ws.Url);
                };

                ws.OnError += delegate (object sender, WebSocketSharp.ErrorEventArgs e)
                {
                    try
                    {
                        Console.WriteLine("Closing fs for " + localDstFullName + (fs == null ? "fs is null" : "fs hash = " + fs.GetHashCode()));
                        fs.Close();
                        //fs = null;
                        Console.WriteLine("Closed fs for " + localDstFullName);
                    }
                    catch (Exception closeFSEx)
                    {
                        Console.WriteLine("Close fs failed for " + closeFSEx);
                    }
                    //try { ws.Close(); } catch { }
                    Console.WriteLine("ws says(OnError): " + e.Message);
                    if (!_cancelDownload)
                    {
                        _cancelDownload = true;
                        onExceptionOccurred(this, new ExceptionArgs(e.Exception));
                    }
                    _cancelDownload = true;
                };

                ws.OnMessage += delegate (object sender, MessageEventArgs e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if (e.Data.Length > 1000)
                    {
                        Console.WriteLine("ws says(OnMessage): Length : " + e.RawData.Length);
                    }
                    else
                    {
                        Console.WriteLine("ws says(OnMessage): " + e.Data);
                    }

                    if (_cancelDownload)
                    {
                        try
                        {
                            Console.WriteLine("Closing fs for " + localDstFullName + (fs == null ? "fs is null" : "fs hash = " + fs.GetHashCode()));
                            fs.Close();
                            Console.WriteLine("Closed fs for " + localDstFullName);
                        }
                        catch (Exception closeFSEx)
                        {
                            Console.WriteLine("Close fs failed for " + closeFSEx);
                        }
                        return;
                    }

                    if (_filedownloadEvt != null)
                    {
                        Console.WriteLine("_filedownloadEvt.Set();");
                        _filedownloadEvt.Set();
                    }

                    Console.WriteLine();

                    if (!downloading)
                    {
                        // File.WriteAllBytes("示范案例1.jpg", new byte[0]);
                        FileDownload(orderId, filename2Download, ws, beginPos, delegate (Exception ex)
                        {
                            try
                            {
                                Console.WriteLine("Closing fs for " + localDstFullName + (fs == null ? "fs is null" : "fs hash = " + fs.GetHashCode()));
                                fs.Close();
                                Console.WriteLine("Closed fs for " + localDstFullName);
                            }
                            catch (Exception closeFSEx)
                            {
                                Console.WriteLine("Close fs failed for " + closeFSEx);
                            }
                            _cancelDownload = true;

                            onExceptionOccurred(this, new ExceptionArgs(ex));
                        });
                        downloading = true;
                    }
                    else
                    {
                        Console.WriteLine("About to write fs for " + fs.GetHashCode());

                        fs.WriteBytes(e.RawData);
                        beginPos += e.RawData.Length;

                        if (beginPos > filesize)
                        //if (e.RawData.Length < 20000)
                        {
                            fs.Close();

                            onFinished(this, new FileDownloadFinishedArgs());

                            return;
                        }
                        TimeSpan ellapsed = DateTime.Now - startUploadDateTime;
                        onProgress(this, new ProgressArgs(beginPos, filesize, beginPos / ellapsed.TotalSeconds));

                        FileDownload(orderId, filename2Download, ws, beginPos, delegate (Exception ex)
                        {
                            try
                            {
                                Console.WriteLine("Closing fs for " + localDstFullName + (fs == null ? "fs is null" : "fs hash = " + fs.GetHashCode()));
                                fs.Close();
                                Console.WriteLine("Closed fs for " + localDstFullName);
                            }
                            catch (Exception closeFSEx)
                            {
                                Console.WriteLine("Close fs failed for " + closeFSEx);
                            }
                            _cancelDownload = true;
                            onExceptionOccurred(this, new ExceptionArgs(ex));
                        });

                        // File.WriteAllBytes("Java WebSocket API_1.0_Final.pdf", e.RawData);
                        // Process.Start("Java WebSocket API_1.0_Final.pdf");
                    }

                    // Console.WriteLine("beginPos = " + beginPos);
                    // long ellm = sw.ElapsedMilliseconds / 1000;

                    // if (ellm != 0 && beginPos != 0)
                    // {
                    //     Console.WriteLine("Percentage: " + (beginPos / (float)filesize).ToString("P1"));
                    //     Console.WriteLine("Remains: " + ((filesize - beginPos) / (beginPos / ellm)) + "s");

                    //     Console.WriteLine("Speed: " + ((beginPos / 1024) / ellm) + "KB/s");
                    // }
                };

                ws.Connect();

                Console.ForegroundColor = ConsoleColor.Yellow;
                //string msg = "G##orderid1##orderno1##user1";
                string msg = "G##" + orderId + "##" + orderNo + "##" + userId;
                Console.WriteLine("Sending(Text): " + msg);
                ws.Send(msg);
            }
            catch (Exception ex)
            {
                try
                {
                    Console.WriteLine("Closing fs for " + localDstFullName + (fs == null ? " fs is null" : " fs hash = " + fs.GetHashCode()));
                    fs.Close();
                    Console.WriteLine("Closed fs for " + localDstFullName);
                }
                catch (Exception closeFSEx)
                {
                    Console.WriteLine("Close fs failed for " + closeFSEx);
                }
                try { ws.Close(); } catch { }
                _cancelDownload = true;
                onExceptionOccurred(this, new ExceptionArgs(ex));
            }
        }

        public static void CancelDownload()
        {
            _cancelDownload = true;
        }
    }

    public class ExceptionArgs : EventArgs
    {
        public ExceptionArgs(Exception ex)
        {
            this.Exception = ex;
        }
        public ExceptionArgs(string msg)
        {
            this.Exception = new Exception(msg);
        }

        public Exception Exception { get; set; }
    }

    public class ProgressArgs : EventArgs
    {
        public float? Percentage
        {
            get
            {
                if (!Finished.HasValue)
                {
                    return null;
                }

                return Finished.Value / (float)Total;
            }
        }

        public long Total { get; set; }

        public long? Finished { get; set; }
        public long? Remains
        {
            get
            {
                if (!Finished.HasValue)
                {
                    return null;
                }

                return Total - Finished.Value;
            }
        }

        public double? Speed;

        public string RemainTimeHumanReadable
        {
            get
            {
                if (RemainTimeInMillisecond.HasValue)
                {
                    if (RemainTimeInMillisecond.Value > 60)
                    {
                        return (int)(RemainTimeInMillisecond.Value / 60) + "分钟";
                    }
                    else if (RemainTimeInMillisecond.Value > 3600)
                    {
                        return (int)(RemainTimeInMillisecond.Value / 3600) + "小时";
                    }
                    else
                    {
                        return (int)RemainTimeInMillisecond.Value + "秒";
                    }
                }
                return "未知";
            }
        }
        public long? RemainTimeInMillisecond { get; set; }

        public ProgressArgs(long? finished, long total, double? speed)
        {
            this.Finished = finished;
            this.Total = total;
            this.Speed = speed;

            if (speed.HasValue && finished.HasValue)
            {
                RemainTimeInMillisecond = (int)(Remains / speed);
            }
        }

        public override string ToString()
        {
            return string.Format("{0:P1}", Percentage);
            return string.Format("Finished: {0}, Remains: {2}, Total: {1}, Percentage: {3:P1}", Finished, Total, Remains, Percentage);
        }
    }

    public class FileDownloadFinishedArgs : EventArgs
    {
    }

    public class FileUploadFinishedArgs : EventArgs
    {
        public string FileName { get; set; }

        public FileUploadFinishedArgs(string filename)
        {
            FileName = filename;
        }
    }

    public class FileListFinishedArgs : EventArgs
    {
        public List<FileListItem> Result { get; set; }
    }

    public class FileListItem
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string UserId { get; set; }
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
    }

    public class CreateOrderArgs : EventArgs
    {
        public Order OrderCreated { get; set; }
    }

    public class Order
    {
        public string OrderId { get; set; }
        public string OrderNo { get; set; }
    }

    public enum UploadFileStatus
    {
        NotStartYet,
        申请开始上传文件RequestSent,
        申请开始上传文件ResponseReceived,
        准备开始上传数据,
        传输数据RequestSent,
        传输数据ResponseReceived,
        上传文件结束RequestSent,
        上传文件结束ResponseReceived
    }
    class IndexLength
    {
        public int Index { get; set; }
        public int Length { get; set; }
    }

    public enum 云数据类型Enum
    {
        Dicom数据压缩包,
        处理结果
    }
}
