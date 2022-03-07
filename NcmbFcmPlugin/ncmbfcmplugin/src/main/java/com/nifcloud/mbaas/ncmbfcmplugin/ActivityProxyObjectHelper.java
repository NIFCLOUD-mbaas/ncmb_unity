/*******
 * Copyright 2017-2022 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 **********/

package com.nifcloud.mbaas.ncmbfcmplugin;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.util.Log;

import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

public class ActivityProxyObjectHelper {
    public static final String metaDataValue = "UnityPlayerActivityProxy";
    protected static final String TAG = "NCMB";

    private List<Class<?>> _proxyClasses = new ArrayList<Class<?>>();
    private Activity _context;

    public ActivityProxyObjectHelper(Activity context) {
        this._context = context;
    }

    protected void onCreate(Bundle savedInstanceState) {
        Bundle bundle;
        try {
            ApplicationInfo ai = this._context.getPackageManager()
                    .getApplicationInfo(this._context.getPackageName(), 128);
            bundle = ai.metaData;

            for (String key : bundle.keySet()) {
                try {
                    Object bundleValue = bundle.get(key);
                    if ((bundleValue instanceof String)) {
                        String value = (String) bundleValue;
                        if (value.equalsIgnoreCase(metaDataValue)) {
                            try {
                                Class<?> classObj = Class.forName(key);
                                this._proxyClasses.add(classObj);
                                Log.i(TAG, "found Activity proxy class: " + classObj);
                            } catch (ClassNotFoundException e) {
                                Log.e(TAG, "no proxy class found for " + key);
                            }
                        }
                    }
                } catch (Exception localException) {
                }
            }
        } catch (PackageManager.NameNotFoundException e) {
            Log.i(TAG, "Failed to load meta-data, NameNotFound: " + e.getMessage());
        } catch (NullPointerException e) {
            Log.e(TAG, "Failed to load meta-data, NullPointer: " + e.getMessage());
        }

        for (Class<?> c : this._proxyClasses) {
            try {
                Method m = c.getMethod("onCreate", new Class[]{Bundle.class});
                m.invoke(null, new Object[]{savedInstanceState});
            } catch (Exception localException1) {
            }
        }
    }

    protected void onNewIntent(Intent intent) {
        for (Class<?> c : this._proxyClasses) {
            try {
                Method m = c.getMethod("onNewIntent", new Class[]{Intent.class});
                m.invoke(null, new Object[]{intent});
            } catch (Exception localException) {
            }
        }
    }

    protected void invokeZeroParameterMethod(String method) {
        for (Class<?> c : this._proxyClasses) {
            try {
                Method m = c.getMethod(method, new Class[0]);
                m.invoke(null, new Object[0]);
            } catch (Exception localException) {
            }
        }
    }
}
