#if SW_STAGE_STAGE1_OR_ABOVE
using System;
using UnityEditor;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    internal class SwAndroidStage1SettingsTab : SwAndroidCoreSettingsTab
    {
        #region --- Members ---

        private readonly GUIContent _gameAnalyticsAndroidGameKeyLabel = new GUIContent("GameAnalytics Game Key", "Your GameAnalytics Android Game Key");
        private readonly GUIContent _gameAnalyticsAndroidSecretKeyLabel = new GUIContent("GameAnalytics Secret Key", "Your GameAnalytics Android Secret Key");

        #endregion


        #region --- Construction ---

        public SwAndroidStage1SettingsTab(SerializedObject soSettings) : base(soSettings)
        { }

        #endregion


        #region --- Private Methods ---

        protected internal override void DrawContent ()
        {
            base.DrawContent();

            DrawGameAnalytics();
        }

        private void DrawGameAnalytics ()
        {
            //Draw GA Android Game Key
            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();
            var (androidGameKey, androidSecretKey) = SwStage1EditorUtils.GetGameAnalyticsKeys(RuntimePlatform.Android);
            GUILayout.Label(_gameAnalyticsAndroidGameKeyLabel, GUILayout.Width(LabelWidth));
            var newAndroidGameKey = TextFieldWithoutSpaces(androidGameKey);
            Settings.androidGameAnalyticsGameKey = androidGameKey;
            GUILayout.EndHorizontal();

            //Draw GA Android Secret Key
            GUILayout.Space(SpaceBetweenFields);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_gameAnalyticsAndroidSecretKeyLabel, GUILayout.Width(LabelWidth));
            var newAndroidSecretKey = TextFieldWithoutSpaces(androidSecretKey);
            Settings.androidGameAnalyticsSecretKey = newAndroidSecretKey;
            GUILayout.EndHorizontal();

            if (!newAndroidGameKey.Equals(androidGameKey) || !newAndroidSecretKey.Equals(androidSecretKey))
            {
                SwStage1EditorUtils.SetGameAnalyticsKeys(RuntimePlatform.Android, newAndroidGameKey, newAndroidSecretKey);
            }
        }

        #endregion
    }
}
#endif