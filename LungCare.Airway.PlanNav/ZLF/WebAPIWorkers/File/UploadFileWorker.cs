using LungCare.SupportPlatform.Network;
using LungCare.SupportPlatform.Security;
using System;
using System.IO;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    public class UploadFileWorker
    {
        public enum UploadStatus
        {
            未创建订单,
            创建订单失败,
            已创建订单,
            已上传dicom信息并置于待上传文件,
            上传dicom信息并置于待上传文件失败,
            上传文件完毕,
            上传文件失败,
            已上传dicom信息并置于已上传文件,
            上传dicom信息并置于已上传文件失败
        }

        private UploadStatus _uploadStatus
        {
            get { return __uploadStatus; }
            set
            {
                Console.WriteLine(value);
                __uploadStatus = value;
            }
        }

        private UploadStatus __uploadStatus = UploadStatus.未创建订单;

        // 0. 创建订单    创建完后保留orderid。出错：从0开始
        // 1. 上传dicom信息（待上传） 上传完毕后记录flag 出错：从1开始
        // 2. 上传文件，续传  记录创建文件报文是否成功 出错：如果创建文件报文就没成功，从2开始。如果创建成功，续传
        // 3. 上传dicom信息（已上传） 出错：从3开始 
        private MESPDownloadUpload MESPDownloadUpload = new MESPDownloadUpload();
        
        public void ManuallyCancelUpload()
        {
            MESPDownloadUpload.CancelUploadManually();
        }

        public void UploadFile(
            云数据类型Enum 云数据类型,
            string filename,
            bool isResume,
            Action successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback,
            Action<ProgressArgs> uploadProgressCallback)
        {
            MESPDownloadUpload.UploadFileAsync(
                云数据类型,
                filename,
                SessionManager.UserName,
                MESPDownloadUpload.OrderId,
                MESPDownloadUpload.OrderNo,
                isResume,
                new EventHandler<ExceptionArgs>(delegate (Object senderUploadFile, ExceptionArgs eUploadFile)
                {
                    //CancelOrderWorker.SendCancelOrderRequeset(
                    //    MESPDownloadUpload.OrderId,
                    //    successCallback: delegate (Models.CancelOrderResponse response)
                    //    {
                    //    },
                    //    failureCallback: delegate (string cancelOrderFailureReason)
                    //    {
                    //    },
                    //    errorCallback: delegate (Exception ex)
                    //    {
                    //    });
                    errorCallback(eUploadFile.Exception);
                }),
                new EventHandler<FileUploadFinishedArgs>(delegate (Object senderUploadFile, FileUploadFinishedArgs eUploadFile)
                {
                    successCallback();
                }),
                new EventHandler<ProgressArgs>(delegate (Object senderProgress, ProgressArgs eProgress)
                {
                    uploadProgressCallback(eProgress);
                })
            );
        }

        private void UploadDicomInfo(
            string status,
            Action successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            UploadDicomInfo(filename,
                InstitutionName,
                PatientAge,
                PatientName,
                PatientSex,
                SeriesInstanceUID,
                StudyInstanceUID,
                acquisitionDate,
                acquisitionTime, UploadTimestamp,
                status,
                successCallback,
                failureCallback,
                errorCallback);
        }

        private void UploadDicomInfo(
            string filename,
            string InstitutionName,
            string PatientAge,
            string PatientName,
            string PatientSex,
            string SeriesInstanceUID,
            string StudyInstanceUID,
            string acquisitionDate,
            string acquisitionTime,
            DateTime UploadTimestamp,
            string status,
            Action successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.CTDicomInfo ctDicomInfo = new Models.CTDicomInfo();

            //ctDicomInfo.InstitutionName = "InstitutionName";
            //ctDicomInfo.PatientAge = "35";
            //ctDicomInfo.PatientName = new FileInfo(filename).Name;
            //ctDicomInfo.PatientSex = "F";
            //ctDicomInfo.SeriesInstanceUID = Guid.NewGuid().ToString();
            //ctDicomInfo.StudyInstanceUID = Guid.NewGuid().ToString();
            //ctDicomInfo.UploadTimestamp = DateTime.Now;

            ctDicomInfo.InstitutionName = InstitutionName;
            ctDicomInfo.PatientAge = PatientAge;
            ctDicomInfo.PatientName = PatientName;
            ctDicomInfo.PatientSex = PatientSex;
            ctDicomInfo.SeriesInstanceUID = SeriesInstanceUID;
            ctDicomInfo.StudyInstanceUID = StudyInstanceUID;
            ctDicomInfo.UploadTimestamp = UploadTimestamp;

            ctDicomInfo.AcquisitionDate = acquisitionDate;
            ctDicomInfo.AcquisitionTime = acquisitionTime;

            UpdateDataWorker.SendUpdateDataRequest(
                ctDicomInfo,
                new FileInfo(filename).Name,
                status,
                successCallback: delegate (Models.GeneralWebAPIResponse response)
                {
                    successCallback();
                },
                failureCallback: delegate (string failureReason)
                {
                    failureCallback(failureReason);
                },
                errorCallback: delegate (Exception ex)
                {
                    errorCallback(ex);
                });
        }

        //public void ResumeUploadFileThenUploadDicomInfo(
        //    string filename,
        //    string InstitutionName,
        //    string PatientAge,
        //    string PatientName,
        //    string PatientSex,
        //    string SeriesInstanceUID,
        //    string StudyInstanceUID,
        //    string acquisitionDate,
        //    string acquisitionTime,
        //    DateTime UploadTimestamp,
        //    Action successCallback,
        //    Action<string> failureCallback,
        //    Action<Exception> errorCallback,
        //    Action<ProgressArgs> uploadProgressCallback)
        //{
        //    UploadFile(
        //        filename,
        //        true,
        //        delegate
        //        {
        //            UploadDicomInfo(
        //                filename,
        //                InstitutionName,
        //                PatientAge,
        //                PatientName,
        //                PatientSex,
        //                SeriesInstanceUID,
        //                StudyInstanceUID,
        //                acquisitionDate,
        //                acquisitionTime,
        //                UploadTimestamp,
        //                "已上传",
        //                successCallback,
        //                delegate (string failureReason)
        //                {
        //                    failureCallback(failureReason);
        //                },
        //                delegate (Exception ex)
        //                {
        //                    errorCallback(ex);
        //                });
        //        },
        //        failureCallback,
        //        errorCallback,
        //        uploadProgressCallback);
        //}

        private string filename;
        private string InstitutionName;
        private string PatientAge;
        private string PatientName;
        private string PatientSex;
        private string SeriesInstanceUID;
        private string StudyInstanceUID;
        private string acquisitionDate;
        private string acquisitionTime;
        private DateTime UploadTimestamp;

        private Action successCallback;
        private Action<string> failureCallback;
        private Action<Exception> errorCallback;
        private Action<ProgressArgs> uploadProgressCallback;

        private void UploadDicomInfoThenUploadFileThenUploadDicomInfo()
        {

        }

        public void CreateOrderThenUploadFileThenUploadDicomInfo(
            string filename,
            string InstitutionName,
            string PatientAge,
            string PatientName,
            string PatientSex,
            string SeriesInstanceUID,
            string StudyInstanceUID,
            string acquisitionDate,
            string acquisitionTime,
            DateTime UploadTimestamp,
            Action successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback,
            Action<ProgressArgs> uploadProgressCallback)
        {
            this.successCallback = successCallback;
            this.failureCallback = failureCallback;
            this.errorCallback = errorCallback;
            this.uploadProgressCallback = uploadProgressCallback;

            this.filename = filename;
            this.InstitutionName = InstitutionName;
            this.PatientAge = PatientAge;
            this.PatientName = PatientName;
            this.PatientSex = PatientSex;
            this.SeriesInstanceUID = SeriesInstanceUID;
            this.StudyInstanceUID = StudyInstanceUID;
            this.acquisitionDate = acquisitionDate;
            this.acquisitionTime = acquisitionTime;
            this.UploadTimestamp = UploadTimestamp;

            Console.WriteLine(_uploadStatus);
            if (_uploadStatus == UploadStatus.未创建订单 || _uploadStatus == UploadStatus.创建订单失败)
            {
                StartFrom未创建订单();
            }
            else if (_uploadStatus == UploadStatus.已创建订单 || _uploadStatus == UploadStatus.上传dicom信息并置于待上传文件失败)
            {
                StartFrom已创建订单();
            }
            else if (_uploadStatus == UploadStatus.已上传dicom信息并置于待上传文件 || _uploadStatus == UploadStatus.上传文件失败)
            {
                StartFrom已上传dicom信息并置于待上传文件();
            }
            else if (_uploadStatus == UploadStatus.上传文件完毕 || _uploadStatus == UploadStatus.上传dicom信息并置于已上传文件失败)
            {
                StartFrom上传文件完毕();
            }
            else if (_uploadStatus == UploadStatus.已上传dicom信息并置于已上传文件)
            {
                StartFrom已上传dicom信息并置于已上传文件();
            }
        }

        private void StartFrom未创建订单()
        {
            Console.WriteLine("StartFrom未创建订单");
            MESPDownloadUpload.CreateOrderAsync(
                SessionManager.UserName,
                new EventHandler<ExceptionArgs>(delegate (Object senderInner, ExceptionArgs eInner)
                {
                    _uploadStatus = UploadStatus.创建订单失败;
                    errorCallback(eInner.Exception);
                }),
                new EventHandler<CreateOrderArgs>(delegate (Object senderInner, CreateOrderArgs eInner)
                {
                    MESPDownloadUpload.OrderId = eInner.OrderCreated.OrderId;
                    MESPDownloadUpload.OrderNo = eInner.OrderCreated.OrderNo;

                    _uploadStatus = UploadStatus.已创建订单;
                    StartFrom已创建订单();
                }));
        }

        private void StartFrom已创建订单()
        {
            Console.WriteLine("StartFrom已创建订单");
            UploadDicomInfo(
                filename,
                InstitutionName,
                PatientAge,
                PatientName,
                PatientSex,
                SeriesInstanceUID,
                StudyInstanceUID,
                acquisitionDate,
                acquisitionTime,
                UploadTimestamp,
                "待上传",
                delegate
                {
                    _uploadStatus = UploadStatus.已上传dicom信息并置于待上传文件;
                    StartFrom已上传dicom信息并置于待上传文件();
                },
                delegate (string updateDicomInofFailureReason)
                {
                    //CancelOrderWorker.SendCancelOrderRequeset(
                    //    MESPDownloadUpload.OrderId,
                    //    successCallback: delegate (Models.CancelOrderResponse response)
                    //    {
                    //    },
                    //    failureCallback: delegate (string cancelOrderFailureReason)
                    //    {
                    //    },
                    //    errorCallback: delegate (Exception ex)
                    //    {
                    //    });
                    _uploadStatus = UploadStatus.上传dicom信息并置于待上传文件失败;
                    failureCallback(updateDicomInofFailureReason);
                },
                delegate (Exception ex)
                {
                    //CancelOrderWorker.SendCancelOrderRequeset(
                    //    MESPDownloadUpload.OrderId,
                    //    successCallback: delegate (Models.CancelOrderResponse response)
                    //    {
                    //    },
                    //    failureCallback: delegate (string cancelOrderFailureReason)
                    //    {
                    //    },
                    //    errorCallback: delegate (Exception ex1)
                    //    {
                    //    });
                    _uploadStatus = UploadStatus.上传dicom信息并置于待上传文件失败;
                    errorCallback(ex);
                }
                );

        }
        private void StartFrom已上传dicom信息并置于待上传文件()
        {
            Console.WriteLine("StartFrom已上传dicom信息并置于待上传文件");
            UploadFile(
                云数据类型Enum.Dicom数据压缩包,
                filename,
                _uploadStatus == UploadStatus.上传文件失败 ? true : false,
                delegate
                {
                    _uploadStatus = UploadStatus.上传文件完毕;
                    StartFrom上传文件完毕();
                },
                delegate (string uploadFailureReason)
                {
                    _uploadStatus = UploadStatus.上传文件失败;
                    failureCallback(uploadFailureReason);
                },
                delegate (Exception uploadFileEx)
                {
                    _uploadStatus = UploadStatus.上传文件失败;
                    errorCallback(uploadFileEx);
                },
                uploadProgressCallback);

        }
        private void StartFrom上传文件完毕()
        {
            Console.WriteLine("StartFrom上传文件完毕");
            UploadDicomInfo(
                filename,
                InstitutionName,
                PatientAge,
                PatientName,
                PatientSex,
                SeriesInstanceUID,
                StudyInstanceUID,
                acquisitionDate,
                acquisitionTime,
                UploadTimestamp,
                "已上传",
                delegate
                {
                    _uploadStatus = UploadStatus.已上传dicom信息并置于已上传文件;
                    successCallback();
                },
                delegate (string failureReason)
                {
                    //CancelOrderWorker.SendCancelOrderRequeset(
                    //    MESPDownloadUpload.OrderId,
                    //    successCallback: delegate (Models.CancelOrderResponse response)
                    //    {
                    //    },
                    //    failureCallback: delegate (string cancelOrderFailureReason)
                    //    {
                    //    },
                    //    errorCallback: delegate (Exception ex)
                    //    {
                    //    });
                    _uploadStatus = UploadStatus.上传dicom信息并置于已上传文件失败;
                    failureCallback(failureReason);
                },
                delegate (Exception ex)
                {
                    //CancelOrderWorker.SendCancelOrderRequeset(
                    //    MESPDownloadUpload.OrderId,
                    //    successCallback: delegate (Models.CancelOrderResponse response)
                    //    {
                    //    },
                    //    failureCallback: delegate (string cancelOrderFailureReason)
                    //    {
                    //    },
                    //    errorCallback: delegate (Exception iex)
                    //    {
                    //    });
                    _uploadStatus = UploadStatus.上传dicom信息并置于已上传文件失败;
                    errorCallback(ex);
                });
        }

        private void StartFrom已上传dicom信息并置于已上传文件()
        {

        }
    }
}
