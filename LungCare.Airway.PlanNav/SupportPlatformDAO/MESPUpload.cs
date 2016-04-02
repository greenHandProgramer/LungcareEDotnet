using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;
using System.Threading;

namespace LungCare.SupportPlatform.Network
{
    public partial class MESPDownloadUpload
    {
        public void CancelUploadManually()
        {
            Console.WriteLine("CancelUploadManually()");
            _cancelUpload = true;
            _cancelUploadManually = true;
            ReleaseUploadResource(wsUpload, "手动取消上传！！！");
        }

        private bool _cancelUpload;
        private bool _cancelUploadManually = false;
        bool waitingResponse = false;
        ManualResetEvent _fileuploadEvt;

        List<byte> byteUploadContent;

        private void ReleaseUploadResource(WebSocket ws,string reason)
        {
            Console.WriteLine("CancelUpload && ReleaseUploadResource(). Reason: " + reason);

            _cancelUpload = true;

            try
            {
                //ws.Close();
            }
            catch
            {
            }

            //try
            //{
            //    ws = null;
            //}
            //catch
            //{
            //}

            byteUploadContent = null;
            GC.Collect();
        }

        WebSocket wsUpload;

        private void CancelTaskResumable(Exception ex, EventHandler<ExceptionArgs> onExceptionOccurred,string reason)
        {
            if(this._cancelUpload || this._cancelUploadManually)
            {
                return;
            }
            ReleaseUploadResource(wsUpload, reason);
            //if (!_cancelUpload)
            {
                if (onExceptionOccurred != null) { onExceptionOccurred(this, new ExceptionArgs(ex)); }
            }
        }

        public void UploadFileAsync(
            云数据类型Enum 云数据类型,
            string filename,
            string userId,
            string orderId,
            string orderNo,
            bool isResume,
            EventHandler<ExceptionArgs> onExceptionOccurred,
            EventHandler<FileUploadFinishedArgs> onFinished,
            EventHandler<ProgressArgs> onProgress)
        {
            if (_cancelUploadManually)
            {
                throw new Exception("本次上传操作已被永久取消。请启动新的上传。");
            }
            if (onExceptionOccurred == null)
            {
                throw new ArgumentNullException("onExceptionOccurred");
            }
            if (onFinished == null)
            {
                throw new ArgumentNullException("onFinished");
            }
            if (onProgress == null)
            {
                throw new ArgumentNullException("onProgress");
            }

            var urlPath = 云数据类型 == 云数据类型Enum.处理结果 ? "/lungcare/DealFileUpDown" : "/lungcare/FileUpDown";

            wsUpload = new WebSocket(url: ServerUrl + urlPath);
            
            try
            {
                _cancelUpload = false;
                _cancelUploadManually = false;

                DateTime startUploadDateTime = DateTime.Now;

                //const int sizePerBlock = 1024 * 100;
                const int sizePerBlock = 1024 * 30;
                //const int sizePerBlock = 2;
                if (byteUploadContent == null)
                {
                    byteUploadContent = new List<byte>(File.ReadAllBytes(filename));
                }

                int totalLength = byteUploadContent.Count;
                int totalBlocks = (int)Math.Ceiling((float)totalLength / sizePerBlock);
                int byteSent = 0;

                int indexLengthIdx = 0;
                IndexLength[] IndexLengthList = new IndexLength[totalBlocks];
                for (int i = 0; i < totalBlocks; ++i)
                {
                    IndexLength item = new IndexLength();
                    item = new IndexLength();
                    item.Index = i * sizePerBlock;
                    item.Length = sizePerBlock;

                    if (item.Index + item.Length > totalLength)
                    {
                        item.Length = totalLength - item.Index;
                    }

                    IndexLengthList[i] = item;
                }

                UploadFileStatus status = UploadFileStatus.NotStartYet;

                wsUpload.OnOpen += delegate
                {
                    Console.WriteLine("Connected: " + wsUpload.Url);
                };

                wsUpload.OnError += delegate (object sender, WebSocketSharp.ErrorEventArgs e)
                {
                    Console.WriteLine("ws says(OnError): " + e.Message);
                    if (!waitingResponse)
                    {
                        CancelTaskResumable(ex: e.Exception, onExceptionOccurred: onExceptionOccurred, reason: "WebSocket出错。没有等待返回消息。");
                    }
                };

                wsUpload.OnMessage += delegate (object sender, MessageEventArgs e)
                {
                    waitingResponse = false;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Now + " ws says(OnMessage): " + e.Data);

                    while (true)
                    {
                        bool shouldLoop = false;

                        if (_cancelUpload)
                        {
                            ReleaseUploadResource(wsUpload, "原因未知");
                            return;
                        }

                        if (status == UploadFileStatus.申请开始上传文件RequestSent)
                        {
                            if (_fileuploadEvt != null)
                            {
                                Console.WriteLine("_fileuploadEvt.Set();");
                                _fileuploadEvt.Set();
                            }

                            status = UploadFileStatus.申请开始上传文件ResponseReceived;

                            if (!isResume)
                            {
                                string expected0Response = "0##" + byteSent;
                                if (e.Data != expected0Response)
                                {
                                    Exception inconsistentDataEx = new Exception(string.Format("Expected response: {0}. Actually: {1}", expected0Response, e.Data));
                                    CancelTaskResumable(ex: inconsistentDataEx, onExceptionOccurred: onExceptionOccurred, reason: "数据跟服务器端不一致");
                                    return;
                                }
                            }
                            else
                            {
                                byteSent = int.Parse(e.Data.Split(new string[] { "##" },StringSplitOptions.RemoveEmptyEntries)[1]);

                                if (byteSent != 0)
                                {
                                    int tmpByteSent = 0;

                                    for (int i = 0; i < IndexLengthList.Length; ++i)
                                    {
                                        tmpByteSent += IndexLengthList[indexLengthIdx].Length;

                                        if (tmpByteSent == byteSent)
                                        {
                                            indexLengthIdx = i + 1;

                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    indexLengthIdx = 0;
                                }
                            }

                            if (indexLengthIdx == IndexLengthList.Length)
                            {
                                // 已经传完了。传完最后一piece后没有收到服务器响应。
                                status = UploadFileStatus.传输数据ResponseReceived;
                                SendText(wsUpload, "2");
                                waitingResponse = true;
                                return;
                            }
                            else
                            {
                                status = UploadFileStatus.准备开始上传数据;
                            }
                        }

                        if (status == UploadFileStatus.准备开始上传数据 || status == UploadFileStatus.传输数据ResponseReceived)
                        {
                            byte[] piece = byteUploadContent.GetRange(
                                IndexLengthList[indexLengthIdx].Index,
                                IndexLengthList[indexLengthIdx].Length).ToArray();

                            SendBytes(wsUpload, piece);
                            waitingResponse = true;

                            _fileuploadEvt = new ManualResetEvent(true);
                            _fileuploadEvt.Reset();

                            ThreadPool.QueueUserWorkItem(delegate
                            {
                                Console.WriteLine("start _fileuploadEvt.WaitOne");
                                bool waitRet = _fileuploadEvt.WaitOne(new TimeSpan(0, 0, 0, 25));
                                Console.WriteLine("end _fileuploadEvt.WaitOne: " + waitRet);
                                if (!waitRet)
                                {
                                    Exception inconsistentDataEx = new Exception("上传文件时服务器返回超时。SendBytes");
                                    CancelTaskResumable(ex: inconsistentDataEx, onExceptionOccurred: onExceptionOccurred, reason: "上传文件时服务器返回超时。SendBytes");
                                }
                            });

                            byteSent += IndexLengthList[indexLengthIdx].Length;
                            indexLengthIdx++;

                            status = UploadFileStatus.传输数据RequestSent;
                        }
                        else if (status == UploadFileStatus.传输数据RequestSent)
                        {
                            if (_fileuploadEvt != null)
                            {
                                Console.WriteLine("_fileuploadEvt.Set();");
                                _fileuploadEvt.Set();
                            }
                            status = UploadFileStatus.传输数据ResponseReceived;

                            string expected1Response = "1##" + IndexLengthList[indexLengthIdx - 1].Length;
                            if (e.Data != expected1Response)
                            {
                                Exception inconsistentDataEx = new Exception(string.Format("Expected response: {0}. Actually: {1}", expected1Response, e.Data));
                                CancelTaskResumable(ex: inconsistentDataEx, onExceptionOccurred: onExceptionOccurred, reason: "数据跟服务器端不一致");
                                return;
                            }

                            TimeSpan ellapsed = DateTime.Now - startUploadDateTime;

                            onProgress(this, new ProgressArgs(byteSent, totalLength, byteSent / ellapsed.TotalSeconds));

                            if (indexLengthIdx > IndexLengthList.Length - 1)
                            {
                                status = UploadFileStatus.上传文件结束RequestSent;
                                SendText(wsUpload, "2");
                                waitingResponse = true;
                                return;
                            }
                            else
                            {
                                shouldLoop = true;
                                //SendText(ws, "0##" + new FileInfo(filename).Name + "##" + byteContent.Count + "##" + orderId + "##0##" + userId + "##" + orderNo);
                                status = UploadFileStatus.准备开始上传数据;
                            }
                        }
                        else if (status == UploadFileStatus.上传文件结束RequestSent)
                        {
                            status = UploadFileStatus.上传文件结束ResponseReceived;

                            string expected2Response = "2##" + totalLength;

                            if (e.Data != expected2Response)
                            {
                                Exception inconsistentDataEx = new Exception(string.Format("Expected response: {0}. Actually: {1}", expected2Response, e.Data));
                                CancelTaskResumable(ex: inconsistentDataEx, onExceptionOccurred: onExceptionOccurred, reason: "数据跟服务器端不一致");
                                return;
                            }

                            wsUpload.Close();
                            onProgress(this, new ProgressArgs(totalLength, totalLength, null));
                            onFinished(this, new FileUploadFinishedArgs(filename));
                        }

                        if (!shouldLoop)
                        {
                            break;
                        }
                    }
                };

                wsUpload.Connect();

                status = UploadFileStatus.NotStartYet;

                // 是否重传文件
                // 1--删除服务器上已存在的文件，重新传输。
                // 0--保留服务器上的文件

                string msg = "0##" + new FileInfo(filename).Name + "##" + byteUploadContent.Count + "##" + orderId + "##" + (isResume ? 0 : 1 ) + "##" + userId + "##" + orderNo;
                SendText(wsUpload, msg);

                _fileuploadEvt = new ManualResetEvent(true);
                _fileuploadEvt.Reset();

                ThreadPool.QueueUserWorkItem(delegate
                {
                    Console.WriteLine("start _fileuploadEvt.WaitOne");
                    bool waitRet = _fileuploadEvt.WaitOne(new TimeSpan(0, 0, 0, 5));
                    Console.WriteLine("end _fileuploadEvt.WaitOne: " + waitRet);
                    if (!waitRet)
                    {
                        Exception inconsistentDataEx = new Exception("上传文件时服务器返回超时。Sent:" + msg);
                        CancelTaskResumable(ex: inconsistentDataEx, onExceptionOccurred: onExceptionOccurred, reason: "上传文件时服务器返回超时。Sent:" + msg);
                    }
                });
                waitingResponse = true;

                status = UploadFileStatus.申请开始上传文件RequestSent;
            }
            catch (Exception ex)
            {
                CancelTaskResumable(ex: ex, onExceptionOccurred: onExceptionOccurred, reason: "主线程异常。");
            }
        }
    }
}
