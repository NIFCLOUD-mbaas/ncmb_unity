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
// using System;
using UnityEngine;
// using NCMB.Internal;
// using System.Collections.Generic;

namespace NCMB
{
	/// <summary>
	/// 初期設定を操作するクラスです。
	/// </summary>
	public class NCMBPushSettings : MonoBehaviour
	{

		//PUSH通知フラグ
		private static bool _usePush = false;
		//開封通知フラグ
		private static bool _useAnalytics = false;

		[SerializeField]
		internal bool
			usePush = false;

		[SerializeField]
		internal bool
			useAnalytics = false;

		/// <summary>
		/// プッシュ通知設定の取得を行います。
		/// </summary>
		public static bool UsePush {
			get {
				return _usePush;
			}
		}

		/// <summary>
		/// 開封通知設定の取得を行います。
		/// </summary>
		public static bool UseAnalytics {
			get {
				return _useAnalytics;
			}
		}

		/// <summary>
		/// コンストラクター
		/// </summary>
		public NCMBPushSettings ()
		{
		}


		/// <summary>
		/// iOS,Androidそれぞれの端末登録を行う
		/// </summary>
		/// <param name="usePush">true:プッシュ通知有効　false:プッシュ通知無効</param>
		/// <param name="useAnalytics">true:開封通知有効　false:開封通知無効</param>
		/// <param name="getLocation">true:位置情報有効　false:位置情報無効</param>
		private static void RegisterPush (bool usePush, bool useAnalytics, bool getLocation = false)
		{

			//Push関連設定
			_usePush = usePush;
			_useAnalytics = useAnalytics;

			// Register
			if (usePush) {
				//Installation基本情報を取得
				NCMBManager.CreateInstallationProperty ();
				if (!getLocation) {
					#if UNITY_ANDROID
					NCMBPushUtils.Register();
					#elif UNITY_IOS
					NCMBPushUtils.Register (useAnalytics);
					#endif
				} else {
					#if UNITY_ANDROID
					//not Analytics
					NCMBPushUtils.RegisterWithLocation();
					#elif UNITY_IOS
					NCMBPushUtils.RegisterWithLocation ();
					#endif
				}
			}
		}


		/// <summary>
		/// 初期設定を行います。
		/// </summary>
		public virtual void Awake ()
		{
				//NCMBSettings.RegisterPush(this.usePush, this.androidSenderId, this.getLocation);
				NCMBPushSettings.RegisterPush (this.usePush, this.useAnalytics, false);
		}


	}
}
