namespace SupersonicWisdomSDK.Editor
{
    internal static class SwEditorConstants
    {
        #region --- Constants ---

        public const string DefaultIntegrationGuideUrl = "https://assets.mobilegamestats.com/docs/supersonic-default-stage-update-integration-guide.pdf";

        #endregion


        #region --- Inner Classes ---

        internal struct GamePlatformKey
        {
            #region --- Constants ---

            internal const string Android = "google_play";
            internal const string Ios = "app_store";
            internal const string IosChina = "app_store_cn";

            #endregion
        }

        internal struct SwKeys
        {
            #region --- Constants ---

            internal const string ImportedStageUpdateInfo = "Sw.ImportedStageUpdateInfo";
            internal const string LastLoginAlertTimestamp = "Sw.LastLoginAlertTimestamp";
            internal const string LastStageUpdateCheckupTimestamp = "Sw.LastStageUpdateCheckupTimestamp";
            internal const string TempAuthToken = "Sw.TempToken"; // Should be saved for a small period
            internal const string UpdatedSdkStageNumber = "Sw.UpdatedSdkStageNumber";

            #endregion
        }

        internal struct UI
        {
            #region --- Constants ---

            public const string Android = "Android";
            public const string AppliedBackupRules = "Applied Backup rules successfully";
            public const string CannotApplyBackupRules = "Cannot apply backup rules.\nOpen the editor console to see failure reason.";
            public const string CannotDebuggableNetworkConfigurationAppliedSuccessfully = "Cannot apply debuggable network configuration.\n Open editor console to see the failure reason.";
            public const string CantCheckUpdates = "Can't check updates";
            public const string ChooseTitleManually = "Please choose your game from the drop down list to retrieve the credential IDs.";
            public const string ConfigurationIsUpToDate = "Configuration is up to date!";
            public const string ConfigurationSuccessfullyCopied = "Configuration successfully copied to settings";
            public const string DebuggableNetworkConfigurationAppliedSuccessfully = "Debuggable network configuration applied successfully";
            public const string DuplicateItems = "Looks like game platforms list has a duplicated item, actual list is: {0}";
            public const string DuplicateProduct = "There is a duplication of \"{0} \" in the \" Products\" list.\nPlease remove the duplication.";
            public const string FailedToCheckCurrentStage = "Failed to check current stage. Error: {0}";
            public const string FailedToDownloadUnityPackage = "Failed to download Unity package";
            public const string FailedToGenerateRemoteUrl = "Unexpected error: remote URL failed to generate.";
            public const string FailedToImportUnityPackage = "Failed to import Unity package";
            public const string InvalidEmailAddress = "That's not a valid email address";

            public const string LoginExpired = "Login expired";
            public const string MissingPassword = "Missing password";
            public const string MissingUnityIap = "Missing Unity IAP Package";
            public const string NewCredentialsIdsInSettings = "New credential IDs for {0} were populated to Wisdom SDK";
            public const string NoNeedToUpdate = "No updates available";
            public const string ParamIsMissing = "The following param: \"{0}\" is missing.\nPlease add it.";
            public const string PleaseChooseTitle = "Please choose title";
            public const string ReachTechnicalSupport = "Please contact technical support support@supersonic.com or try again";
            public const string StageUpdatedSuccessfully = "SupersonicWisdom updated stage successfully";
            public const string StageUpdateFailedDueToCompilation = "Stage update failed due to compilation!";
            public const string StageUpdateUnexpectedError = "An unexpected error, failed with verifying versions and stage number.";
            public const string SupersonicWisdom = "Supersonic Wisdom";
            public const string SupersonicWisdomSDK = "Supersonic Wisdom SDK";
            public const string UpdateInProgress = "An update is currently still in progress, wait until the current update is done before retry.";
            public const string VerifyGASettings = "The following GameAnalytics Advanced Settings should be set to true:\nSend Version\nSubmit Errors\n";
            public const string WelcomeMessage = "Please go to settings window and Login with your Supersonic platform credentials to automatically retrieve the relevant credential IDs";
            public const string WelcomeTitle = "Welcome to Supersonic Wisdom SDK!";

            #endregion


            #region --- Members ---

            public static readonly string SupersonicWisdomSDKError = $"{SupersonicWisdomSDK}: Error";
            public static readonly string SupersonicWisdomSDKWarning = $"{SupersonicWisdomSDK}: Warning";

            #endregion


            #region --- Public Methods ---

            public static string DownloadingStageUpdate(string downloadedSize, string percentagesDownloaded)
            {
                return $"Please do not compile your code during this process ({downloadedSize}, {percentagesDownloaded}%)";
            }

            #endregion


            #region --- Inner Classes ---

            internal struct ButtonTitle
            {
                #region --- Constants ---

                public const string Awesome = "Awesome!";
                public const string Cancel = "Cancel";
                public const string Close = "Close";
                public const string GoToSettings = "Go to Settings";
                public const string LoginNow = "Login now";
                public const string Ok = "OK";
                public const string SetToTrueAndSave = "Set to true & Save";
                public const string Thanks = "Thanks";

                #endregion
            }

            #endregion
        }

        #endregion
    }

    public static class SwErrors
    {
        #region --- Enums ---

        public enum EMenu
        {
            CannotApplyDebuggableNetworkConfiguration = 130,
            CannotApplyBackupRules = 131
        }

        public enum ESettings
        {
            InvalidEmail = 100,
            MissingPassword = 101,
            LoginEndpoint = 102,
            LoginExpired = 103,
            MissingTitles = 104,
            DuplicatePlatform = 105,
            ChooseTitle = 106
        }

        public enum EStageUpdate
        {
            ImportFailed = 140,
            VerifyStageNumber = 141,
            CheckCurrentStage = 142,
            DownloadUpdatePackageFailed = 143,
            CompilationInterference = 144,
            UpdateCheckRequestFailed = 354,
            InvalidUnityPackageRemoteUrl = 355,
            DownloadUpdatePackageCanceled = 356
        }

        #endregion
    }
}