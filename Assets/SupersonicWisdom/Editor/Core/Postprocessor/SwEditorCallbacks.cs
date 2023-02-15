using System;
using System.Collections.Generic;
using UnityEditor;

namespace SupersonicWisdomSDK.Editor
{
    public static class SwEditorCallbacks
    {
        #region --- Private Methods ---

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnPostCompilation ()
        {
            RegisterToPreCompilation();
            SwAccountUtils.TryToRestoreLoginToken();
            WelcomeMessageUtils.TryShowWelcomeMessage();
            SwStageUpdate.TryShowStageUpdateFinished();
            SwStageUpdate.UpdateStageIfNeeded();
        }

        private static void RegisterToPreCompilation ()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= OnPreCompilation;
            AssemblyReloadEvents.beforeAssemblyReload += OnPreCompilation;
        }

        #endregion


        #region --- Event Handler ---

        private static void OnPreCompilation ()
        {
            SwStageUpdate.OnPreCompilation();
        }

        #endregion
    }
}