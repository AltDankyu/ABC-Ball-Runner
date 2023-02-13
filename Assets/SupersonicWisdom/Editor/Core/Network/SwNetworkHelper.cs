using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SupersonicWisdomSDK.Editor
{
    internal static class SwNetworkHelper
    {
        #region --- Constants ---

        private const int HttpRequestTimeout = 60;

        #endregion


        #region --- Public Methods ---

        public static async Task<Tuple<string, SwEditorError, HttpResponseMessage>> PerformRequest(string urlString, Dictionary<string, object> jsonDictionary, Dictionary<string, string> headers)
        {
            Tuple<string, SwEditorError, HttpResponseMessage> result = null;
            var didFinish = false;

            PerformRequest(urlString, jsonDictionary, headers, (resultString, error, httpResponseMessage) =>
            {
                result = new Tuple<string, SwEditorError, HttpResponseMessage>(resultString, error, httpResponseMessage);
                didFinish = true;
            });

            while (!didFinish)
            {
                await Task.Delay(300);
            }

            return result ?? new Tuple<string, SwEditorError, HttpResponseMessage>(null, null, null);
        }

        public static void PerformRequest(string urlString, Dictionary<string, object> jsonDictionary, Dictionary<string, string> headers, Action<string, SwEditorError, HttpResponseMessage> callback)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            var client = new HttpClient(handler) { Timeout = new TimeSpan(0, 0, HttpRequestTimeout) };

            headers?.Keys.ToList().ForEach(key => { client.DefaultRequestHeaders.Add(key, headers[key]); });

            Task<HttpResponseMessage> task;

            if ((jsonDictionary?.Count ?? 0) > 0)
            {
                var jsonString = SwJsonParser.Serialize(jsonDictionary);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                task = client.PostAsync(urlString, content);
            }
            else
            {
                task = client.GetAsync(urlString);
            }

            task.ContinueWith((Task sameTask) =>
            {
                string responseString = null;
                var error = new SwEditorError();
                HttpResponseMessage result = null;

                try
                {
                    result = task.Result;

                    if (!task.Result.IsSuccessStatusCode)
                    {
                        error.ErrorMessage = task.Result.ReasonPhrase;

                        if (string.IsNullOrEmpty(error.ErrorMessage))
                        {
                            error.ErrorMessage = task.Result.Content.ReadAsStringAsync().Result;
                        }

                        error.ResponseCode = (int)task.Result.StatusCode;
                    }
                    else
                    {
                        responseString = task.Result.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception e)
                {
                    error.ErrorMessage = "No result";

                    if (string.IsNullOrEmpty(error.ErrorMessage))
                    {
                        error.ErrorMessage = task.Result.Content.ReadAsStringAsync().Result;
                    }

                    error.ResponseCode = 0;
                }

                // This `completed` operation is running on a background thread, in most cases we would like to get a result on the main thread.
                SwEditorUtils.RunOnMainThread(() => { callback.Invoke(responseString, error, result); });
            });
        }

        #endregion


        #region --- Private Methods ---

        internal static async Task<Tuple<string, int>> DownloadFileAsync(Uri uri, string destinationFilePath, Action<float, ulong, Action> downloadProgressionHandler = null)
        {
            var errorCode = 0;
            var didFinish = false;

            SwEditorCoroutines.StartEditorCoroutine(DownloadFile(uri.ToString(), destinationFilePath, (percentage, bytes, cancelAction) =>
            {
                downloadProgressionHandler?.Invoke(percentage, bytes, () =>
                {
                    cancelAction?.Invoke();
                    errorCode = (int)SwErrors.EStageUpdate.DownloadUpdatePackageCanceled;
                    didFinish = true;
                });
            }), callback: () => { didFinish = true; });

            while (!didFinish)
            {
                await Task.Delay(300);
            }

            var resultPath = File.Exists(destinationFilePath) ? destinationFilePath : "";

            return Tuple.Create(resultPath, errorCode);
        }

        private static IEnumerator DownloadFile(string downloadFileUrl, string destinationFilePath, Action<float, ulong, Action> progressionHandler = null, Action<int> onDone = null)
        {
            var errorCode = 0;
            var isCancelled = false;
            var downloadWebClient = new UnityWebRequest(downloadFileUrl) { downloadHandler = new DownloadHandlerFile(destinationFilePath) };
            downloadWebClient.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (downloadWebClient.result != UnityWebRequest.Result.ProtocolError)
#else
            if (!downloadWebClient.isHttpError && !downloadWebClient.isNetworkError)
#endif
            {
                while (!downloadWebClient.isDone && !isCancelled)
                {
                    yield return new WaitForSeconds(0.1f);

                    if (progressionHandler == null) continue;

                    var downloadProgress = downloadWebClient.downloadProgress;
                    var downloadedBytes = downloadWebClient.downloadedBytes;

                    SwEditorUtils.RunOnMainThread(() =>
                    {
                        progressionHandler(downloadProgress, downloadedBytes, () =>
                        {
                            // User clicked "cancel" button
                            if (downloadWebClient != null && !downloadWebClient.isDone)
                            {
                                downloadWebClient.Abort();
                            }

                            isCancelled = true;
                            File.Delete(destinationFilePath);
                        });
                    });
                }
            }
            else
            {
                SwEditorLogger.LogError("Error Downloading from '" + downloadFileUrl + "' : " + downloadWebClient.error);
                File.Delete(destinationFilePath);

                errorCode = (int)SwErrors.EStageUpdate.DownloadUpdatePackageFailed;
            }

            downloadWebClient.Dispose();

            onDone?.Invoke(errorCode);
        }

        #endregion
    }
}