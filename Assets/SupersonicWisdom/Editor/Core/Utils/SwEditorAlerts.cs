using System.Collections.Generic;
using UnityEditor;

namespace SupersonicWisdomSDK.Editor
{
    public static class SwEditorAlerts
    {
        #region --- Public Methods ---

        #region --- Package import ---

        public static void AlertWelcomeMessage ()
        {
            if (EditorUtility.DisplayDialog(SwEditorConstants.UI.WelcomeTitle, SwEditorConstants.UI.WelcomeMessage, SwEditorConstants.UI.ButtonTitle.GoToSettings))
            {
                SwEditorUtils.OpenSettings();
            }
        }

        #endregion

        #endregion


        #region --- General ---

        public static bool Alert(string message, string okText, string cancel = "")
        {
            return EditorUtility.DisplayDialog(SwEditorConstants.UI.SupersonicWisdomSDK, message, okText, cancel);
        }

        public static bool AlertWarning(string message, string okText, string cancel = "")
        {
            return EditorUtility.DisplayDialog(SwEditorConstants.UI.SupersonicWisdomSDKWarning, message, okText, cancel);
        }

        public static bool AlertError(string message, long code, string buttonText, string cancel = "")
        {
            return EditorUtility.DisplayDialog(SwEditorConstants.UI.SupersonicWisdomSDKError, $"{message}\n\nError code: {(int)code}", buttonText, cancel);
        }

        #endregion


        #region --- Settings ---

        public static void AlertMissingTitles ()
        {
            SwEditorUtils.CallOnNextUpdate(() => { AlertError(SwEditorConstants.UI.ReachTechnicalSupport, (int)SwErrors.ESettings.MissingTitles, SwEditorConstants.UI.ButtonTitle.Ok); });
        }

        public static void AlertDuplicatePlatform(List<GamePlatform> gamePlatforms)
        {
            SwEditorLogger.LogWarning(SwEditorConstants.UI.DuplicateItems.Format(gamePlatforms));

            AlertError(SwEditorConstants.UI.DuplicateItems.Format(gamePlatforms), (int)SwErrors.ESettings.DuplicatePlatform, SwEditorConstants.UI.ButtonTitle.Close);
        }

        public static void AlertSuccess(string selectedTitleName)
        {
            SwEditorUtils.CallOnNextUpdate(() => { Alert(SwEditorConstants.UI.NewCredentialsIdsInSettings.Format(selectedTitleName), SwEditorConstants.UI.ButtonTitle.Awesome); });
        }

        public static void AlertTitleNotFound(List<TitleDetails> current)
        {
            SwEditorUtils.CallOnNextUpdate(() =>
            {
                if (Alert(SwEditorConstants.UI.ChooseTitleManually, SwEditorConstants.UI.ButtonTitle.GoToSettings))
                    SwEditorUtils.OpenSettings();
            });
        }

        public static void AlertLoginExpired ()
        {
            if (AlertError(SwEditorConstants.UI.LoginExpired, (int)SwErrors.ESettings.LoginExpired, SwEditorConstants.UI.ButtonTitle.LoginNow, SwEditorConstants.UI.ButtonTitle.Cancel))
            {
                SwAccountUtils.GoToLoginTab();
            }
        }

        public static void AlertConfigurationIsUpToDate ()
        {
            Alert(SwEditorConstants.UI.ConfigurationIsUpToDate, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static void AlertMissingUnityPackage ()
        {
            Alert(SwEditorConstants.UI.MissingUnityIap, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static void TitleSavedToSettingsSuccess ()
        {
            Alert(SwEditorConstants.UI.ConfigurationSuccessfullyCopied, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        #endregion


        #region --- Menu ---

        public static void AlertApplyBackupRules ()
        {
            EditorUtility.DisplayDialog(SwEditorConstants.UI.SupersonicWisdomSDK, SwEditorConstants.UI.AppliedBackupRules, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static void AlertCannotApplyBackupRules ()
        {
            AlertError(SwEditorConstants.UI.CannotApplyBackupRules, (int)SwErrors.EMenu.CannotApplyBackupRules, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static void AlertDebuggableNetworkConfigurationAppliedSuccessfully ()
        {
            Alert(SwEditorConstants.UI.DebuggableNetworkConfigurationAppliedSuccessfully, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static void AlertCannotDebuggableNetworkConfigurationAppliedSuccessfully ()
        {
            AlertError(SwEditorConstants.UI.CannotDebuggableNetworkConfigurationAppliedSuccessfully, (int)SwErrors.EMenu.CannotApplyDebuggableNetworkConfiguration, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        #endregion


        #region --- Stage update ---

        public static void AlertUpdateInProgress ()
        {
            Alert(SwEditorConstants.UI.UpdateInProgress, SwEditorConstants.UI.ButtonTitle.Ok);
        }

        public static bool AlertUpdateSuccess()
        {
            return Alert(SwEditorConstants.UI.StageUpdatedSuccessfully,
                SwEditorConstants.UI.ButtonTitle.Awesome);

        }

        public static void AlertNoUpdateAvailable ()
        {
            Alert(SwEditorConstants.UI.NoNeedToUpdate, SwEditorConstants.UI.ButtonTitle.Thanks);
        }

        public static void AlertStageUpdateFailed ()
        {
            AlertError(SwEditorConstants.UI.StageUpdateUnexpectedError, (int)SwErrors.EStageUpdate.VerifyStageNumber, SwEditorConstants.UI.ButtonTitle.Close);
        }

        public static bool AlertFailedToImportPackage ()
        {
            return AlertError(SwEditorConstants.UI.FailedToImportUnityPackage, (int)SwErrors.EStageUpdate.ImportFailed, SwEditorConstants.UI.ButtonTitle.Close);
        }

        public static void AlertFailedToDownloadPackage(int errorCode)
        {
            AlertError(SwEditorConstants.UI.FailedToDownloadUnityPackage, errorCode, SwEditorConstants.UI.ButtonTitle.Close);
        }

        public static void AlertFailedToCheckCurrentStage(string errorMessage)
        {
            AlertError(SwEditorConstants.UI.FailedToCheckCurrentStage.Format(errorMessage), (int)SwErrors.EStageUpdate.CheckCurrentStage, SwEditorConstants.UI.ButtonTitle.Close);
        }

        #endregion
    }
}