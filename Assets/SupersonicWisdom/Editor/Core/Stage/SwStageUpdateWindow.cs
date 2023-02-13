using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEditor;

namespace SupersonicWisdomSDK.Editor
{
    public class SwStageUpdateWindow : EditorWindow
    {
        #region --- Constants ---

        private const float WindowWidth = 320;
        private const string AppNamePlaceholder = "{appName}";
        private const string ButtonTitlePlaceholder = "{buttonTitle}";

        #endregion


        #region --- Members ---

        private Action<bool> _onUpdateButtonSelected;
        private GUIContent _downloadButtonContent;
        private GUIContent _integrationGuideButtonContent;
        private string _integrationGuidDescription;
        private string _integrationGuideUrl;
        private string _messageBody;
        private Vector2 _scrollPosition = Vector2.up;

        #endregion


        #region --- Mono Override ---

        private void OnDestroy ()
        {
            var onDismiss = _onUpdateButtonSelected;
            _onUpdateButtonSelected = null;
            onDismiss?.Invoke(false);
        }

        private void OnGUI ()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(SwGameObjectLogo.Logo, GUILayout.Width(64), GUILayout.Height(64));
            GUILayout.BeginVertical();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false, GUILayout.MinWidth(0));
            GUILayout.Label(_messageBody, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            EditorGUILayout.Space();

            GUILayout.Label(_integrationGuidDescription, EditorStyles.wordWrappedMiniLabel, GUILayout.MaxWidth(WindowWidth));

            EditorGUILayout.Space();

            if (_onUpdateButtonSelected == null)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button(_downloadButtonContent))
            {
                var onUpdateSelected = _onUpdateButtonSelected;
                _onUpdateButtonSelected = null;
                onUpdateSelected?.Invoke(true);
            }

            if (_onUpdateButtonSelected == null)
            {
                GUI.enabled = true;
            }

            if (_integrationGuideUrl?.Length > 0)
            {
                if (GUILayout.Button(_integrationGuideButtonContent))
                {
                    Application.OpenURL(_integrationGuideUrl);
                }
            }

            GUILayout.EndVertical();
        }

        #endregion


        #region --- Public Methods ---

        public static void CloseWindow ()
        {
            try
            {
                GetWindow<SwStageUpdateWindow>().Close();
                ;
            }
            catch (Exception e)
            {
                SwEditorLogger.LogError(e);
            }
        }

        public static void ShowNew(string updateButtonTitle, string updateButtonTip, string messageTitle, string messageBody, string integrationGuideButtonTitle, string integrationGuideButtonTip, string integrationGuideDescription, string integrationGuideUrl, Action<bool> onUpdateButtonSelected)
        {
            var appName = SwEditorUtils.AppName;
            var buttonTitle = updateButtonTitle ?? "Upgrade";

            var updateWindow = (SwStageUpdateWindow)GetWindow(typeof(SwStageUpdateWindow), true);

            updateWindow.position = new Rect(150, 150, 400, 200);
            updateWindow.titleContent = new GUIContent((messageTitle ?? "Supersonic Wisdom Upgrade Available").Replace(AppNamePlaceholder, appName).Replace(ButtonTitlePlaceholder, buttonTitle));

            updateWindow._onUpdateButtonSelected = onUpdateButtonSelected;
            updateWindow._downloadButtonContent = new GUIContent(buttonTitle, updateButtonTip ?? "click to start downloading and importing the new package");
            updateWindow._messageBody = (messageBody ?? $"Good News!\nYour game, {AppNamePlaceholder}, has advanced to the next level.\nUpgrade your Wisdom package to add functionalities needed to keep progressing.").Replace(AppNamePlaceholder, appName).Replace(ButtonTitlePlaceholder, buttonTitle);

            updateWindow._integrationGuidDescription = integrationGuideDescription ?? "* While upgrading wisdom package you can view the integration guide.".Replace(AppNamePlaceholder, appName).Replace(ButtonTitlePlaceholder, buttonTitle);

            updateWindow._integrationGuideUrl = integrationGuideUrl ?? SwEditorConstants.DefaultIntegrationGuideUrl;
            updateWindow._integrationGuideButtonContent = new GUIContent(integrationGuideButtonTitle ?? "Integration Guide", integrationGuideButtonTip ?? "Link to Wisdom integration guide");

            updateWindow.Show();
        }

        #endregion
    }
}