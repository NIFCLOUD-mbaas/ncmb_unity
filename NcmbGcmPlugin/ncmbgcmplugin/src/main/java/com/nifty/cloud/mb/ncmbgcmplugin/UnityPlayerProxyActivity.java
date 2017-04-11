/*******
 * Copyright 2017 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 * <p/>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p/>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p/>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 **********/

package com.nifty.cloud.mb.ncmbgcmplugin;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;

public class UnityPlayerProxyActivity extends Activity {
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        String[] classNames = {"com.nifty.cloud.mb.ncmbgcmplugin.UnityPlayerActivity",
                "com.nifty.cloud.mb.ncmbgcmplugin.UnityPlayerNativeActivity"};
        try {
            boolean supportsNative = Build.VERSION.SDK_INT >= 9;
            Class<?> activity = null;

            if (supportsNative) {
                activity = Class.forName(classNames[1]);
            } else {
                activity = Class.forName(classNames[0]);
            }

            Intent intent = new Intent(this, activity);
            intent.addFlags(65536);

            Bundle extras = getIntent().getExtras();
            if (extras != null) {
                intent.putExtras(extras);
            }
            Uri data = getIntent().getData();
            if (data != null) {
                intent.setData(data);
            }
            startActivity(intent);    //アクティビティ開始
        } catch (ClassNotFoundException e) {
            e.printStackTrace();
        } finally {
            finish();
        }
    }
}
