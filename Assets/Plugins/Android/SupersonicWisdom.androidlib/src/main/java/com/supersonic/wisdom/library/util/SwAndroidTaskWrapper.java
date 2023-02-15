package com.supersonic.wisdom.library.util;

import java.lang.reflect.Method;

public class SwAndroidTaskWrapper<ResultType> {
    private final Object taskObject;
    private final SwCallback<ResultType> callback;

    public SwAndroidTaskWrapper(Object taskObject, SwCallback<ResultType> callback) {
        this.taskObject = taskObject;
        this.callback = callback;
        waitForResult();
    }

    private void waitForResult() {
        if (callback == null) return;
        if (taskObject == null) {
            callback.onDone(null);
            return;
        }

        SwUtils.bgThreadHandler().postDelayed(new Runnable() {
            @Override
            public void run() {
                ResultType result = null;

                try {
                    Class<?> appSetIdClientClass = Class.forName("com.google.android.gms.tasks.Task");
                    Method isTaskCompletedGetterMethod = appSetIdClientClass.getMethod("isComplete");
                    Object isTaskCompletedObject = isTaskCompletedGetterMethod.invoke(taskObject);
                    if (isTaskCompletedObject instanceof Boolean && ((Boolean) isTaskCompletedObject)) {
                        Method resultGetterMethod = appSetIdClientClass.getMethod("getResult");
                        Object taskResultObject = resultGetterMethod.invoke(taskObject);
                        if (taskResultObject != null) {
                            Class<?> appSetIdInfoClass = Class.forName("com.google.android.gms.appset.AppSetIdInfo");
                            Method infoGetterMethod = appSetIdInfoClass.getMethod("getId");
                            Object resultObject = infoGetterMethod.invoke(taskResultObject);
                            if (resultObject != null) {
                                result = (ResultType) resultObject;
                            }
                        }

                        callback.onDone(result);
                    } else {
                        waitForResult();
                    }
                } catch (Throwable e) {
                    SdkLogger.error("SwAndroidTaskWrapper", e);
                    callback.onDone(result);
                }
            }
        }, 200);
    }
}