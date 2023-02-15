using System.Linq;
using UnityEditor;
using UnityEngine;
using static System.String;

namespace SupersonicWisdomSDK.Editor
{
    internal class SwGeneralCoreSettingsTab : SwCoreSettingsTab
    {
        #region --- Constants ---

        private const string _accountEmailLabel = "Email";
        private const string _accountLoginLabel = "Login";
        private const string _accountLogoutLabel = "Logout";
        private const string _accountPasswordLabel = "Password";
        private const string _accountRefreshGameListLabel = "Refresh Game list";
        private const string _accountRetrieveConfiguration = "Retrieve configuration";
        private const string _accountStatusLabel = "Status";
        private const string _accountStatusText = "Logged in";

        private const string _accountTitleLabel = "Account";

        #endregion


        #region --- Members ---

        private static string _accountPassword;

        #endregion


        #region --- Construction ---

        public SwGeneralCoreSettingsTab(SerializedObject soSettings) : base(soSettings)
        { }

        #endregion


        #region --- Private Methods ---

        protected internal override void DrawContent ()
        {
            GUILayout.Space(SpaceBetweenFields * 5);
            DrawAccountFields();
            GUILayout.Space(SpaceBetweenFields * 2);
        }

        protected internal override string Name ()
        {
            return "General";
        }

        protected virtual void SaveToSettings ()
        {
            if (SwAccountUtils.IsSelectedTitleDummy)
            {
                SwEditorAlerts.AlertError(SwEditorConstants.UI.PleaseChooseTitle, (int)SwErrors.ESettings.ChooseTitle, SwEditorConstants.UI.ButtonTitle.Ok);

                return;
            }

            SwAccountUtils.SaveSelectedTitleGamesToSettings();
            SwEditorAlerts.TitleSavedToSettingsSuccess();
        }

        private void DrawAccountFields ()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(_accountTitleLabel, EditorStyles.largeLabel);
            GUILayout.EndHorizontal();

            if (SwAccountUtils.IsLoggedIn)
            {
                DrawAccountTools();
            }
            else
            {
                DrawAccountLogin();
            }
        }

        private void DrawAccountLogin ()
        {
            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_accountEmailLabel, GUILayout.Width(LabelWidth));
            Settings.accountEmail = TextFieldWithoutSpaces(Settings.accountEmail, alwaysEnable: true);
            GUILayout.EndHorizontal();

            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_accountPasswordLabel, GUILayout.Width(LabelWidth));
            _accountPassword = EditorGUILayout.PasswordField("", _accountPassword);
            GUILayout.EndHorizontal();

            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(_accountLoginLabel, GUILayout.Width(80)))
            {
                if (IsNullOrEmpty(_accountPassword))
                {
                    SwEditorAlerts.AlertError(SwEditorConstants.UI.MissingPassword, (int)SwErrors.ESettings.MissingPassword, SwEditorConstants.UI.ButtonTitle.Close);

                    return;
                }

                if (!Settings.accountEmail.SwIsValidEmailAddress())
                {
                    SwEditorAlerts.AlertError(SwEditorConstants.UI.InvalidEmailAddress, (int)SwErrors.ESettings.InvalidEmail, SwEditorConstants.UI.ButtonTitle.Close);

                    return;
                }

                SwAccountUtils.Login(Settings.accountEmail, _accountPassword);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(SpaceBetweenFields * 2);

            DrawHorizontalLine();
        }

        private void DrawAccountTools ()
        {
            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_accountEmailLabel, GUILayout.Width(LabelWidth));
            GUILayout.Label(Settings.accountEmail);
            GUILayout.EndHorizontal();

            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_accountStatusLabel, GUILayout.Width(LabelWidth));
            GUILayout.Label(_accountStatusText);
            GUILayout.EndHorizontal();

            GUILayout.Space(SpaceBetweenFields * 3);
            GUILayout.BeginHorizontal();
            var isTitleSelectedAutomatically = Settings.isTitleSelectedAutomatically;

            if (isTitleSelectedAutomatically)
            {
                GUI.enabled = false;
            }

            var names = SwAccountUtils.TitlesList?.Select(game => game.name).ToArray();
            Settings.selectedGameIndex = EditorGUILayout.Popup(Settings.selectedGameIndex, names, GUILayout.Width(LabelWidth));

            if (isTitleSelectedAutomatically)
            {
                GUI.enabled = true;

                if (GUILayout.Button(_accountRetrieveConfiguration, GUILayout.Width(LabelWidth)))
                {
                    SwAccountUtils.FetchTitles();
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                if (GUILayout.Button(_accountRetrieveConfiguration, GUILayout.Width(LabelWidth)))
                {
                    SaveToSettings();
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(_accountRefreshGameListLabel, GUILayout.Width(LabelWidth)))
                {
                    SwAccountUtils.FetchTitles();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(SpaceBetweenFields);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(_accountLogoutLabel, GUILayout.Width(80)))
            {
                _accountPassword = Empty;

                SwAccountUtils.Logout(Settings);
                DrawContent();
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(SpaceBetweenFields * 2);

            DrawHorizontalLine();
        }

        private void OnLoginFail(SwEditorError error)
        {
            SwEditorAlerts.AlertError(error.ErrorMessage, (int)SwErrors.ESettings.LoginEndpoint, SwEditorConstants.UI.ButtonTitle.Close);
        }

        #endregion
    }
}