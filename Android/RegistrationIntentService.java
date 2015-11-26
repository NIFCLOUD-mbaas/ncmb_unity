/**
 * Copyright 2015 Google Inc. All Rights Reserved.
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
 */

package com.nifty.cloud.mb.ncmbgcmplugin;

import android.app.IntentService;
import android.content.Intent;
import android.util.Log;

import com.google.android.gms.gcm.GoogleCloudMessaging;
import com.google.android.gms.iid.InstanceID;
import com.unity3d.player.UnityPlayer;

public class RegistrationIntentService extends IntentService {

    private static final String TAG = "RegIntentService";
    private static final String[] TOPICS = {"global"};
    private String token = null;

    public RegistrationIntentService() {
        super(TAG);
    }

    @Override
    protected void onHandleIntent(Intent intent) {
        try {
            //デバイストークンを取得します
            InstanceID instanceID = InstanceID.getInstance(this);
            String senderId = "";
            senderId = intent.getStringExtra("senderId");  //UnityのNCMBSettingsからsenderIdを受け取ります
            token = instanceID.getToken(senderId,
                    GoogleCloudMessaging.INSTANCE_ID_SCOPE, null);
            Log.i(TAG, "GCM Registration Token: " + token);
        } catch (Exception e) {
            // Looper prepare
            final String errorMessage = e.getMessage();
            UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
                public void run() {
                    UnityPlayer.UnitySendMessage("NCMBManager", "OnRegistration", errorMessage);
                }
            });
        }
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        //非同期処理のコールバック。Unityへデバイストークンを返す
        if (token != null) {
            UnityPlayer.UnitySendMessage("NCMBManager", "onTokenReceived", token);
        }
    }
}
