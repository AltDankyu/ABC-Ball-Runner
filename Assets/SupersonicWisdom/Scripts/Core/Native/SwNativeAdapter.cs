using System.Collections;
using UnityEngine;

namespace SupersonicWisdomSDK
{
    internal class SwNativeAdapter : ISwReadyEventListener
    {
        #region --- Constants ---

        private const string EventsRemoteConfigStorageKey = "SupersonicWisdomEventsConfig";

        #endregion


        #region --- Members ---

        protected readonly SwUserData UserData;
        protected bool DidFirstSessionStart;

        protected string AbId = "";
        protected string AbName = "";
        protected string AbVariant = "";
        private readonly ISwSessionListener _sessionListener;
        private readonly ISwSettings _settings;

        private readonly SwNativeApi _wisdomNativeApi;

        #endregion


        #region --- Construction ---

        public SwNativeAdapter(SwNativeApi wisdomNativeApi, ISwSettings settings, SwUserData userData, ISwSessionListener sessionListener)
        {
            _wisdomNativeApi = wisdomNativeApi;
            _settings = settings;
            UserData = userData;
            _sessionListener = sessionListener;
        }

        #endregion


        #region --- Public Methods ---

        public virtual IEnumerator InitNativeSession ()
        {
            _wisdomNativeApi.InitializeSession(GetEventMetadata());

            if (_wisdomNativeApi.IsSupported() && GetEventsConfig().enabled)
            {
                while (!DidFirstSessionStart)
                {
                    yield return null;
                }
            }
        }

        public virtual IEnumerator InitSDK ()
        {
            var eventsConfig = GetEventsConfig();

            if (!eventsConfig.enabled)
            {
                SwInfra.Logger.LogWarning("SwNativeAdapter | InitSDK | " + $"enabled: {eventsConfig.enabled}");

                yield break;
            }

            SwInfra.Logger.Log($"SwNativeAdapter | InitSDK | enabled: {eventsConfig.enabled}");

            yield return _wisdomNativeApi.Init(GetWisdomNativeConfiguration());

            _wisdomNativeApi.AddSessionStartedCallback(OnSessionStarted);
            _wisdomNativeApi.AddSessionEndedCallback(OnSessionEnded);

            UpdateMetadata();
        }

        public void StoreNativeConfig(SwNativeEventsConfig config)
        {
            var jsonConfig = JsonUtility.ToJson(config);

            if (string.IsNullOrEmpty(jsonConfig))
            {
                SwInfra.Logger.Log("SwNativeAdapter.NativeConfig | StoreNativeConfig | config is null");

                return;
            }

            SwInfra.KeyValueStore.SetString(EventsRemoteConfigStorageKey, jsonConfig);
            SwInfra.KeyValueStore.Save();
        }

        public bool ToggleBlockingLoader(bool shouldPresent)
        {
            return _wisdomNativeApi.ToggleBlockingLoader(shouldPresent);
        }

        public void TrackEvent(string eventName, string customsJson, string extraJson)
        {
            SwInfra.Logger.Log($"SwNativeAdapter | TrackEvent | eventName={eventName} | customsJson={customsJson} | extraJson = {extraJson}");
            _wisdomNativeApi.TrackEvent(eventName, customsJson, extraJson);
        }

        public void UpdateAbData(string abId, string abName, string abVariant)
        {
            AbId = abId;
            AbName = abName;
            AbVariant = abVariant;
        }

        public void UpdateConfig ()
        {
            if (GetEventsConfig().enabled)
            {
                _wisdomNativeApi.UpdateWisdomConfiguration(GetWisdomNativeConfiguration());
            }
            else
            {
                _wisdomNativeApi.Destroy();
            }
        }

        public void UpdateMetadata ()
        {
            _wisdomNativeApi.UpdateMetadata(GetEventMetadata());
        }

        public void OnSwReady ()
        {
            // Waiting for readiness for updating appsFlyerId which is available only after appsFlyer init complete
            UpdateMetadata();
        }

        #endregion


        #region --- Private Methods ---

        protected virtual SwNativeEventsConfig GetDefaultConfig ()
        {
            return new SwNativeEventsConfig();
        }

        protected virtual SwEventMetadataDto GetEventMetadata ()
        {
            var attStatus = SwAttUtils.GetStatus();

            var eventMetadata = new SwEventMetadataDto
            {
                bundle = UserData.BundleIdentifier,
                os = UserData.Platform,
                osVer = SystemInfo.operatingSystem,
                uuid = UserData.Uuid,
                swInstallationId = UserData.CustomUuid,
                device = SystemInfo.deviceModel,
                version = Application.version,
                sdkVersion = SwConstants.SdkVersion,
                sdkVersionId = SwConstants.SdkVersionId,
                sdkStage = SwStageUtils.CurrentStage.sdkStage.ToString(),
                installDate = UserData.InstallDate,
                apiKey = _settings.GetAppKey(),
                gameId = _settings.GetGameId(),
                feature = SwConstants.Feature,
                featureVersion = SwConstants.FeatureVersion,
                unityVersion = SwUtils.UnityVersion,
                attStatus = attStatus == SwAttAuthorizationStatus.Unsupported ? "" : $"{attStatus}",
                abId = AbId,
                abName = AbName,
                abVariant = AbVariant
            };

            var organizationAdvertisingId = UserData.OrganizationAdvertisingId;
#if UNITY_IOS
            eventMetadata.sandbox = SwUtils.IsIosSandbox ? "1" : "0";
            eventMetadata.idfv = organizationAdvertisingId;
#endif
#if UNITY_ANDROID
            eventMetadata.appSetId = organizationAdvertisingId;
#endif

            return eventMetadata;
        }

        protected virtual SwNativeEventsConfig GetEventsConfig ()
        {
            var jsonConfig = SwInfra.KeyValueStore.GetString(EventsRemoteConfigStorageKey, null);

            return JsonUtility.FromJson<SwNativeEventsConfig>(jsonConfig) ?? GetDefaultConfig();
        }

        protected virtual string GetSubdomain ()
        {
            var version = Application.version.Replace('.', '-');

            return $"{version}-{_settings.GetGameId()}";
        }

        protected virtual SwNativeConfig GetWisdomNativeConfiguration ()
        {
            return CreateWisdomNativeConfiguration();
        }

        private SwNativeConfig CreateWisdomNativeConfiguration ()
        {
            var config = GetEventsConfig();
            var blockingLoaderResourceRelativePath = SwUtils.IsRunningOnIos() ? "SupersonicWisdom/LoaderFrames" : "SupersonicWisdom/LoaderGif/animated_loader.gif";

            return new SwNativeConfig
            {
                Subdomain = GetSubdomain(),
                ConnectTimeout = config.connectTimeout,
                ReadTimeout = config.readTimeout,
                IsLoggingEnabled = _settings.IsDebugEnabled(),
                InitialSyncInterval = config.initialSyncInterval,
                StreamingAssetsFolderPath = Application.streamingAssetsPath,
                BlockingLoaderResourceRelativePath = blockingLoaderResourceRelativePath,
                BlockingLoaderViewportPercentage = 20
            };
        }

        private void OnSessionEnded(string sessionid)
        {
            _sessionListener.OnSessionEnded(sessionid);
        }

        private void OnSessionStarted(string sessionId)
        {
            _sessionListener.OnSessionStarted(sessionId);

            if (!DidFirstSessionStart)
            {
                DidFirstSessionStart = true;
            }
        }

        #endregion
    }
}