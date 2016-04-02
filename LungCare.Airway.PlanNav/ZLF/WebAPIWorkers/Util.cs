using LungCare.SupportPlatform.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LungCare.SupportPlatform.WebAPIWorkers
{
    internal class Util
    {
        internal static string Encrypt(string str)
        {
            LungCare.SupportPlatform.RSACryptoService RSACryptoService =
                new LungCare.SupportPlatform.RSACryptoService(null, @"MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCh+w3+MPHYQgfLliDdglN9byynWu8Llu4JM/yd
Q4nUSIA2GVPV7eSyt0iigf5wG8fbLKwv+aLSKojNdWB/VvDlZn5IHTg/hkKINGYU6JyGOPjgpCoh
8s0qw8EAvGBr/q4vMaMruY7nlWKqqmA4ttmWNsB8xGppCdu6KTjeuynRUwIDAQAB");

            return RSACryptoService.Encrypt(str);
        }

        internal static void ShowExceptionMessage(Exception ex,string msg)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(msg))
            {
                sb.AppendLine(msg);
            }

            Exception tmp = ex;
            while (tmp != null)
            {
                sb.AppendLine(tmp.Message);

                tmp = tmp.InnerException;
            }

            MessageBox.Show(sb.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal static void ShowExceptionMessage(Exception ex)
        {
            ShowExceptionMessage(ex, string.Empty);
        }

        internal static void PostAsync<T>(
            object entity2Post,
            string URI,
            Action<T> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback) where T : GeneralWebAPIResponse
        {
            PostAsync<T>(entity2Post, URI, null, successCallback, failureCallback, errorCallback);
        }

        internal static void PostAsync<T>(
            object entity2Post,
            string URI,
            double? httpClientTimeout,
            Action<T> successCallback,
            Action<string> failureCallback,
            Action<Exception> errorCallback) where T:GeneralWebAPIResponse
        {
            using (var client = new HttpClient())
            {
                if (httpClientTimeout.HasValue)
                {
                    client.Timeout = TimeSpan.FromSeconds(httpClientTimeout.Value);
                }
                string serializedProduct = JsonConvert.SerializeObject(entity2Post);
                Console.WriteLine(URI);
                Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + ": " + serializedProduct);

                var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                try
                {
                    client.PostAsync(URI, content).ContinueWith((postTask) =>
                    {
                        try
                        {
                            if (postTask.Status == TaskStatus.Canceled)
                            {
                                if (errorCallback != null)
                                {
                                    errorCallback(new Exception("连接服务器超时。请确认您已连入互联网。"));
                                }
                                //failureCallback("连接服务器超时。请确认您已连入互联网。");
                                return;
                            }

                            HttpStatusCode statusCode = postTask.Result.StatusCode;
                            Console.WriteLine(statusCode + Environment.NewLine);

                            Task<string> taskReadString = postTask.Result.Content.ReadAsStringAsync();
                            taskReadString.Wait();
                            string responseString = taskReadString.Result;

                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff") + ": " + responseString);

                            T response = JsonConvert.DeserializeObject<T>(responseString);

                            if (response.Success)
                            {
                                if (successCallback != null)
                                    successCallback(response);
                            }
                            else
                            {
                                if (failureCallback != null)
                                {
                                    failureCallback(response.ErrorMsg);
                                }
                            }
                        }
                        catch (Exception exProgressResponse)
                        {
                            Console.WriteLine(exProgressResponse.Message);

                            if (errorCallback != null)
                            {
                                errorCallback(exProgressResponse);
                            }
                        }
                    }).Wait();
                }
                catch (Exception exPost)
                {
                    Console.WriteLine(exPost.Message);

                    if (errorCallback != null)
                    {
                        errorCallback(exPost);
                    }
                }
            }
        }
    }
}
