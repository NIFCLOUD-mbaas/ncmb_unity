/*
 * Copyright 2017-2018 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.nifty.cloud.mb.ncmbfcmplugin;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.content.ContextWrapper;
import android.graphics.Color;
import android.os.Build;

import com.unity3d.player.UnityPlayer;

/**
 * The NCMBNotificationUtils Class contains register channel and get channel method
 */
public class NCMBNotificationUtils extends ContextWrapper{
    private NotificationManager mManager;
    // デフォルトチャンネルID
    private static final String DEFAULT_CHANNEL_ID = "com.nifty.cloud.mb.push.channel";
    // デフォルトチャンネル名
    private static final String DEFAULT_CHANNEL_NAME = "NCMB Push Channel";
    // デフォルトチャンネル説明
    private static final String DEFAULT_CHANNEL_DES = "Nifty Cloud mobile backend push notification channel";

    public NCMBNotificationUtils(Context base) {
        super(base);
    }

    public static void settingDefaultChannels() {
        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            // チャンネルを作成
            NotificationChannel androidChannel = new NotificationChannel(DEFAULT_CHANNEL_ID,
                    DEFAULT_CHANNEL_NAME, NotificationManager.IMPORTANCE_DEFAULT);
            androidChannel.setDescription(DEFAULT_CHANNEL_DES);
            androidChannel.enableLights(true);
            androidChannel.enableVibration(true);
            androidChannel.setLightColor(Color.GREEN);
            androidChannel.setLockscreenVisibility(Notification.VISIBILITY_PRIVATE);
            new NCMBNotificationUtils(UnityPlayer.currentActivity).getManager().createNotificationChannel(androidChannel);
        }
    }

    public NotificationManager getManager() {
        if (mManager == null) {
            mManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
        }
        return mManager;
    }

    public static String getDefaultChannel(){
        return DEFAULT_CHANNEL_ID;
    }
}

