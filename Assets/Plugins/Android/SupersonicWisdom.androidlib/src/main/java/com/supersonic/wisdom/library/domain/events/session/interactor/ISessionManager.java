package com.supersonic.wisdom.library.domain.events.session.interactor;

import com.supersonic.wisdom.library.domain.events.session.ISessionListener;
import com.supersonic.wisdom.library.domain.watchdog.listsener.IBackgroundWatchdogListener;

public interface ISessionManager extends IBackgroundWatchdogListener {

    String getCurrentSessionId();
    void initializeSession(String metadata);
    void registerSessionListener(ISessionListener listener);
    void unregisterSessionListener(ISessionListener listener);
    void unregisterAllSessionListeners();
}
