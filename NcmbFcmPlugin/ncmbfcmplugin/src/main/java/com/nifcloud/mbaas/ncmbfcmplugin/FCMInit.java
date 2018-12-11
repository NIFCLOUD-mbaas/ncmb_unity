/*******
 * Copyright 2017-2018 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
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

package com.nifcloud.mbaas.ncmbfcmplugin;

import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Build;
import android.support.annotation.NonNull;
import android.util.Log;

import com.google.android.gms.common.ConnectionResult;
import com.google.android.gms.common.GoogleApiAvailability;
import com.google.android.gms.tasks.OnCanceledListener;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnFailureListener;
import com.google.android.gms.tasks.Task;
import com.google.firebase.FirebaseApp;
import com.google.firebase.iid.FirebaseInstanceId;
import com.google.firebase.iid.InstanceIdResult;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.TimeZone;

//FCMの初期化処理を扱います
public class FCMInit extends Activity {
    private static final String NOT_SUPPORT_PLAY_SERVICE_MESSAGE =
             "This device is not supported google-play-services-APK.";
    private static final String CAN_NOT_GET_TOKEN_MESSAGE =
            "Can not get device token";

    public static void Init(){

        if (checkPlayServices()) {
            // Init firebase app
            FirebaseApp.initializeApp(UnityPlayer.currentActivity.getApplicationContext());
            if (!FirebaseApp.getApps(UnityPlayer.currentActivity.getApplicationContext()).isEmpty()) {

                //On Complete Listener
                FirebaseInstanceId.getInstance().getInstanceId().addOnCompleteListener(new OnCompleteListener<InstanceIdResult>() {
                    @Override
                    public void onComplete(@NonNull Task<InstanceIdResult> task) {
                        if(task.isSuccessful()) {
                            final String token = task.getResult().getToken();
                            //Setting chanel for Android O
                            if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.O){
                                NCMBNotificationUtils utils = new NCMBNotificationUtils(UnityPlayer.currentActivity);
                                utils.settingDefaultChannels();
                            }
                            //Save recent token
                            NCMBNotificationUtils.saveRecentToken(token);
                            //Send token to NCMBManager
                            sendMessageToManager("onTokenReceived", token);
                        } else{
                            sendMessageToManager("OnRegistration", CAN_NOT_GET_TOKEN_MESSAGE);
                        }
                    }
                });

                //On Canceled Listener
                FirebaseInstanceId.getInstance().getInstanceId().addOnCanceledListener(new OnCanceledListener() {
                    @Override
                    public void onCanceled() {
                        sendMessageToManager("OnRegistration", CAN_NOT_GET_TOKEN_MESSAGE);
                    }
                });

                //On Failure Listener
                FirebaseInstanceId.getInstance().getInstanceId().addOnFailureListener(new OnFailureListener() {
                    @Override
                    public void onFailure(@NonNull Exception e) {
                        Log.e("Error", e.toString());
                        sendMessageToManager("OnRegistration", CAN_NOT_GET_TOKEN_MESSAGE);
                    }
                });
            } else {
                sendMessageToManager("OnRegistration", CAN_NOT_GET_TOKEN_MESSAGE);
            }

        } else {
            sendMessageToManager("OnRegistration", NOT_SUPPORT_PLAY_SERVICE_MESSAGE);
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

    /**
     * Send message to NCMBManager
     */
    protected static void sendMessageToManager(final String method, final String message){
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                UnityPlayer.UnitySendMessage("NCMBManager", method, message);
            }
        });
    }
}
