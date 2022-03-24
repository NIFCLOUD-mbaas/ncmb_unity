/*******
 Copyright 2017-2022 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.
 
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

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import com.unity3d.player.UnityPlayer;

public class UnityPlayerActivity extends com.unity3d.player.UnityPlayerActivity {
	private ActivityProxyObjectHelper _proxyHelper;

	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		try {
			this._proxyHelper = new ActivityProxyObjectHelper(this);
			this._proxyHelper.onCreate(savedInstanceState);
		} catch (Exception e) {
			Log.i("NCMB", "Failed to create proxyHelper: " + e.getMessage());
		}
		
	}

	protected void onNewIntent(Intent intent) {
		super.onNewIntent(intent);
		setIntent(intent);
		this._proxyHelper.onNewIntent(intent);
	}

	protected void onDestroy() {
		super.onDestroy();
		this._proxyHelper.invokeZeroParameterMethod("onDestroy");
	}

	public void onResume() {
		super.onResume();
		//リッチプッシュ処理
		NCMBPush.richPushHandler(this, getIntent());
		getIntent().removeExtra("com.nifcloud.mbaas.RichUrl");        //再表示させたくない場合はintentからURLを削除します

		//開封通知処理
		String pushId = getIntent().getStringExtra("com.nifcloud.mbaas.PushId");        //プッシュIDがあればUnityへ送る
		if (pushId != null) {
			UnityPlayer.UnitySendMessage("NCMBManager", "onAnalyticsReceived", pushId);    //Unityの開封通知メソッド呼び出し
			getIntent().removeExtra("com.nifcloud.mbaas.PushId");
		}
		this._proxyHelper.invokeZeroParameterMethod("onResume");
	}
}
