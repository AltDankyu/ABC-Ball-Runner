package com.supersonic.wisdom.library.api;

import android.app.Activity;
import android.app.Application;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Handler;

import com.supersonic.wisdom.library.IdentifiersGetter;
import com.supersonic.wisdom.library.api.listener.IWisdomInitListener;
import com.supersonic.wisdom.library.api.listener.IWisdomSessionListener;
import com.supersonic.wisdom.library.data.framework.events.EventsQueue;
import com.supersonic.wisdom.library.data.framework.local.ConversionDataLocalApi;
import com.supersonic.wisdom.library.data.framework.local.EventsLocalApi;
import com.supersonic.wisdom.library.data.framework.local.storage.api.IWisdomStorage;
import com.supersonic.wisdom.library.data.framework.local.storage.api.WisdomDbHelper;
import com.supersonic.wisdom.library.data.framework.local.storage.core.WisdomEventsStorage;
import com.supersonic.wisdom.library.data.framework.network.core.WisdomNetwork;
import com.supersonic.wisdom.library.data.framework.network.core.WisdomNetworkDispatcher;
import com.supersonic.wisdom.library.data.framework.network.utils.NetworkUtils;
import com.supersonic.wisdom.library.data.framework.watchdog.ApplicationLifecycleService;
import com.supersonic.wisdom.library.data.framework.local.EventMetadataLocalApi;
import com.supersonic.wisdom.library.data.framework.network.api.IWisdomNetwork;
import com.supersonic.wisdom.library.data.repository.ConversionDataRepository;
import com.supersonic.wisdom.library.data.repository.datasource.ConversionDataLocalDataSource;
import com.supersonic.wisdom.library.data.repository.datasource.EventsLocalDataSource;
import com.supersonic.wisdom.library.domain.events.IEventsQueue;
import com.supersonic.wisdom.library.domain.events.reporter.interactor.EventsReporter;
import com.supersonic.wisdom.library.domain.events.IConversionDataRepository;
import com.supersonic.wisdom.library.domain.events.reporter.interactor.IEventsReporter;
import com.supersonic.wisdom.library.domain.events.interactor.ConversionDataManager;
import com.supersonic.wisdom.library.domain.events.interactor.IConversionDataManager;
import com.supersonic.wisdom.library.data.framework.remote.EventsRemoteApi;
import com.supersonic.wisdom.library.data.repository.EventMetadataRepository;
import com.supersonic.wisdom.library.data.repository.EventsRepository;
import com.supersonic.wisdom.library.data.repository.datasource.EventMetadataLocalDataSource;
import com.supersonic.wisdom.library.data.repository.datasource.EventsRemoteDataSource;
import com.supersonic.wisdom.library.domain.events.IEventsRepository;
import com.supersonic.wisdom.library.domain.events.interactor.EventMetadataManager;
import com.supersonic.wisdom.library.domain.events.interactor.IEventMetadataManager;
import com.supersonic.wisdom.library.domain.events.session.interactor.SessionManager;
import com.supersonic.wisdom.library.domain.events.session.interactor.ISessionManager;
import com.supersonic.wisdom.library.domain.mapper.ListStoredEventJsonMapper;
import com.supersonic.wisdom.library.data.framework.local.mapper.StoredEventMapper;
import com.supersonic.wisdom.library.domain.watchdog.BackgroundWatchdog;
import com.supersonic.wisdom.library.api.dto.WisdomConfigurationDto;
import com.supersonic.wisdom.library.domain.watchdog.interactor.ApplicationLifecycleServiceRegistrar;
import com.supersonic.wisdom.library.domain.watchdog.interactor.BackgroundWatchdogRegistrar;
import com.supersonic.wisdom.library.domain.watchdog.interactor.IApplicationLifecycleService;
import com.supersonic.wisdom.library.domain.watchdog.interactor.IBackgroundWatchdog;
import com.supersonic.wisdom.library.ui.BlockingFullScreenLoader;
import com.supersonic.wisdom.library.util.SdkLogger;
import com.supersonic.wisdom.library.util.SwCallback;
import com.supersonic.wisdom.library.util.SwUtils;

import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

class WisdomSDKImpl implements IWisdomSDK {

    private static final String WISDOM_PREFS_NAME = "WisdomEventsPreferences";
    private static final String TAG = WisdomSDKImpl.class.getSimpleName();

    private Application mApplication;
    private IApplicationLifecycleService mAppLifecycleService;
    private IBackgroundWatchdog mBgWatchdog;

    private List<IWisdomInitListener> mInitListeners = new ArrayList<>();

    private BackgroundWatchdogRegistrar mBackgroundWatchdogRegistrar;
    private ApplicationLifecycleServiceRegistrar mAppLifecycleServiceRegistrar;

    private WisdomNetworkDispatcher mDispatcher;
    private IWisdomNetwork mNetwork;

    private EventsRemoteApi mEventsRemoteApi;
    private EventsRemoteDataSource mEventsRemoteDataSource;
    private IEventsRepository mEventsRepository;

    private EventMetadataLocalApi mEventMetadataLocalApi;
    private EventMetadataLocalDataSource mEventMetadataLocalDataSource;
    private EventMetadataRepository mEventMetadataRepository;

    private ConversionDataLocalApi mConversionDataLocalApi;
    private ConversionDataLocalDataSource mConversionDataLocalDataSource;
    private IConversionDataRepository mConversionDataRepository;

    private ISessionManager mSessionManager;
    private IEventMetadataManager mEventMetadataManager;
    private IConversionDataManager mConversionDatManager;

    private SharedPreferences mPrefs;
    private SharedPreferences mUnityPrefs;

    private boolean mIsInitialized = false;

    private WisdomDbHelper mDbHelper;
    private IWisdomStorage mWisdomEventsStorage;
    private EventsLocalApi mEventsLocalApi;
    private EventsLocalDataSource mEventsLocalDatSource;
    private IEventsReporter mEventsReporter;
    private IEventsQueue mEventsQueue;

    private BlockingFullScreenLoader fullScreenLoader;
    private Handler unityMainThreadHandler;

    @Override
    public void init(Activity initialActivity, WisdomConfigurationDto config) {
        SdkLogger.setup(config.isLoggingEnabled);

        unityMainThreadHandler = new Handler();
        mApplication = initialActivity.getApplication();
        mPrefs = initialActivity.getSharedPreferences(WISDOM_PREFS_NAME, Context.MODE_PRIVATE);
        mUnityPrefs = initialActivity.getSharedPreferences(initialActivity.getPackageName() + ".v2.playerprefs", Context.MODE_PRIVATE);

        mAppLifecycleService = new ApplicationLifecycleService(mApplication);
        mAppLifecycleServiceRegistrar = new ApplicationLifecycleServiceRegistrar(mAppLifecycleService);

        mBgWatchdog = new BackgroundWatchdog(initialActivity.getLocalClassName());
        mBackgroundWatchdogRegistrar = new BackgroundWatchdogRegistrar(mBgWatchdog);

        mAppLifecycleServiceRegistrar.registerWatchdog(mBgWatchdog);
        mAppLifecycleServiceRegistrar.startService();

        NetworkUtils networkUtils = new NetworkUtils(mApplication);
        mDispatcher = new WisdomNetworkDispatcher();
        mNetwork = new WisdomNetwork(mDispatcher, config.connectTimeout, config.readTimeout, networkUtils);

        ListStoredEventJsonMapper listJsonMapper = new ListStoredEventJsonMapper();

        mEventsRemoteApi = new EventsRemoteApi(mNetwork, config.subdomain, listJsonMapper);
        mEventsRemoteDataSource = new EventsRemoteDataSource(mEventsRemoteApi);

        StoredEventMapper storedEventMapper = new StoredEventMapper();
        mDbHelper = new WisdomDbHelper(mApplication.getApplicationContext());
        mWisdomEventsStorage = new WisdomEventsStorage(mDbHelper);
        mEventsLocalApi = new EventsLocalApi(mWisdomEventsStorage, storedEventMapper);
        mEventsLocalDatSource = new EventsLocalDataSource(mEventsLocalApi);

        mEventsRepository = new EventsRepository(mEventsRemoteDataSource, mEventsLocalDatSource);

        mEventsQueue = new EventsQueue(mEventsRepository, config.initialSyncInterval);
        mEventsQueue.startQueue();

        mEventMetadataLocalApi = new EventMetadataLocalApi(mPrefs);
        mEventMetadataLocalDataSource = new EventMetadataLocalDataSource(mEventMetadataLocalApi);

        mEventMetadataRepository = new EventMetadataRepository(mEventMetadataLocalDataSource);
        mEventMetadataManager = new EventMetadataManager(mEventMetadataRepository);

        mConversionDataLocalApi = new ConversionDataLocalApi(mUnityPrefs);
        mConversionDataLocalDataSource = new ConversionDataLocalDataSource(mConversionDataLocalApi);
        mConversionDataRepository = new ConversionDataRepository(mConversionDataLocalDataSource);
        mConversionDatManager = new ConversionDataManager(mConversionDataRepository);

        fullScreenLoader = new BlockingFullScreenLoader(initialActivity, config.streamingAssetsFolderPath + "/" + config.blockingLoaderResourceRelativePath, config.blockingLoaderViewportPercentage);

        mEventsReporter = new EventsReporter(mEventsRepository);
        mSessionManager = new SessionManager(mEventsReporter, mEventMetadataManager, mConversionDatManager, mEventsQueue);

        IdentifiersGetter.fetch(initialActivity, new SwCallback<IdentifiersGetter.GetterResults>() {
            @Override
            public void onDone(IdentifiersGetter.GetterResults result) {
                if (result == null) {
                    SdkLogger.error(TAG, "Failed to fetch app set ID + advertising ID");
                } else {
                    String appSetId = result.getAppSetIdentifier();
                    String advertisingId = result.getAdvertisingIdentifier();
                    SdkLogger.log("Got app set ID '" + appSetId + "' from calling via " + TAG);
                    SdkLogger.log("Got advertising ID '" + advertisingId + "' from calling via " + TAG);
                }

                mIsInitialized = true;

                for (IWisdomInitListener initListener : mInitListeners) {
                    initListener.onInitEnded();
                }
            }
        });
    }

    @Override
    public boolean isInitialized() {
        return mIsInitialized;
    }

    @Override
    public void initializeSession(String metadata) {
        mBackgroundWatchdogRegistrar.registerWatchdogListener(mSessionManager);
        mSessionManager.initializeSession(metadata);
        setEventMetadata(metadata);
    }

    @Override
    public boolean toggleBlockingLoader(boolean shouldPresent) {
        boolean didToggle;
        if (shouldPresent) {
            didToggle = fullScreenLoader.show(unityMainThreadHandler);
        } else {
            didToggle = fullScreenLoader.hide();
        }

        return didToggle;
    }

    @Override
    public void updateWisdomConfiguration(WisdomConfigurationDto config) {
        mNetwork.setConnectTimeout(config.connectTimeout);
        mNetwork.setReadTimeout(config.readTimeout);
        mEventsQueue.setInitialSyncInterval(config.initialSyncInterval);
    }

    @Override
    public void setEventMetadata(String metadataJson) {
        mEventMetadataManager.set(metadataJson);
    }

    @Override
    public void updateEventMetadata(String metadataJson) {
        mEventMetadataManager.update(metadataJson);
    }

    @Override
    public void trackEvent(String eventName, String customsJson, String extraJson) {
        String sessionId = mSessionManager.getCurrentSessionId();
        String metaDataJson = (mEventMetadataManager.get());
        JSONObject eventJson = SwUtils.createEvent(eventName, sessionId, mConversionDatManager.getConversionData(), metaDataJson, customsJson, extraJson);
        
        mEventsReporter.reportEvent(eventJson);
    }

    @Override
    public void registerInitListener(IWisdomInitListener listener) {
        mInitListeners.add(listener);
    }

    @Override
    public void unregisterInitListener(IWisdomInitListener listener) {
        mInitListeners.remove(listener);
    }

    @Override
    public void registerSessionListener(IWisdomSessionListener listener) {
        mSessionManager.registerSessionListener(listener);
    }

    @Override
    public void unregisterSessionListener(IWisdomSessionListener listener) {
        mSessionManager.unregisterSessionListener(listener);
    }
    
    @Override
    public String getAdvertisingIdentifier() {
        return IdentifiersGetter.getAdvertisingId();
    }

    @Override
    public String getAppSetIdentifier() {
        return IdentifiersGetter.getAppSetId();
    }

    @Override
    public void destroy() {
        mIsInitialized = false;
        mEventsQueue.stopQueue();

        mAppLifecycleServiceRegistrar.stopService();
        mBackgroundWatchdogRegistrar.unregisterAllWatchdogs();
        mSessionManager.unregisterAllSessionListeners();

        mAppLifecycleServiceRegistrar = null;
        mBackgroundWatchdogRegistrar = null;
        mEventsRemoteApi = null;
        mEventsRemoteDataSource = null;
        mEventsRepository = null;
        mEventMetadataLocalApi = null;
        mEventMetadataLocalDataSource = null;
        mEventMetadataRepository = null;
        mEventMetadataManager = null;
        mSessionManager = null;
        mBgWatchdog = null;
    }
}
