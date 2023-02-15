using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Facebook.Unity.Settings;

namespace SupersonicWisdomSDK.Editor
{
    public class SwEditorUtils
    {
        #region --- Constants ---

        private const string IapAssemblyFullName = "UnityEngine.Purchasing";
        private const string SupersonicWisdomResourceDirName = "SupersonicWisdom";
        private const string SupersonicWisdomSettingsAssetResourceFileName = "Settings";

        #endregion


        #region --- Members ---

        private static readonly string SettingsAssetFileRelativePath = $"Resources/{SupersonicWisdomResourceDirName}/{SupersonicWisdomSettingsAssetResourceFileName}.asset";
        private static readonly string AccountDataAssetFileRelativePath = $"{SupersonicWisdomResourceDirName}/Editor/Core/Account/SwAccountData.asset";
        private static SwSettingsManager<SwSettings> _settingsManager;

        private static readonly Lazy<SwMainThreadActionsQueue> LazyMainThreadActions = new Lazy<SwMainThreadActionsQueue>(() =>
        {
            // Register to main thread updates, the "Lazy"'s factory mechanism will ensure that it will occur only once.
            EditorApplication.update += OnEditorApplicationUpdate;

            return new SwMainThreadActionsQueue();
        });

        #endregion


        #region --- Properties ---

        public static string AppName
        {
            get { return string.IsNullOrEmpty(SwSettings.appName) ? Application.productName : SwSettings.appName; }
        }

        public static SwSettings SwSettings
        {
            get
            {
                if (_settingsManager == null)
                {
                    _settingsManager = new SwSettingsManager<SwSettings>(null, $"{SupersonicWisdomResourceDirName}/{SupersonicWisdomSettingsAssetResourceFileName}");
                }

                // Fix unity issue in post build script where Resource.Load doesn't always work on first run
                if (_settingsManager.Settings == null && DoesSettingsAssetFileExist)
                {
                    var settings = AssetDatabase.LoadAssetAtPath($"Assets/{SettingsAssetFileRelativePath}", typeof(SwSettings)) as SwSettings;
                    _settingsManager.Load(settings);
                }

                return _settingsManager.Settings;
            }
        }

        public static bool AllowEditingSettings { get; set; }

        public static string FacebookAppId
        {
            set
            {
                if (FacebookSettings.AppIds.Count == 0)
                {
                    FacebookSettings.AppIds = new List<string> { value };
                    FacebookSettings.AppLabels = new List<string> { AppName };
                }
                else
                {
                    FacebookSettings.AppIds[0] = value;
                    FacebookSettings.AppLabels[0] = AppName;
                }

                EditorUtility.SetDirty(FacebookSettings.Instance);
            }
            get
            {
                if (FacebookSettings.AppIds.Count == 0) return "";

                return FacebookSettings.AppIds[0];
            }
        }

        internal static SwAccountData SwAccountData
        {
            get
            {
                var assetPath = $"Assets/{AccountDataAssetFileRelativePath}";

                var swAccountData = AssetDatabase.LoadAssetAtPath(assetPath, typeof(SwAccountData)) as SwAccountData;

                if (swAccountData == null)
                {
                    var swAccountDataAsset = ScriptableObject.CreateInstance<SwAccountData>();
                    AssetDatabase.CreateAsset(swAccountDataAsset, assetPath);
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();

                    swAccountData = swAccountDataAsset;
                }

                return swAccountData;
            }
        }

        private static bool DoesSettingsAssetFileExist
        {
            get { return File.Exists($"{Application.dataPath}/{SettingsAssetFileRelativePath}"); }
        }

        private static SwMainThreadActionsQueue MainThreadActions
        {
            get { return LazyMainThreadActions.Value; }
        }

        #endregion


        #region --- Public Methods ---

        public static void CallOnNextUpdate(Action action)
        {
            new CallOnInspectorUpdate(action);
        }

        public static async Task<bool> ImportPackage(string packagePath, bool isSilent)
        {
            bool? didImport = null;

            new SwImportPackageCallback(errorMessage =>
            {
                didImport = errorMessage == null;

                if (!didImport.Value)
                {
                    SwEditorLogger.LogError(errorMessage);
                }
            });

            AssetDatabase.ImportPackage(packagePath, !isSilent);

            while (didImport == null)
            {
                await Task.Delay(300);
            }

            return didImport.Value;
        }

        public static void OpenSettings ()
        {
            if (SwSettings == null)
            {
                CreateSettings();
            }

            Selection.activeObject = SwSettings;
        }

        public static float Percentage(float part, float total)
        {
            if (part == 0 || total == 0) return 0;

            return part / total * 100f;
        }

        /// <summary>
        ///     Runs a given action on the main thread (UI thread).
        ///     In general, when we want to present dialogs, progress bars or make operations like import packages, etc. we must
        ///     call it on the main thread. Otherwise the Unity Editor will ignore our request (it won't crash or throw an error,
        ///     but it will ignore).
        ///     Whenever the process returns from a <see cref="T:System.Threading.Tasks.Task" />, it runs on a secondary thread (
        ///     by a pool or custom). Therefore we should use this
        /// </summary>
        /// <param name="actionToRun">The action that will actually run on the main thread.</param>
        /// <param name="afterDelayMilliseconds">
        ///     The delay to wait before executing the action. Default value is zero (run
        ///     immediately)
        /// </param>
        public static void RunOnMainThread(Action actionToRun, int afterDelayMilliseconds = 0)
        {
            if (afterDelayMilliseconds == 0)
            {
                void SafeRun ()
                {
                    try
                    {
                        actionToRun();
                    }
                    catch (Exception mainThreadActionException)
                    {
                        SwEditorLogger.LogError(mainThreadActionException.ToString());
                    }
                }

                var isRunningOnMainThread = !Thread.CurrentThread.IsBackground;

                if (isRunningOnMainThread)
                {
                    SafeRun();
                }
                else
                {
                    MainThreadActions.Add(SafeRun);
                }
            }
            else
            {
                new Task(() =>
                {
                    Thread.Sleep(afterDelayMilliseconds);
                    RunOnMainThread(actionToRun);
                }).Start();
            }
        }

        #endregion


        #region --- Private Methods ---

        internal static bool IsUnityIapAssetInstalled ()
        {
            return DoesAssemblyExists(IapAssemblyFullName);
        }

        internal static void OpenIronSourceIntegrationManager ()
        {
            var ironSourceMenuType = Type.GetType("IronSourceMenu, Assembly-CSharp-Editor");

            try
            {
                ironSourceMenuType.GetMethod("SdkManagerProd", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            }
            catch (Exception)
            {
                Debug.LogError("Could not open IronSource Integration Manager");
            }
        }

        internal static IEnumerator WaitUntilEndOfCompilation ()
        {
            yield return new WaitUntil(() => !EditorApplication.isCompiling);
        }

        private static void CreateSettings ()
        {
            try
            {
                if (_settingsManager.Settings == null)
                {
                    // If the settings asset doesn't exist, then create it. We require a resources folder
                    if (!Directory.Exists($"{Application.dataPath}/Resources"))
                    {
                        Directory.CreateDirectory($"{Application.dataPath}/Resources");
                    }

                    if (!Directory.Exists($"{Application.dataPath}/Resources/{SupersonicWisdomResourceDirName}"))
                    {
                        Directory.CreateDirectory($"{Application.dataPath}/Resources/{SupersonicWisdomResourceDirName}");
                        Debug.Log($"SupersonicWisdom: Resources/{SupersonicWisdomResourceDirName} folder is required to store settings. it was created ");
                    }

                    var assetPath = $"Assets/{SettingsAssetFileRelativePath}";

                    if (File.Exists(assetPath))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                        AssetDatabase.Refresh();
                    }

                    var asset = ScriptableObject.CreateInstance<SwSettings>();
                    AssetDatabase.CreateAsset(asset, assetPath);
                    AssetDatabase.Refresh();

                    AssetDatabase.SaveAssets();
                    Debug.Log("SupersonicWisdom: Settings file didn't exist and was created");
                    Selection.activeObject = asset;

                    _settingsManager.Load();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SupersonicWisdom: Error getting Settings in InitAPI: " + e.Message);
            }
        }

        private static bool DoesAssemblyExists(string assemblyFullName)
        {
            return AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name.Equals(assemblyFullName));
        }

        private static void OnEditorApplicationUpdate ()
        {
            MainThreadActions.Run();
        }

        #endregion
    }

    internal class SwImportPackageCallback
    {
        #region --- Members ---

        private readonly AssetDatabase.ImportPackageCallback _onImportCompleted;
        private readonly AssetDatabase.ImportPackageFailedCallback _onImportFailed;

        #endregion


        #region --- Construction ---

        public SwImportPackageCallback(Action<string> onComplete)
        {
            _onImportCompleted = packageName =>
            {
                AssetDatabase.importPackageCompleted -= _onImportCompleted;
                AssetDatabase.importPackageFailed -= _onImportFailed;

                onComplete?.Invoke(null);
            };

            _onImportFailed = (packageName, errorMessage) =>
            {
                AssetDatabase.importPackageCompleted -= _onImportCompleted;
                AssetDatabase.importPackageFailed -= _onImportFailed;

                onComplete?.Invoke(errorMessage);
            };

            AssetDatabase.importPackageCompleted += _onImportCompleted;
            AssetDatabase.importPackageFailed += _onImportFailed;
        }

        #endregion
    }
}