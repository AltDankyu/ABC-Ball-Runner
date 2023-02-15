package com.supersonic.wisdom.library.domain.watchdog.listsener;

/**
 * Interface definition for a callback to be invoked when application
 * changes the state(foreground/background)
 */
public interface IBackgroundWatchdogListener {

    /**
     * Called when application moved to the foreground
     */
    void onAppMovedToForeground();

    /**
     * Called when application moved to the background
     */
    void onAppMovedToBackground();
}
