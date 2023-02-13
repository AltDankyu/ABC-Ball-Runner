using UnityEditor;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    internal abstract class SwCoreSettingsTab
    {
        #region --- Constants ---

        internal const int LabelWidth = 180;
        internal const int SpaceBetweenFields = 5;

        #endregion


        #region --- Members ---

        public ISwRepaintDelegate RepaintDelegate = null;
        protected readonly SerializedObject SoSettings;

        #endregion


        #region --- Properties ---

        protected SwSettings Settings
        {
            get { return SwEditorUtils.SwSettings; }
        }

        #endregion


        #region --- Construction ---

        internal SwCoreSettingsTab(SerializedObject soSettings)
        {
            SoSettings = soSettings;
        }

        #endregion


        #region --- Private Methods ---

        protected static void DrawHorizontalLine ()
        {
            GUILayout.Space(15);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            GUILayout.Space(15);
        }

        protected static string TextFieldWithoutSpaces(string actualValue, string label = "", bool alwaysEnable = false)
        {
            var previousGUIEnabled = GUI.enabled;
            GUI.enabled = alwaysEnable || SwEditorUtils.AllowEditingSettings;
            var fieldValue = EditorGUILayout.TextField(label, actualValue?.SwRemoveSpaces() ?? "").SwRemoveSpaces();
            GUI.enabled = previousGUIEnabled;

            return fieldValue;
        }

        protected internal abstract void DrawContent ();

        protected internal abstract string Name ();

        #endregion
    }
}