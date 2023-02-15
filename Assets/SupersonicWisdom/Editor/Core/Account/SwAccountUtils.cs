using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    internal static class SwAccountUtils
    {
        #region --- Constants ---

        private const string DUMMY_SELECT_TITLE_NAME = "--- select ---";

        #endregion


        #region --- Events ---

        public static event Action FetchTitlesCompletedEvent;

        #endregion


        #region --- Members ---

        private static bool _isUpdatingGamesList;

        #endregion


        #region --- Properties ---

        public static bool IsLoggedIn
        {
            get { return !string.IsNullOrEmpty(AccountToken); }
        }

        public static bool IsSelectedTitleDummy
        {
            get { return SelectedTitle.id == null; }
        }

        public static bool WasLoggedIn
        {
            get { return Settings.wasLoggedIn; }
        }

        public static TitleDetails SelectedTitle
        {
            get { return TitlesList != null && Settings != null && TitlesList.Count > Settings.selectedGameIndex ? TitlesList[Settings.selectedGameIndex] : null; }
        }

        public static List<TitleDetails> TitlesList
        {
            get { return SwEditorUtils.SwAccountData.TitleDetailsList; }
            private set { SwEditorUtils.SwAccountData.TitleDetailsList = value; }
        }

        public static string AccountToken { get; private set; }

        private static bool IsTitleListEmpty
        {
            get { return TitlesList?.Count == 0; }
        }

        private static Dictionary<string, GamePlatform> SelectedGameDictionary
        {
            get { return SelectedGame.ToDictionary(e => e.platform); }
        }

        private static List<GamePlatform> SelectedGame
        {
            get { return SelectedTitle?.games; }
        }

        private static SwSettings Settings
        {
            get { return SwEditorUtils.SwSettings; }
        }

        #endregion


        #region --- Public Methods ---

        public static void FetchTitles ()
        {
            if (!IsLoggedIn || _isUpdatingGamesList) return;

            _isUpdatingGamesList = true;
            TitlesList = new List<TitleDetails>();

            SwNetworkHelper.PerformRequest(SwPlatformCommunication.URLs.Titles, null, SwPlatformCommunication.CreateAuthorizationHeadersDictionary(), (value, error, httpResponseMessage) =>
            {
                if (error.IsValid)
                {
                    SwEditorLogger.LogError(error.ErrorMessage);

                    if (IsLoggedIn && error.ResponseCode == 401 && (error.ErrorMessage?.ToLower().Contains("unauthorized") ?? false))
                    {
                        SetAccountToken(null);
                        SwEditorAlerts.AlertLoginExpired();
                    }
                    else
                    {
                        SwEditorAlerts.AlertError(error.ErrorMessage, error.ResponseCode, SwEditorConstants.UI.ButtonTitle.Cancel);
                    }

                    return;
                }

                var jsonString = "{}";
                if (value != null) jsonString = value;

                var titlesResponse = JsonUtility.FromJson<TitlesResponse>(jsonString);

                TitlesList = titlesResponse.result;
                _isUpdatingGamesList = false;
                Settings.selectedGameIndex = 0;
                SaveSelectedTitleToSettings();
                FetchTitlesCompletedEvent?.Invoke();
            });
        }

        public static void GoToLoginTab ()
        {
            SwCoreSettingsInspector.SelectedTabIndex = 0;
            SwEditorUtils.OpenSettings();
        }

        public static void Login(string email, string password)
        {
            SwNetworkHelper.PerformRequest(SwPlatformCommunication.URLs.Login, new Dictionary<string, object>
            {
                { "email", email },
                { "password", password }
            }, null, (responseString, error, httpResponseMessage) =>
            {
                var loginObject = JsonUtility.FromJson<AccountLogin>(responseString);
                SetAccountToken(loginObject?.token ?? "");
                string errorMessage = null;

                if (error.IsValid)
                {
                    errorMessage = error.ErrorMessage;
                }
                else if (!error.IsValid && !IsLoggedIn)
                {
                    errorMessage = "Unknown error";
                }

                if (!IsLoggedIn)
                {
                    SwEditorAlerts.AlertError(errorMessage, (int)SwErrors.ESettings.LoginEndpoint, SwEditorConstants.UI.ButtonTitle.Close);
                }
                else
                {
                    Settings.wasLoggedIn = true;
                    FetchTitles();
                }
            });
        }

        public static void Logout(SwSettings settings)
        {
            settings.selectedGameIndex = 0;
            SetAccountToken(string.Empty);
            TitlesList.Clear();
            _isUpdatingGamesList = false;
        }

        public static void SaveSelectedTitleGamesToSettings ()
        {
            // Select the facebook app id from the first platform where available 
            var gamePlatformWithFacebookAppId = SelectedGameDictionary.Values.FirstOr(item => !string.IsNullOrWhiteSpace(item.facebookAppId), null);

            SwEditorUtils.FacebookAppId = gamePlatformWithFacebookAppId?.facebookAppId ?? "";

            GamePlatform gamePlatform = null;

            gamePlatform = SelectedGameDictionary.SwSafelyGet(SwEditorConstants.GamePlatformKey.Android, null);

            if (gamePlatform != null)
            {
                Settings.androidGameId = gamePlatform.id.SwRemoveSpaces();
                Settings.adMobAndroidAppId = gamePlatform.adMobId.SwRemoveSpaces();
                Settings.ironSourceAndroidAppKey = gamePlatform.isAppKey.SwRemoveSpaces();
                Settings.androidGameAnalyticsGameKey = gamePlatform.gameAnalyticsKey;
                Settings.androidGameAnalyticsSecretKey = gamePlatform.gameAnalyticsSecret;
            }
            else
            {
                Settings.androidGameId = Settings.adMobAndroidAppId = Settings.ironSourceAndroidAppKey = string.Empty;
            }

            gamePlatform = SelectedGameDictionary.SwSafelyGet(SwEditorConstants.GamePlatformKey.Ios, null);

            if (gamePlatform != null)
            {
                Settings.iosGameId = gamePlatform.id.SwRemoveSpaces();
                Settings.iosAppId = gamePlatform.storeId.Replace("id", "").SwRemoveSpaces();
                Settings.iosGameAnalyticsGameKey = gamePlatform.gameAnalyticsKey.SwRemoveSpaces();
                Settings.iosGameAnalyticsSecretKey = gamePlatform.gameAnalyticsSecret.SwRemoveSpaces();
                Settings.adMobIosAppId = gamePlatform.adMobId.SwRemoveSpaces();
                Settings.ironSourceIosAppKey = gamePlatform.isAppKey.SwRemoveSpaces();
            }
            else
            {
                Settings.iosGameId = Settings.iosAppId = Settings.adMobIosAppId = Settings.ironSourceIosAppKey = Settings.iosGameAnalyticsGameKey = Settings.iosGameAnalyticsSecretKey = string.Empty;
            }

            gamePlatform = SelectedGameDictionary.SwSafelyGet(SwEditorConstants.GamePlatformKey.IosChina, null);

            if (gamePlatform != null)
            {
                Settings.iosChinaGameId = gamePlatform.id.SwRemoveSpaces();
                Settings.iosChinaAppId = gamePlatform.storeId.Replace("id", "").SwRemoveSpaces();
                Settings.adMobIosChinaAppId = gamePlatform.adMobId.SwRemoveSpaces();
                Settings.ironSourceIosChinaAppKey = gamePlatform.isAppKey.SwRemoveSpaces();
                Settings.iosChinaGameAnalyticsGameKey = gamePlatform.gameAnalyticsKey.SwRemoveSpaces();
                Settings.iosChinaGameAnalyticsSecretKey = gamePlatform.gameAnalyticsSecret.SwRemoveSpaces();
            }
            else
            {
                Settings.iosChinaGameId = Settings.iosChinaAppId = Settings.adMobIosChinaAppId = Settings.ironSourceIosChinaAppKey = Settings.iosChinaGameAnalyticsGameKey = Settings.iosChinaGameAnalyticsSecretKey = string.Empty;
            }
        }

        public static void TryToRestoreLoginToken ()
        {
            if (IsLoggedIn) return;
            var restoredToken = EditorPrefs.GetString(SwEditorConstants.SwKeys.TempAuthToken, null);

            if (string.IsNullOrEmpty(restoredToken)) return;

            SetAccountToken(restoredToken);

            if (IsLoggedIn)
            {
                EditorPrefs.DeleteKey(SwEditorConstants.SwKeys.TempAuthToken);
            }
        }

        #endregion


        #region --- Private Methods ---

        private static void AddDummyTitle(this List<TitleDetails> titlesList)
        {
            titlesList.Insert(0, new TitleDetails { name = DUMMY_SELECT_TITLE_NAME });
        }

        private static bool IsSettingsAndTitleAreTheSame(TitleDetails titleDetails, SwSettings settings)
        {
            if (titleDetails?.games == null || titleDetails.games.Count == 0) return false;

            foreach (var gamePlatform in titleDetails.games)
            {
                if (SwEditorUtils.FacebookAppId != gamePlatform.facebookAppId) return false;

                switch (gamePlatform.os)
                {
                    case "ios":
                    {
                        var notEqual = settings.iosGameId != gamePlatform.id.SwRemoveSpaces() || settings.iosAppId != gamePlatform.storeId.Replace("id", "").SwRemoveSpaces() || settings.iosGameAnalyticsGameKey != gamePlatform.gameAnalyticsKey.SwRemoveSpaces() || settings.iosGameAnalyticsSecretKey != gamePlatform.gameAnalyticsSecret.SwRemoveSpaces() || settings.adMobIosAppId != gamePlatform.adMobId.SwRemoveSpaces() || settings.ironSourceIosAppKey != gamePlatform.isAppKey.SwRemoveSpaces();

                        if (notEqual) return false;

                        break;
                    }
                    case "iosChina":
                    {
                        var notEqual = settings.iosChinaGameId != gamePlatform.id.SwRemoveSpaces() || settings.iosChinaAppId != gamePlatform.storeId.Replace("id", "").SwRemoveSpaces() || settings.adMobIosChinaAppId != gamePlatform.adMobId.SwRemoveSpaces() || settings.ironSourceIosChinaAppKey != gamePlatform.isAppKey.SwRemoveSpaces() || settings.iosChinaGameAnalyticsGameKey != gamePlatform.gameAnalyticsKey.SwRemoveSpaces() || settings.iosChinaGameAnalyticsSecretKey != gamePlatform.gameAnalyticsSecret.SwRemoveSpaces();

                        if (notEqual) return false;

                        break;
                    }
                    case "android":
                    {
                        var notEqual = settings.androidGameId != gamePlatform.id.SwRemoveSpaces() || settings.androidGameAnalyticsGameKey != gamePlatform.gameAnalyticsKey.SwRemoveSpaces() || settings.androidGameAnalyticsSecretKey != gamePlatform.gameAnalyticsSecret.SwRemoveSpaces() || settings.adMobAndroidAppId != gamePlatform.adMobId.SwRemoveSpaces() || settings.ironSourceAndroidAppKey != gamePlatform.isAppKey.SwRemoveSpaces();

                        if (notEqual) return false;

                        break;
                    }
                }
            }

            return true;
        }

        private static bool IsTitleFoundAutomatically(List<TitleDetails> titles)
        {
            var index = SearchTitleInProjectSettings(titles);
            Settings.isTitleSelectedAutomatically = index > 0;
            Settings.selectedGameIndex = index;

            return Settings.isTitleSelectedAutomatically;
        }

        private static bool IsValidTitle(TitleDetails titleDetails)
        {
            return titleDetails.id != null;
        }

        private static void SaveSelectedTitleToSettings ()
        {
            if (IsTitleListEmpty)
            {
                SwEditorAlerts.AlertMissingTitles();

                return;
            }
            
            TitlesList.AddDummyTitle();

            if (!IsTitleFoundAutomatically(TitlesList))
            {
                InternalEditorUtility.RepaintAllViews();
                SwEditorAlerts.AlertTitleNotFound(TitlesList);

                return;
            }

            if (IsSettingsAndTitleAreTheSame(SelectedTitle, Settings))
            {
                InternalEditorUtility.RepaintAllViews();
                SwEditorAlerts.AlertConfigurationIsUpToDate();

                return;
            }

            var gamePlatformsDictionary = SelectedGameDictionary;

            if (SelectedGame.Count != gamePlatformsDictionary.Count)
            {
                SwEditorAlerts.AlertDuplicatePlatform(SelectedGame);
            }
            else
            {
                SaveSelectedTitleGamesToSettings();
                SwEditorAlerts.AlertSuccess(SelectedTitle.name);
            }
        }

        private static int SearchTitleInProjectSettings(List<TitleDetails> titles)
        {
            if (titles == null) return 0;

            var androidApplicationPackageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            var iosApplicationBundleIdentifier = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);

            // starting after the dummy title
            for (var titleIndex = 1; titleIndex < titles?.Count; titleIndex++)
            {
                var titleDetails = titles[titleIndex];

                if (!IsValidTitle(titleDetails)) continue;

                for (var gameIndex = 0; gameIndex < titleDetails.games.Count; gameIndex++)
                {
                    var platformGame = titleDetails.games[gameIndex];

                    switch (platformGame.os.ToLower())
                    {
                        case "android":
                        {
                            if (platformGame.bundleId == androidApplicationPackageName)
                            {
                                return titleIndex;
                            }

                            break;
                        }
                        case "ios":
                        {
                            if (platformGame.bundleId == iosApplicationBundleIdentifier)
                            {
                                return titleIndex;
                            }

                            break;
                        }
                    }
                }
            }

            return 0;
        }

        private static void SetAccountToken(string jwtToOverride)
        {
            AccountToken = jwtToOverride;

            if (string.IsNullOrEmpty(jwtToOverride))
            {
                EditorPrefs.DeleteKey(SwEditorConstants.SwKeys.TempAuthToken);
            }
        }

        #endregion
    }
}