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

package com.nifty.cloud.mb.ncmbgcmplugin;


import android.os.Bundle;

public class NCMBDialogListenerService extends NCMBGcmListenerService {

    @Override
    public void onMessageReceived(String from, Bundle bundleData) {
        //プッシュ通知受信時の挙動をカスタマイズ

        //デフォルトの通知を実行する場合はsuper.onMessageReceivedを実行する
        super.onMessageReceived(from, bundleData);

        //NCMBDialogPushConfigurationクラスのインスタンスを作成
        NCMBDialogPushConfiguration dialogPushConfiguration = new NCMBDialogPushConfiguration();
        //標準的なダイアログを表示するタイプ
        dialogPushConfiguration.setDisplayType(NCMBDialogPushConfiguration.DIALOG_DISPLAY_DIALOG);
        NCMBPush.dialogPushHandler(getApplicationContext(), bundleData, dialogPushConfiguration);
    }
}

