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

using System.Collections;
using System;
using UnityEngine;
using NCMB.Internal;
using System.Collections.Generic;

namespace NCMB
{
	/// <summary>
	/// 初期設定を操作するクラスです。
	/// </summary>
	public class NCMBSettings : MonoBehaviour
	{
		//アプリケションキー
		private static string _applicationKey = "";
		//クライアントキー
		private static string _clientKey = "";
		//ANDROID SENDER キー
		private static string _androidSenderId = "";
		//シグネチャチェックフラグ
		internal static bool _responseValidationFlag = false;
		//初回のみ実行フラグ
		private static bool _isInitialized = false;
		//PUSH通知フラグ
		private static bool _usePush = false;
		//開封通知フラグ
		private static bool _useAnalytics = false;
		//static NG
		[SerializeField]
		internal string
			applicationKey = "";
		[SerializeField]
		internal string
			clientKey = "";
		[SerializeField]
		internal bool
			usePush = false;
		[SerializeField]
		internal bool
			useAnalytics = false;
		[SerializeField]
		internal string
			androidSenderId = "";
		//[SerializeField]
		//internal bool
		//getLocation = false;
		[SerializeField]
		internal bool
			responseValidation = false;
		//Current user
		private static string _currentUser = null;
		internal static string filePath = "";
		internal static string currentInstallationPath = "";

		/// <summary>
		/// Current userの取得、または設定を行います。 
		/// </summary>
		internal static string CurrentUser {
			get {
				return _currentUser;
			}
			set {
				_currentUser = value;
			}
		}

		/// <summary>
		/// アプリケションキーの取得、または設定を行います。 
		/// </summary>
		public static string ApplicationKey {
			get {
				return _applicationKey;
			}
			set {
				_applicationKey = value;
			}
		}

		/// <summary>
		/// クライアントキーの取得、または設定を行います。 
		/// </summary>
		public static string ClientKey {
			get {
				return _clientKey;
			}
			set {
				_clientKey = value;
			}
		}

		/// <summary>
		/// Android SenderIdの取得を行います。
		/// </summary>
		public static string AndroidSenderId {
			get {
				return _androidSenderId;
			}
		}

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
		public NCMBSettings ()
		{
		}

		/// <summary>
		/// 初期設定を行います。
		/// </summary>
		/// <param name="applicationKey">アプリケーションキー</param>
		/// <param name="clientKey">クライアントキー</param>
		public static void Initialize (String applicationKey, String clientKey)
		{
			// アプリケーションキーを設定
			_applicationKey = applicationKey;
			// クライアントキーを設定
			_clientKey = clientKey;
		}

		/// <summary>
		/// iOS,Androidそれぞれの端末登録を行う
		/// </summary>
		/// <param name="usePush">true:プッシュ通知有効　false:プッシュ通知無効</param>
		/// <param name="useAnalytics">true:開封通知有効　false:開封通知無効</param>
		/// <param name="androidSenderId">Android SenderId</param>
		/// <param name="getLocation">true:位置情報有効　false:位置情報無効</param>
		private static void RegisterPush (bool usePush, bool useAnalytics, String androidSenderId, bool getLocation = false)
		{
		
			//Push関連設定
			_usePush = usePush;
			_useAnalytics = useAnalytics;
			_androidSenderId = androidSenderId;

			// Register
			if (usePush) {
				//Installation基本情報を取得
				NCMBManager.CreateInstallationProperty ();
				if (!getLocation) {
					#if UNITY_ANDROID
					NCMBPush.Register (androidSenderId);
					#elif UNITY_IOS
					NCMBPush.Register (useAnalytics);
					#endif
				} else {
					#if UNITY_ANDROID
					//not Analytics
					NCMBPush.RegisterWithLocation (androidSenderId);
					#elif UNITY_IOS
					NCMBPush.RegisterWithLocation ();
					#endif
				}
			}
		}

		/// <summary>
		/// レスポンスが改ざんされていないか判定する機能を有効にします。<br/>
		/// デフォルトは無効です。
		/// </summary>
		/// <param name="checkFlag">true:有効　false:無効</param>
		public static void EnableResponseValidation (bool checkFlag)
		{
			_responseValidationFlag = checkFlag;
		}

		/// <summary>
		/// 初期設定を行います。
		/// </summary>
		public virtual void Awake ()
		{
			if (!NCMBSettings._isInitialized) {
				NCMBSettings._isInitialized = true;
				_responseValidationFlag = responseValidation;
				DontDestroyOnLoad (base.gameObject);
				NCMBSettings.Initialize (this.applicationKey, this.clientKey);
				//NCMBSettings.RegisterPush(this.usePush, this.androidSenderId, this.getLocation);
				filePath = Application.persistentDataPath;
				currentInstallationPath = filePath + "/currentInstallation";
				NCMBSettings.RegisterPush (this.usePush, this.useAnalytics, this.androidSenderId, false);
				base.StartCoroutine (Platform.RunLoop ());
			}
		}
	}
}