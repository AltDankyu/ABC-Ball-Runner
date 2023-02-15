#if SW_STAGE_STAGE1_OR_ABOVE
using UnityEditor;

namespace SupersonicWisdomSDK.Editor
{
    internal class SwStage1Postprocessor : AssetPostprocessor
    {
        #region --- Public Methods ---

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (GameAnalyticsSDK.GameAnalytics.SettingsGA != null && SwEditorUtils.SwSettings != null && SwEditorUtils.SwSettings.wasLoggedIn)
            {
                SwGameAnalyticsUtils.VerifyMandatoryFlags();
            }
        }

        #endregion
    }
}
#endif