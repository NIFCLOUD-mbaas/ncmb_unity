/*******
 Copyright 2017-2018 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 
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
package com.nifcloud.mbaas.ncmbfcmplugin;

import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.media.RingtoneManager;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;

import androidx.core.app.NotificationCompat;

import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import com.unity3d.player.UnityPlayer;

import java.util.Map;
import java.util.Random;

/**
 * FCM push notification receive class
 */
public class NCMBFirebaseMessagingService extends FirebaseMessagingService {
	//<meta-data>
	static final String SMALL_ICON_KEY = "smallIcon";   //AndroidManifestから情報を取得
	static final String SMALL_ICON_COLOR_KEY = "smallIconColor"; //AndroidManifestから情報を取得
	private final String TAG = "NCMBFirebaseMessagingService";
	public static final String NS = "NCMB_SPLITTER";

	/**
	 * Called if InstanceID token is updated. This may occur if the security of
	 * the previous token had been compromised. Note that this is called when the InstanceID token
	 * is initially generated so this is where you would retrieve the token.
	 */
	@Override
	public void onNewToken(String token) {
		// Send refesh token to update installation
		final String saveToken = token;
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				UnityPlayer.UnitySendMessage("NCMBManager", "onTokenReceived", saveToken);
			}
		});
	}

	@Override
	public void onMessageReceived(RemoteMessage remoteMessage) {
		
		if (remoteMessage != null && remoteMessage.getData() != null){
			SharedPreferences recentPushIdPref = this.getSharedPreferences("ncmbPushId", Context.MODE_PRIVATE);
			String recentPushId = recentPushIdPref.getString("recentPushId", "");
			String currentPushId = remoteMessage.getData().get("com.nifcloud.mbaas.PushId");
			Log.d("Unity", "NCMBFirebaseMessagingService onMessageReceived: " + remoteMessage.getData().toString());

			if(!recentPushId.equals(currentPushId)) {
				SharedPreferences.Editor editor = recentPushIdPref.edit();
				editor.putString("recentPushId", currentPushId);
				editor.commit();

				super.onMessageReceived(remoteMessage);
				Bundle bundle = getBundleFromRemoteMessage(remoteMessage);
				sendNotification(bundle); //プッシュ通知表示
				sendPayloadToUnity(bundle); //Unity上でペイロードデータを扱えるよう、メッセージを送る

				//NCMBDialogPushConfigurationクラスのインスタンスを作成
				NCMBDialogPushConfiguration dialogPushConfiguration = new NCMBDialogPushConfiguration();
				//標準的なダイアログを表示するタイプ
				dialogPushConfiguration.setDisplayType(NCMBDialogPushConfiguration.DIALOG_DISPLAY_DIALOG);
				NCMBPush.dialogPushHandler(getApplicationContext(), bundle, dialogPushConfiguration);

			}

		}
	}

	protected Bundle getBundleFromRemoteMessage(RemoteMessage remoteMessage){
		Bundle bundle = new Bundle();
		Map<String, String> data = remoteMessage.getData();
		for(String key: data.keySet()){
			bundle.putString(key, data.get(key));
		}
		return bundle;
	}

	private void sendNotification(Bundle pushData) {

		//サイレントプッシュ
		if ((!pushData.containsKey("message")) && (!pushData.containsKey("title"))) {
			return;
		}

		NotificationCompat.Builder notificationBuilder = notificationSettings(pushData);

		//デフォルト複数表示
		int notificationId = new Random().nextInt();
		Log.d("Unity", "sendNotification " + notificationId);

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

		Log.d("Unity", "activityName: "+ activityName + "|  packageName:" + packageName);

		//Note FCM
		//通知エリアに表示されるプッシュ通知をタップした際に起動するアクティビティ画面を設定する
		Intent intent = new Intent(this, com.nifcloud.mbaas.ncmbfcmplugin.UnityPlayerActivity.class);
		intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
		intent.putExtras(pushData);
		PendingIntent pendingIntent = PendingIntent.getActivity(this, new Random().nextInt(), intent,
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
		//SmallIconカラーを設定
		int smallIconColor = appInfo.metaData.getInt(SMALL_ICON_COLOR_KEY);

		//Notification作成
		Uri defaultSoundUri = RingtoneManager.getDefaultUri(RingtoneManager.TYPE_NOTIFICATION);
		NotificationCompat.Builder notificationBuilder = new NotificationCompat.Builder(this, com.nifcloud.mbaas.ncmbfcmplugin.NCMBNotificationUtils.getDefaultChannel())
				.setSmallIcon(icon)//通知エリアのアイコン
				.setColor(smallIconColor)//通知エリアのアイコンカラー
				.setContentTitle(title)
				.setContentText(message)
				.setAutoCancel(true)//通知をタップしたら自動で削除する
				.setSound(defaultSoundUri)//端末のデフォルトサウンド
				.setContentIntent(pendingIntent);//通知をタップした際に起動するActivity
		return notificationBuilder;
	}

	private void sendPayloadToUnity(Bundle payloadData){
		// Get data
		String pushId = payloadData.getString("com.nifcloud.mbaas.PushId");
		String data = payloadData.getString("com.nifcloud.mbaas.Data");
		String title = payloadData.getString("title");
		String message = payloadData.getString("message");
		String channel = payloadData.getString("com.nifcloud.mbaas.Channel");
		boolean dialog = payloadData.containsKey("com.nifcloud.mbaas.Dialog");
		String richUrl = payloadData.getString("com.nifcloud.mbaas.RichUrl");

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