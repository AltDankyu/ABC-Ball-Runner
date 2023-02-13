using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    internal class SwStageUpdate
    {
        #region --- Members ---

        private static bool _welcomeMessageShown;

        #endregion


        #region --- Properties ---

        private static string PendingUnityPackagesToImportFolderPath
        {
            get { return (Application.temporaryCachePath + "/sw-pending-unity-packages-to-import").Replace(" ", "-"); }
        }

        private static string TitleId
        {
            get
            {
                var gameId = SwEditorUtils.SwSettings.iosGameId;

                if (string.IsNullOrEmpty(gameId))
                {
                    gameId = SwEditorUtils.SwSettings.androidGameId;
                }

                return gameId;
            }
        }

        private static bool IsDownloadProcessInProgress { get; set; }

        private static bool IsUpdateProcessInProgress { get; set; }

        #endregion


        #region --- Public Methods ---

        public static void OnPreCompilation ()
        {
            EditorPrefs.SetString(SwEditorConstants.SwKeys.TempAuthToken, SwAccountUtils.AccountToken);

            if (!IsDownloadProcessInProgress) return;

            IsUpdateProcessInProgress = false;
            EditorUtility.ClearProgressBar();
            SwStageUpdateWindow.CloseWindow();
            SwEditorAlerts.AlertError(SwEditorConstants.UI.StageUpdateFailedDueToCompilation, (long)SwErrors.EStageUpdate.CompilationInterference, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static void TryShowStageUpdateFinished ()
        {
            var updateInfoJsonString = EditorPrefs.GetString(SwEditorConstants.SwKeys.ImportedStageUpdateInfo, "{}");
            var updateInfoJsonDictionary = updateInfoJsonString.SwToJsonDictionary();
            int.TryParse(updateInfoJsonDictionary.SwSafelyGet(SwEditorConstants.SwKeys.UpdatedSdkStageNumber, "-1").ToString(), out var updatedStageNumber);

            var didUpdateStage = updatedStageNumber == SwStageUtils.CurrentStage.sdkStage;

            if (didUpdateStage)
            {
                SwEditorCoroutines.StartEditorCoroutine(OnPostStageUpdate());
            }
        }

        public static void UpdateStageIfNeeded(bool isInitiatedByUser = false)
        {
            if (SwEditorUtils.SwSettings == null)
            {
                return;
            }

            if (IsUpdateProcessInProgress)
            {
                if (isInitiatedByUser)
                {
                    SwEditorUtils.RunOnMainThread(SwEditorAlerts.AlertUpdateInProgress);
                }

                return;
            }

            var currentTimestampSeconds = DateTime.Now.SwTimestampSeconds();

            if (QuitStageCheckAndAlertIfNeeded(isInitiatedByUser, currentTimestampSeconds)) return;

            EditorPrefs.SetString(SwEditorConstants.SwKeys.LastStageUpdateCheckupTimestamp, currentTimestampSeconds.ToString());
            IsUpdateProcessInProgress = true;

            SwEditorLogger.Log("Checking for updates...");

            FetchCurrentStageRemotely(isInitiatedByUser, async updatedStageNumber =>
            {
                var shouldUpdate = SwStageUtils.CurrentStage.sdkStage < updatedStageNumber;

                if (shouldUpdate)
                {
                    SwEditorLogger.Log("Stage should be updated.");

                    var stageKey = $"{SwStageUtils.CurrentStageNumber}-{updatedStageNumber}";
                    var uiConfigDictionary = await FetchUpdateConfig();
                    uiConfigDictionary = (Dictionary<string, object>)uiConfigDictionary.SwSafelyGet(stageKey, new Dictionary<string, object>());

                    var didSelectUpdate = await ShowUpdateConfirmation(uiConfigDictionary);

                    if (didSelectUpdate)
                    {
                        var didUpdate = await UpdateStage(updatedStageNumber);

                        if (didUpdate)
                        {
                            // Cool, this block will never run as it will be interrupted
                            // and replaced with the new compiled code from the new imported package.
                        }
                        else
                        {
                            SwStageUpdateWindow.CloseWindow();
                        }
                    }
                }
                else
                {
                    SwEditorLogger.Log("Can't / no need to update.");

                    if (isInitiatedByUser && updatedStageNumber >= 0)
                    {
                        SwEditorAlerts.AlertNoUpdateAvailable();
                    }
                }

                IsUpdateProcessInProgress = false;
            });
        }

        #endregion


        #region --- Private Methods ---

        private static async Task<Tuple<string, int>> _downloadStageUpdatePackage(string fromRemoteUrl, string toLocalFilePath)
        {
            var (_, error, httpResponseMessage) = await SwNetworkHelper.PerformRequest(fromRemoteUrl, null, SwPlatformCommunication.CreateAuthorizationHeadersDictionary());

            var fileTempUrl = "";

            if (error.IsValid && 302 == error.ResponseCode)
            {
                // Got a redirect response: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.httpresponseheaders.location?view=net-6.0#system-net-http-headers-httpresponseheaders-location
                fileTempUrl = httpResponseMessage.Headers.Location.ToString();
            }

            if (string.IsNullOrEmpty(fileTempUrl)) return null;

            IsDownloadProcessInProgress = true;

            var (downloadedFilePath, errorCode) = await SwNetworkHelper.DownloadFileAsync(new Uri(fileTempUrl), toLocalFilePath, (percentagesDownloaded, totalBytesDownloaded, onCancel) =>
            {
                if (IsDownloadProcessInProgress)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(SwEditorConstants.UI.SupersonicWisdomSDK, SwEditorConstants.UI.DownloadingStageUpdate(SwUtils.GenerateFileSizeString(totalBytesDownloaded), (percentagesDownloaded * 100).ToString("0")), percentagesDownloaded))
                    {
                        IsDownloadProcessInProgress = false;

                        SwEditorUtils.RunOnMainThread(() =>
                        {
                            onCancel?.Invoke();
                            SwStageUpdateWindow.CloseWindow();
                            EditorUtility.ClearProgressBar();
                        });
                    }
                }
            });

            IsDownloadProcessInProgress = false;

            SwEditorUtils.RunOnMainThread(EditorUtility.ClearProgressBar, 100);

            return Tuple.Create(downloadedFilePath, errorCode);
        }

        private static async Task<bool> _importStageUpdatePackage(string unityPackageFilePath, int updatedStageNumber)
        {
            var didUpdate = false;
            SwEditorLogger.Log($"Importing Unity package file from path: {unityPackageFilePath}");

            try
            {
                EditorPrefs.SetString(SwEditorConstants.SwKeys.TempAuthToken, SwAccountUtils.AccountToken);

                var updateInfoJsonString = new Dictionary<string, object>
                {
                    { SwEditorConstants.SwKeys.UpdatedSdkStageNumber, updatedStageNumber }
                }.SwToJsonString();

                EditorPrefs.SetString(SwEditorConstants.SwKeys.ImportedStageUpdateInfo, updateInfoJsonString);

                didUpdate = await SwEditorUtils.ImportPackage(unityPackageFilePath, true);

                if (!didUpdate)
                {
                    File.Delete(unityPackageFilePath);
                }
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e.ToString());
            }

            return didUpdate;
        }

        private static string CurrentStageUrl ()
        {
            if (string.IsNullOrEmpty(TitleId)) return "";

            var queryString = SwUtils.SerializeToQueryString(new Dictionary<string, object>
            {
                [QueryParamKeys.Id] = TitleId,
                [QueryParamKeys.SdkVersionId] = SwConstants.SdkVersionId.ToString()
            });

            return $"{SwPlatformCommunication.URLs.CurrentStageApi}?{queryString}";
        }

        private static void FetchCurrentStageRemotely(bool isInitiatedByUser, Action<int> callback)
        {
            var currentStageUrl = CurrentStageUrl();

            if (string.IsNullOrEmpty(currentStageUrl))
            {
                callback.Invoke(-1);

                return;
            }

            SwNetworkHelper.PerformRequest(currentStageUrl, null, SwPlatformCommunication.CreateAuthorizationHeadersDictionary(), (responseString, error, httpResponseMessage) =>
            {
                if (error.IsValid)
                {
                    if (isInitiatedByUser)
                    {
                        SwEditorAlerts.AlertFailedToCheckCurrentStage(error.ErrorMessage);
                    }

                    callback.Invoke(-1);

                    return;
                }

                var responseDictionary = SwJsonParser.DeserializeToDictionary(responseString);
                var errorCode = (int)(responseDictionary.SwSafelyGet("errorCode", null) ?? -1);

                if (errorCode >= 0)
                {
                    var errorMessage = responseDictionary.SwSafelyGet("errorMessage", "").ToString();

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        SwEditorLogger.LogError(errorMessage);
                    }

                    callback.Invoke(-1);

                    return;
                }

                var stageNumberString = responseDictionary.SwSafelyGet("stage", "-1").ToString();

                if (!int.TryParse(stageNumberString, NumberStyles.Any, CultureInfo.InvariantCulture, out var stageNumber))
                {
                    stageNumber = -1;
                    SwEditorLogger.LogError("Failed to parse an integer from stage number string: " + stageNumberString);
                }

                callback.Invoke(stageNumber);
            });
        }

        private static async Task<Dictionary<string, object>> FetchUpdateConfig ()
        {
            Dictionary<string, object> uiConfigDictionary = null;

            try
            {
                var (jsonString, error, httpResponseMessage) = await SwNetworkHelper.PerformRequest(SwPlatformCommunication.URLs.UpdateMessagesConfig, null, null);

                if (string.IsNullOrEmpty(jsonString) && error.IsValid)
                {
                    SwEditorLogger.LogError(error.ErrorMessage);
                }
                else
                {
                    uiConfigDictionary = SwJsonParser.DeserializeToDictionary(jsonString);
                }
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            return uiConfigDictionary ?? new Dictionary<string, object>();
        }

        private static string GenerateLocalFilePath(int stageNumber)
        {
            var versionToDownload = SwConstants.SdkVersion;

            if (!Directory.Exists(PendingUnityPackagesToImportFolderPath))
            {
                Directory.CreateDirectory(PendingUnityPackagesToImportFolderPath);
            }

            return PendingUnityPackagesToImportFolderPath + "/SupersonicWisdomSDK_" + versionToDownload + "_stage_" + stageNumber + ".unitypackage";
        }

        private static string GenerateUnityPackageRemoteUrl(int stageToDownload)
        {
            var apiPath = "";

            try
            {
                const string versionToDownload = SwConstants.SdkVersion;
                var fileSuffix = "";

                if (SwStageUtils.MaxStageNumber != stageToDownload)
                {
                    fileSuffix = "_Stage" + stageToDownload;
                }

                var path = versionToDownload + "/SupersonicWisdomSDK_" + versionToDownload + fileSuffix + ".unitypackage";
                apiPath = SwPlatformCommunication.URLs.DownloadWisdomPackage + $"?{QueryParamKeys.Id}={TitleId}&path={path}&{QueryParamKeys.SdkVersionId}={SwConstants.SdkVersionId}";
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            return apiPath;
        }

        private static IEnumerator OnPostStageUpdate ()
        {
            yield return new WaitForSeconds(2);

            try
            {
                Directory.Delete(PendingUnityPackagesToImportFolderPath, true);
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            EditorPrefs.DeleteKey(SwEditorConstants.SwKeys.ImportedStageUpdateInfo);

            if (SwEditorAlerts.AlertUpdateSuccess())
            {
                SwEditorUtils.OpenSettings();
            }
        }

        private static bool QuitStageCheckAndAlertIfNeeded(bool isInitiatedByUser, long currentTimestampSeconds)
        {
            const long totalSecondsInOneHour = 60 * 60;
            const long totalSecondsInOneDay = 24 * totalSecondsInOneHour;


            if (!SwAccountUtils.IsLoggedIn)
            {
                var shouldShowPopup = isInitiatedByUser;

                if (!shouldShowPopup)
                {
                    var lastLoginAlertTimestampString = EditorPrefs.GetString(SwEditorConstants.SwKeys.LastLoginAlertTimestamp, "0");

                    if (!long.TryParse(lastLoginAlertTimestampString, out var lastLoginAlertTimestampSeconds))
                    {
                        lastLoginAlertTimestampSeconds = 0;
                    }

                    EditorPrefs.SetString(SwEditorConstants.SwKeys.LastLoginAlertTimestamp, currentTimestampSeconds.ToString());
                    shouldShowPopup = currentTimestampSeconds - lastLoginAlertTimestampSeconds > totalSecondsInOneDay;
                }

                if (shouldShowPopup)
                {
                    if (SwEditorAlerts.Alert(SwEditorConstants.UI.CantCheckUpdates, SwEditorConstants.UI.ButtonTitle.LoginNow, SwEditorConstants.UI.ButtonTitle.Cancel))
                    {
                        SwAccountUtils.GoToLoginTab();
                    }
                }

                return true;
            }

            if ((SwAccountUtils.TitlesList?.Count ?? 0) == 0) return true;

            var lastCheckupTimestampString = EditorPrefs.GetString(SwEditorConstants.SwKeys.LastStageUpdateCheckupTimestamp, "");
            long.TryParse(lastCheckupTimestampString, out var lastCheckupTimestamp);

            if (!isInitiatedByUser && currentTimestampSeconds - lastCheckupTimestamp < totalSecondsInOneHour) return true;

            return false;
        }

        private static async Task<bool> ShowUpdateConfirmation(Dictionary<string, object> uiConfiguration)
        {
            bool? popupResult = null;

            try
            {
                ShowUpdateConfirmation(uiConfiguration, didUserSelectYes => { popupResult = didUserSelectYes; });

                while (popupResult == null)
                {
                    await Task.Delay(300);
                }
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }

            return popupResult ?? false;
        }

        private static void ShowUpdateConfirmation(Dictionary<string, object> uiConfiguration, Action<bool> onDismiss)
        {
            var integrationGuideUrlKey = "integrationGuideUrl";

            var messageTitle = (string)uiConfiguration?.SwSafelyGet("messageTitle", null);
            var updateButtonTitle = (string)uiConfiguration?.SwSafelyGet("updateButtonTitle", null);
            var updateButtonTip = (string)uiConfiguration?.SwSafelyGet("updateButtonTip", null);
            var messageBody = (string)uiConfiguration?.SwSafelyGet("messageBody", null);
            var integrationGuideButtonTitle = (string)uiConfiguration?.SwSafelyGet("integrationGuideButtonTitle", null);
            var integrationGuideButtonTip = (string)uiConfiguration?.SwSafelyGet("integrationGuideButtonTip", null);
            ;
            var integrationGuideUrl = (string)uiConfiguration?.SwSafelyGet(integrationGuideUrlKey, null);
            SwEditorUtils.SwAccountData.IntegrationGuideUrl = integrationGuideUrl;
            var integrationGuideDescription = (string)uiConfiguration?.SwSafelyGet("integrationGuideDescription", null);

            SwStageUpdateWindow.ShowNew(updateButtonTitle, updateButtonTip, messageTitle, messageBody, integrationGuideButtonTitle, integrationGuideButtonTip, integrationGuideDescription, integrationGuideUrl, onDismiss);
        }

        private static async Task<bool> UpdateStage(int updatedStageNumber)
        {
            var remoteUrl = GenerateUnityPackageRemoteUrl(updatedStageNumber);

            if ((remoteUrl?.Length ?? 0) == 0)
            {
                SwEditorAlerts.AlertError(SwEditorConstants.UI.FailedToGenerateRemoteUrl, (int)SwErrors.EStageUpdate.InvalidUnityPackageRemoteUrl, SwEditorConstants.UI.ButtonTitle.Ok);

                return false;
            }

            var localDestinationFilePath = GenerateLocalFilePath(updatedStageNumber);
            var (downloadedFilePath, errorCode) = await _downloadStageUpdatePackage(remoteUrl, localDestinationFilePath);

            var didUpdate = false;
            EditorUtility.ClearProgressBar();

            if ((downloadedFilePath?.Length ?? 0) > 0)
            {
                Selection.activeObject = null; // Deselecting our settings object in the inspector before import. Motivation: The settings inspector is not refreshed properly in case of stage update while our settings is focused in the inspector.
                didUpdate = await _importStageUpdatePackage(localDestinationFilePath, updatedStageNumber);

                if (!didUpdate)
                {
                    if (SwEditorAlerts.AlertFailedToImportPackage())
                    {
                        SwEditorUtils.OpenSettings();
                    }
                }
            }
            else
            {
                if (errorCode != 0 && errorCode != (int)SwErrors.EStageUpdate.DownloadUpdatePackageCanceled)
                {
                    SwEditorLogger.LogError($"Failed to download stage update Unity package file to path: {localDestinationFilePath}. Error code: {errorCode}");
                    SwEditorAlerts.AlertFailedToDownloadPackage(errorCode);
                }
            }

            return didUpdate;
        }

        #endregion


        #region --- Inner Classes ---

        // Note: These keys are used for two different APIs: 'current-stage', 'download-wisdom-package'
        private static class QueryParamKeys
        {
            #region --- Constants ---

            internal const string Id = "id";
            internal const string SdkVersionId = "sdkVersionId";

            #endregion
        }

        #endregion
    }
}