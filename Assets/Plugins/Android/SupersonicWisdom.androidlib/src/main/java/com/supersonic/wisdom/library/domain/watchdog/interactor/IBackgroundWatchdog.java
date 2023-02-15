package com.supersonic.wisdom.library.domain.watchdog.interactor;

import com.supersonic.wisdom.library.domain.watchdog.listsener.IBackgroundWatchdogListener;

public abstract class IBackgroundWatchdog implements IWatchdog {

    protected abstract void registerListener(IBackgroundWatchdogListener listener);
    protected abstract void unregisterListener(IBackgroundWatchdogListener listener);
    protected abstract void unregisterAllListeners();
}
