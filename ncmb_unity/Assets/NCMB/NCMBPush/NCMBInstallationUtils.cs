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
// using System.Collections;
using System;
using System.IO;
using System.Threading;
// using System.Collections.Generic;
// using MiniJSON;
using NCMB.Internal;
// using System.Linq;
// using UnityEngine;
// using System.Runtime.InteropServices;
// using System.Text;

namespace  NCMB
{
	/// <summary>
	/// プッシュ通知の配信端末を操作するクラスです。
	/// </summary>
	[NCMBClassName ("installationUtils")]
	public class NCMBInstallationUtils
	{

		/// <summary>
		/// デバイストークンの取得を行います。 <br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public static void GetDeviceToken(NCMBInstallation installation, NCMBGetCallback<String> callback){
			if(installation.ContainsKey("deviceToken") && installation["deviceToken"] != null ){
				callback((string)installation["deviceToken"], null);
			} else {
				new Thread(() => {
					for (int i = 0; i < 10; i++){
						if (NCMBManager._token != null){
							installation["deviceToken"] = NCMBManager._token;
							break;
						}
						Thread.Sleep(500);
					}
					if (callback != null){
						if (installation.ContainsKey("deviceToken") && installation["deviceToken"] != null){
							callback((string)installation["deviceToken"], null);
						} else {
							callback(null, new NCMBException("Can not get device token"));
						}
					}
				}).Start();
			}
		}

	}
}
