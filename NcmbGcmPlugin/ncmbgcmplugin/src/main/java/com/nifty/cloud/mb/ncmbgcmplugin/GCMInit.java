/*******
 * Copyright 2014 NIFTY Corporation All Rights Reserved.
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
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GoogleApiAvailability;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.TimeZone;

//GCMの初期化処理を扱います
public class GCMInit extends Activity {

    private static final int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;

    //Unityから呼び出されます
    public static void InitSenderId(String _senderId) {
        final String senderId = _senderId;
        // Looper prepare
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                SetSenderId(senderId);
            }
        });
    }

    public static void InitSenderIdWithLocation(String _senderId) {
        final String senderId = _senderId;
        // Looper prepare
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                SetSenderId(senderId);
            }
        });
    }

    //GCMInit.javaから呼び出され、デバイストークンの取得サービスを開始します
    public static void SetSenderId(String _senderId) {
        final String senderId = _senderId;
        // Looper prepare
        if (checkPlayServices()) {
            UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
                public void run() {
                    Intent intent = new Intent(UnityPlayer.currentActivity, RegistrationIntentService.class);
                    intent.putExtra("senderId", senderId);
                    UnityPlayer.currentActivity.startService(intent);
                }
            });
        } else {
            UnityPlayer.UnitySendMessage("NCMBManager", "OnRegistration", "This device is not supported google-play-services-APK.");
        }
    }

    /**
     * GooglePlay開発者サービスが有効か確認
     */
    private static boolean checkPlayServices() {
        GoogleApiAvailability apiAvailability = GoogleApiAvailability.getInstance();
        int resultCode = apiAvailability.isGooglePlayServicesAvailable(UnityPlayer.currentActivity);
        if (resultCode != ConnectionResult.SUCCESS) {
            return false;
        }
        return true;
    }

    /**
     * C#から呼び出し
     * installationのプロパティを生成して返却
     * @return installation of json string
     */
    public static String getInstallationProperty(){
        //プロパティを生成
        Context context = UnityPlayer.currentActivity;
        String applicationName = "";
        String appVersion = "";
        String timeZone = TimeZone.getDefault().getID();
        try {
            String packageName = context.getPackageName();
            PackageManager pm = context.getPackageManager();
            appVersion = pm.getPackageInfo(packageName, 0).versionName;
            applicationName = pm.getApplicationLabel(pm.getApplicationInfo(packageName, 0)).toString();
        } catch (PackageManager.NameNotFoundException e) {
            Log.e(null, e.getMessage());
        }

        //JSON文字列に変換
        JSONObject json = new JSONObject();
        try {
            json.put("applicationName", applicationName);
            json.put("appVersion", appVersion);
            json.put("deviceType", "android");
            json.put("timeZone", timeZone);
        } catch (JSONException e) {
            Log.e(null,e.getMessage());
        }

        return json.toString();
    }
}
