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
package com.nifty.cloud.mb.ncmbfcmplugin;

import com.google.firebase.iid.FirebaseInstanceId;
import com.google.firebase.iid.FirebaseInstanceIdService;
import com.unity3d.player.UnityPlayer;
/**
 * FCM Id Service to get updated InstanceID token
 */
public class NCMBFirebaseInstanceIdService extends FirebaseInstanceIdService {
    @Override
    public void onTokenRefresh() {
        // Get updated InstanceID token.
        final String refreshedToken = FirebaseInstanceId.getInstance().getToken();

        // Send refesh token to update installation
        if(refreshedToken != null || !refreshedToken.isEmpty()){
            UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
                public void run() {
                    UnityPlayer.UnitySendMessage("NCMBManager", "onTokenReceived", refreshedToken);
                }
            });
        }

    }
}