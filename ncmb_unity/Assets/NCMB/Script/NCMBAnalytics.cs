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
using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using MiniJSON;
using NCMB.Internal;
using System.Linq;
using UnityEngine;

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo ("Assembly-CSharp-Editor")]
namespace  NCMB
{
	/// <summary>
	/// 開封通知操作を扱います。
	/// </summary>
	[NCMBClassName ("analytics")]
	internal class NCMBAnalytics
	{
		internal static void TrackAppOpened (string _pushId)	//(Android/iOS)-NCMBManager.onAnalyticsReceived-this.NCMBAnalytics
		{
			//ネイティブから取得したpushIdからリクエストヘッダを作成
			if (_pushId != null && NCMBManager._token != null && NCMBSettings.UseAnalytics) {

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

				var json = Json.Serialize (requestData);
				string url = NCMBAnalytics._getBaseUrl (_pushId);
				ConnectType type = ConnectType.POST;
				string content = json.ToString ();

				//ログを確認（通信前）
				NCMBDebug.Log ("【url】:" + url + Environment.NewLine + "【type】:" + type + Environment.NewLine + "【content】:" + content);
				// 通信処理
				NCMBConnection con = new NCMBConnection (url, type, content, NCMBUser._getCurrentSessionToken ());
				con.Connect (delegate(int statusCode, string responseData, NCMBException error) {
					try {
						NCMBDebug.Log ("【StatusCode】:" + statusCode + Environment.NewLine + "【Error】:" + error + Environment.NewLine + "【ResponseData】:" + responseData);
					} catch (Exception e) {
						error = new NCMBException (e);
					}
					return;
				});

				#if UNITY_IOS
					UnityEngine.iOS.NotificationServices.ClearRemoteNotifications ();
				#endif

			}
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		internal NCMBAnalytics ()
		{
		}
		//オーバーライド
		internal static string _getBaseUrl (string _pushId)
		{
			return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/push/" + _pushId + "/openNumber";
		}
	}
}
