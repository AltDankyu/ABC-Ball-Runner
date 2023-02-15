using System;
using System.IO;

namespace SupersonicWisdomSDK
{
    public static class SwConstants
    {
        #region --- Constants ---

        public const int DefaultRequestTimeout = 10;
        public const long FeatureVersion = 0;

        public const string AppIconResourceName = "AppIcon";
        public const string BuildNumber = "4806";
        public const string Feature = "";
        public const string GameObjectName = "SupersonicWisdom";
        public const string GitCommit = "c46074a";
        public const string SdkVersion = "7.1.3";
        public const string SettingsResourcePath = "SupersonicWisdom/Settings";
        public const string ExtractedResourcesDirName = "Extracted";
        public const string CrashlyticsDependenciesFilePath = "Firebase/Editor/";
        public const string CrashlyticsDependenciesFileName = "CrashlyticsDependencies.xml";
        public const string FirebaseVersionTextFileName = "FirebaseUnityWrapperVersion";
        public const string IronsourceEditorFolder = "IronSource/Editor/";
        public const string IronsourceAdapterVersionsCacheFilename = "IronSourceAdapterVersions";

        #endregion


        #region --- Members ---

        public static readonly long SdkVersionId = ComputeSdkVersionId();
        public static readonly string AppIconResourcesPath = Path.Combine("Extracted", AppIconResourceName);

        #endregion


        #region --- Private Methods ---

        private static long ComputeSdkVersionId ()
        {
            short major = 0, minor = 0, patch = 0, beta = 0;

            var parts = SdkVersion.Split('.', '-');

            if (parts.Length >= 3)
            {
                major = Convert.ToInt16(parts[0]);
                minor = Convert.ToInt16(parts[1]);
                patch = Convert.ToInt16(parts[2]);
                beta = 99;
            }

            if (parts.Length == 5)
            {
                beta = Convert.ToInt16(parts[4]);
            }

            return (long)(major * 1e6 + minor * 1e4 + patch * 1e2 + beta);
        }

        #endregion
    }
}