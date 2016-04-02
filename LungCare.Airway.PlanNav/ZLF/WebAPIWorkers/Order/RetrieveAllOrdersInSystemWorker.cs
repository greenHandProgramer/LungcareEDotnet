using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LungCare.SupportPlatform.WebAPIWorkers;

namespace LungCare.SupportPlatform.Models
{
    public class RetrieveAllOrdersInSystemWorker
    {
        private static string URI = @"http://116.11.253.243:11888/lungcare/webapi/lungcare/RetrieveAllOrdersInSystem";

        //public static void SendRetrieveAllOrdersInSystemRequest(
        //    Action<Models.AllRetrieveDataListResponse> successCallback,
        //    Action<string> failureCallback,
        //    Action<Exception> errorCallback)
        //{
        //    Models.AllRetrieveDataListRequest request = new Models.AllRetrieveDataListRequest();
        //    request.Sender = "PC Client";

        //    request.Token = Security.TokenManager.Token;

        //    Util.PostAsync<Models.AllRetrieveDataListResponse>(
        //        request,
        //        URI,
        //        successCallback,
        //        failureCallback,
        //        errorCallback);
        //}
        public static void SendRetrieveDataListRequest(
            Models.RetrieveDataListRequest updateDicomDataRequest,
            Action<Models.RetrieveDataListResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Util.PostAsync<Models.RetrieveDataListResponse>(
                updateDicomDataRequest,
                URI,
                successCallback,
                failureCallback,
                errorCallback);
        }

        public static void SendRetrieveDataListRequest(
            Action<Models.RetrieveDataListResponse> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback)
        {
            Models.RetrieveDataListRequest updateDicomDataRequest = new Models.RetrieveDataListRequest();
            updateDicomDataRequest.Sender = "PC Client";

            updateDicomDataRequest.Token = Security.TokenManager.Token;
            //updateDicomDataRequest.UserId = Security.SessionManager.UserName;

            SendRetrieveDataListRequest(updateDicomDataRequest, successCallback, failureCallback, errorCallback);
        }
    
    }
}
