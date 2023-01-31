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

using System.Collections;
// using System;
// using System.IO;
// using System.Threading;
using System.Collections.Generic;
// using MiniJSON;
using NCMB.Internal;
// using System.Linq;
// using UnityEngine;
//
// using System.Runtime.CompilerServices;
//
// [assembly:InternalsVisibleTo ("Assembly-CSharp-Editor")]
// [assembly:InternalsVisibleTo ("Tests")]
namespace  NCMB
{
	/// <summary>
	/// 開封通知操作を扱います。
	/// </summary>
	[NCMBClassName ("analyticsUtils")]
	internal class NCMBAnalyticsUtils
	{
		internal static Dictionary<string,object> TrackAppOpenedPrepare (string _pushId) {
			//ネイティブから取得したpushIdからリクエストヘッダを作成
			if (_pushId != null && NCMBManager._token != null && NCMBPushSettings.UseAnalytics) {

				string deviceType = "";
				#if UNITY_ANDROID
				deviceType = "android";
				#elif UNITY_IOS
				deviceType = "ios";
				#endif

				//RESTリクエストデータ生成
				Dictionary<string,object> requestData = new Dictionary<string,object> {
					{ "pushId", _pushId },
					{ "deviceToken", NCMBManager._token },
					{ "deviceType", deviceType }
				};
				return requestData;
			} else {
				return null;
			}
		}
	}

}
