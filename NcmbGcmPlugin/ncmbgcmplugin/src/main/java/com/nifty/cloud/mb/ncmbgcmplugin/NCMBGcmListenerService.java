/*******
 Copyright 2014 NIFTY Corporation All Rights Reserved.

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

 http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 **********/

package com.nifty.cloud.mb.ncmbgcmplugin;

import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.media.RingtoneManager;
import android.net.Uri;
import android.os.Bundle;
import android.support.v4.app.NotificationCompat;

import com.google.android.gms.gcm.GcmListenerService;
import com.unity3d.player.UnityPlayer;

/**
 * GCM push notification receive class
 */
public class NCMBGcmListenerService extends GcmListenerService {

    //<meta-data>
    static final String SMALL_ICON_KEY = "smallIcon";   //AndroidManifestから情報を取得
    private final String TAG = "NCMBGcmListenerService";
    public static final String NS = "NCMB_SPLITTER";

    @Override
    public void onMessageReceived(String from, Bundle data) {
        sendNotification(data); //プッシュ通知表示
        sendPayloadToUnity(data); //Unity上でペイロードデータを扱えるよう、メッセージを送る
    }

    private void sendNotification(Bundle pushData) {

        //サイレントプッシュ
        if ((!pushData.containsKey("message")) && (!pushData.containsKey("title"))) {
            return;
        }

        NotificationCompat.Builder notificationBuilder = notificationSettings(pushData);

        //デフォルト複数表示
        int notificationId = (int) System.currentTimeMillis();

        NotificationManager notificationManager =
                (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

        notificationManager.notify(notificationId, notificationBuilder.build());
    }

    public NotificationCompat.Builder notificationSettings(Bundle pushData) {
        //AndroidManifestから情報を取得
        ApplicationInfo appInfo = null;
        Class startClass = null;
        String applicationName = null;
        String activityName = null;
        String packageName = null;
        int channelIcon = 0;
        try {
            appInfo = getPackageManager().getApplicationInfo(getPackageName(), PackageManager.GET_META_DATA);
            applicationName = getPackageManager().getApplicationLabel(getPackageManager().getApplicationInfo(getPackageName(), 0)).toString();
            activityName = appInfo.packageName + ".UnityPlayerNativeActivity";
            packageName = appInfo.packageName;
        } catch (PackageManager.NameNotFoundException e) {
            throw new IllegalArgumentException(e);
        }

        //通知エリアに表示されるプッシュ通知をタップした際に起動するアクティビティ画面を設定する
        Intent intent = new Intent(this, UnityPlayerNativeActivity.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
        intent.putExtras(pushData);
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0 /* Request code */, intent,
                PendingIntent.FLAG_CANCEL_CURRENT);

        //pushDataから情報を取得
        String message = "";
        String title = "";
        if (pushData.getString("title") != null) {
            title = pushData.getString("title");
        } else {
            //titleの設定が無い場合はアプリ名をセットする
            title = applicationName;
        }
        if (pushData.getString("message") != null) {
            message = pushData.getString("message");
        }

        //SmallIconを設定。manifestsにユーザー指定の設定が無い場合はアプリアイコンを設定する
        int userSmallIcon = appInfo.metaData.getInt(SMALL_ICON_KEY);
        int icon;
        if (userSmallIcon != 0) {
            //manifestsにアイコン設定がされている場合
            icon = userSmallIcon;
        } else {
            //それ以外はアプリのアイコンを設定する
            icon = appInfo.icon;
        }

        //Notification作成
        Uri defaultSoundUri = RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION);
        NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this)
                .setSmallIcon(icon)//通知エリアのアイコン
                .setContentTitle(title)
                .setContentText(message)
                .setAutoCancel(true)//通知をタップしたら自動で削除する
                .setSound(defaultSoundUri)//端末のデフォルトサウンド
                .setContentIntent(pendingIntent);//通知をタップした際に起動するActivity
        return notificationBuilder;
    }

    private void sendPayloadToUnity(Bundle payloadData){
        // Get data


        String pushId = payloadData.getString("com.nifty.PushId");
        String data = payloadData.getString("com.nifty.Data");
        String title = payloadData.getString("title");
        String message = payloadData.getString("message");
        String channel = payloadData.getString("com.nifty.Channel");
        boolean dialog = payloadData.containsKey("com.nifty.Dialog");
        String richUrl = payloadData.getString("com.nifty.RichUrl");

        String dataString = pushId + NS +
                            data + NS +
                            title + NS +
                            message + NS +
                            channel + NS +
                            dialog + NS +
                            richUrl;
        try {
            UnityPlayer.UnitySendMessage("NCMBManager", "OnNotificationReceived", dataString);
        } catch (UnsatisfiedLinkError error) {
            //バックグラウンド時
        }
    }
}
