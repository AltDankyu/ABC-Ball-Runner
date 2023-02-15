package com.supersonic.wisdom.library.domain.watchdog.interactor;

import com.supersonic.wisdom.library.domain.watchdog.listsener.IBackgroundWatchdogListener;

public class BackgroundWatchdogRegistrar {

    private IBackgroundWatchdog mWatchdog;

    public BackgroundWatchdogRegistrar(IBackgroundWatchdog watchdog) {
        mWatchdog = watchdog;
    }

    public void registerWatchdogListener(IBackgroundWatchdogListener listener) {
        mWatchdog.registerListener(listener);
    }

    public void unregisterWatchdogListener(IBackgroundWatchdogListener listener) {
        mWatchdog.unregisterListener(listener);
    }

    public void unregisterAllWatchdogs() {
        mWatchdog.unregisterAllListeners();
    }
}
