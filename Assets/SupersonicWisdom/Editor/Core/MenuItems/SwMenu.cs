using System;
using System.Collections.Generic;
using Facebook.Unity.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SupersonicWisdomSDK.Editor
{
    internal static class SwMenu
    {
        #region --- Public Methods ---

        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/Allow credentials override", false, 403)]
        public static void AllowEditingSettings ()
        {
            SwEditorUtils.AllowEditingSettings = true;
            InternalEditorUtility.RepaintAllViews();
        }

        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/" + SwEditorConstants.UI.Android + "/Apply Backup Rules", false, 201)]
        public static void ApplyAndroidBackupRules ()
        {
            var isSuccess = SwAndroidBackupRules.ApplyAndroidBackupRules();

            if (isSuccess)
            {
                SwEditorAlerts.AlertApplyBackupRules();
            }
            else
            {
                SwEditorAlerts.AlertCannotApplyBackupRules();
            }
        }

        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/" + SwEditorConstants.UI.Android + "/Apply Debuggable Network Config", false, 202)]
        public static void ApplyAndroidNetworkSecurityConfig ()
        {
            var isSuccess = SwAndroidNetworkSecurityConfig.ApplyAndroidNetworkSecurityConfig();

            if (isSuccess)
            {
                SwEditorAlerts.AlertDebuggableNetworkConfigurationAppliedSuccessfully();
            }
            else
            {
                SwEditorAlerts.AlertCannotDebuggableNetworkConfigurationAppliedSuccessfully();
            }
        }

        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/Check for stage updates...", false, 402)]
        public static void CheckStageUpdates ()
        {
            SwStageUpdate.UpdateStageIfNeeded(true);
        }

        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/Edit Settings", false, 101)]
        public static void EditSettings ()
        {
            SwEditorUtils.OpenSettings();
        }

#if SW_STAGE_STAGE2_OR_ABOVE
        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/Open integration guide", false, 403)]
        public static void OpenIntegrationGuide ()
        {
            var integrationGuideUrl = SwEditorUtils.SwAccountData.IntegrationGuideUrl;
            Application.OpenURL(!string.IsNullOrEmpty(integrationGuideUrl) ? integrationGuideUrl : SwEditorConstants.DefaultIntegrationGuideUrl);
        }
#endif

        [MenuItem(SwEditorConstants.UI.SupersonicWisdom + "/" + SwEditorConstants.UI.Android + "/Re-generate Manifest", false, 200)]
        public static void UpdateAndroidManifest ()
        {
            FacebookSettingsEditor.Edit();
            ManifestMod.GenerateManifest();
        }

        #endregion
    }
}