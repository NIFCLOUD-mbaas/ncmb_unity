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
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MiniJSON;
using NCMB.Internal;

namespace NCMB
{
	/// <summary>
	/// プッシュ通知を操作するクラスです。
	/// </summary>
	[NCMBClassName ("push")]
	public class NCMBPush:NCMBObject
	{
		#if UNITY_ANDROID
		static AndroidJavaClass m_AJClass;

#elif UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void registerNotification (bool useAnalytics);

		[DllImport ("__Internal")]
		private static extern void registerNotificationWithLocation ();

		[DllImport ("__Internal")]
		private static extern void clearAll ();
		#endif
		/*		** 初期化 ***/
		static NCMBPush ()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
                        m_AJClass = new AndroidJavaClass("com.nifcloud.mbaas.ncmbfcmplugin.FCMInit");
			#endif

		}

		/// <summary>
		/// コンストラクター。<br/>
		/// プッシュの作成を行います。
		/// </summary>
		public NCMBPush () : base ()	//継承元のコンストラクタを実施するため
		{
		}
		#if UNITY_ANDROID
		public static void Register ()
		{

#if !UNITY_EDITOR
				m_AJClass.CallStatic("Init");
		#endif
		}

#elif UNITY_IOS
		public static void Register (bool useAnalytics)
		{
			#if !UNITY_EDITOR
		registerNotification(useAnalytics);
			#endif
		}
		#endif
		#if UNITY_ANDROID
	public static void RegisterWithLocation ()
	{

#if !UNITY_EDITOR
			m_AJClass.CallStatic("Init");
		#endif
	}

#elif UNITY_IOS
		/// <summary>
		/// Register for receiving remote notifications (with current location).
		/// </summary>
		internal static void RegisterWithLocation ()
		{
			#if !UNITY_EDITOR
	registerNotificationWithLocation();
			#endif
		}
		#endif
		/*** Push設定 ***/
		/// <summary>
		/// メッセージの取得、または設定を行います。
		/// </summary>
		public string Message {
			get {
				return (string)this ["message"];
			}
			set { this ["message"] = value; }
		}

		/// <summary>
		///  Set search condition
		/// </summary>
		public object SearchCondition {
			get {
				return (object)this ["searchCondition"];
			}
			set { this ["searchCondition"] = value; }
		}

		/// <summary>
		/// 配信時間(日付)の取得、または設定を行います。<br/>
		/// 指定した時間にPushの配信を行います。
		/// </summary>
		public DateTime DeliveryTime {
			get {
				return TimeZoneInfo.ConvertTimeFromUtc ((DateTime)this ["deliveryTime"], TimeZoneInfo.Local);
			}
			set {
				this ["deliveryTime"] = DateTime.Parse (TimeZoneInfo.ConvertTimeToUtc (value).ToString ());
			}
		}

		/// <summary>
		/// 即時配信の取得、または設定を行います。
		/// </summary>
		public bool ImmediateDeliveryFlag {
			get {
				return (bool)this ["immediateDeliveryFlag"];
			}
			set { this ["immediateDeliveryFlag"] = value; }
		}

		/// <summary>
		/// タイトルの取得、または設定を行います(Androidのみ)。
		/// </summary>
		public string Title {
			get {
				return (string)this ["title"];
			}
			set { this ["title"] = value; }
		}

		/// <summary>
		/// iOS端末へ送信フラグの取得、または設定を行います。<br/>
		/// target = [ios, android] または target = [android]の時にfalseを返します。
		/// </summary>
		public bool PushToIOS {
			get {
				bool pushToIOSFlag = false;
				if (ContainsKey ("target")) {
					string[] target = (string[])this ["target"];
					foreach (string value in target) {
						if (value == "ios") {
							pushToIOSFlag = true;
						}
					}
				}
				return pushToIOSFlag;
			}
			set {
				bool pushToAndroidFlag = this.PushToAndroid;
				if (value == true && pushToAndroidFlag == false) {
					this ["target"] = new string[]{ "ios" };
				} else if (value == false && pushToAndroidFlag == true) {
					this ["target"] = new string[]{ "android" };
				} else {
					//[true,true] or [false,false]
					if (ContainsKey ("target")) {
						Remove ("target");
					}
				}
			}
		}

		/// <summary>
		/// Android端末へ送信フラグの取得、または設定を行います。<br/>
		/// target = [ios, android] または target = [ios]の時にfalseを返します。
		/// </summary>
		public bool PushToAndroid {
			get {
				bool PushToAndroidFlag = false;
				if (ContainsKey ("target")) {
					string[] target = (string[])this ["target"];
					foreach (string value in target) {
						if (value == "android") {
							PushToAndroidFlag = true;
						}
					}
				}
				return PushToAndroidFlag;
			}
			set {
				bool pushToIOSFlag = this.PushToIOS;
				if (value == true && pushToIOSFlag == false) {
					this ["target"] = new string[]{ "android" };
				} else if (value == false && pushToIOSFlag == true) {
					this ["target"] = new string[]{ "ios" };
				} else {
					//[true,true] or [false,false]
					if (ContainsKey ("target")) {
						Remove ("target");
					}
				}
			}
		}

		/// <summary>
		/// 設定するバッジ数の取得、または設定を行います(iOSのみ)。<br/>
		/// プッシュ通知のバッジ数は取得出来ません。
		/// </summary>
		public int? Badge {
			get {
				return (int)this ["badgeSetting"];
			}
			set { this ["badgeSetting"] = value; }
		}

		/// <summary>
		/// バッジ増加フラグの取得、または設定を行います(iOSのみ)。
		/// </summary>
		public bool BadgeIncrementFlag {
			get {
				return (bool)this ["badgeIncrementFlag"];
			}
			set { this ["badgeIncrementFlag"] = value; }
		}

		/// <summary>
		/// リッチプッシュURLの取得、または設定を行います。
		/// </summary>
		public string RichUrl {
			get {
				return (string)this ["richUrl"];
			}
			set { this ["richUrl"] = value; }
		}

		/// <summary>
		/// ダイアログプッシュの取得、または設定を行います(Androidのみ)。
		/// </summary>
		public bool Dialog {
			get {
				return (bool)this ["dialog"];
			}
			set { this ["dialog"] = value; }
		}

		/// <summary>
		/// ContentAvailableの取得、または設定を行います(iOSのみ)。
		/// </summary>
		public bool ContentAvailable {
			get {
				return (bool)this ["contentAvailable"];
			}
			set { this ["contentAvailable"] = value; }
		}

		/// <summary>
		/// カテゴリーの取得、または設定を行います(iOSのみ)。
		/// </summary>
		public string Category {
			get {
				return (string)this ["category"];
			}
			set { this ["category"] = value; }
		}

		/// <summary>
		/// 配信期限日の取得、または設定を行います。
		/// </summary>
		public DateTime? DeliveryExpirationDate {
			get {
				return	DeliveryExpirationDate = (DateTime)this ["deliveryExpirationDate"];
			}
			set { this ["deliveryExpirationDate"] = value; }
		}

		/// <summary>
		/// 配信期限時間の取得、または設定を行います。<br/>
		/// 時間単位で指定する場合は「n hour」(n=1～24）、<br/>
		/// 日単位で指定する場合は「n day」（n=1～28） を設定します。
		/// </summary>
		public string DeliveryExpirationTime {
			get {
				return (string)this ["deliveryExpirationTime"];
			}
			set { this ["deliveryExpirationTime"] = value; }
		}
		/*** Push送信 ***/
		/// <summary>
		/// プッシュの送信を行います。
		/// 通信結果が不要な場合はコールバックを指定しないこちらを使用します。
		/// </summary>
		public void SendPush ()
		{
			SendPush (null);
		}
		/*** Push送信 ***/
		/// <summary>
		/// プッシュの送信を行います。
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		public void SendPush (NCMBCallback callback)
		{

			//エラー判定
			if (ContainsKey ("deliveryExpirationDate") && ContainsKey ("deliveryExpirationTime")) {
				throw new ArgumentException ("DeliveryExpirationDate and DeliveryExpirationTime can not be set at the same time.Please set only one.");
			} else if (ContainsKey ("deliveryTime") && ContainsKey ("immediateDeliveryFlag") && ImmediateDeliveryFlag == true) {
				throw new ArgumentException ("deliveryTime and immediateDeliveryFlag can not be set at the same time.Please set only one.");
			}

			//配信時間設定
			if (!ContainsKey ("deliveryTime")) {  //配信日時（日付）の指定がなければ即時配信
				ImmediateDeliveryFlag = true;
			}

			base.SaveAsync (callback);
		}

		#region Process notification for iOS only

		#if UNITY_IOS
		// Clears all notifications.
		public void ClearAll ()
		{
			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			NotificationServices.ClearRemoteNotifications ();
			#else
			UnityEngine.iOS.NotificationServices.ClearRemoteNotifications ();
			#endif

			#if !UNITY_EDITOR
		clearAll();
			#endif
		}
		#endif
		#endregion

		/// <summary>
		/// Push内のオブジェクトで使用出来るクエリを取得します。
		/// </summary>
		/// <returns> クエリ</returns>
		public static NCMBQuery<NCMBPush> GetQuery ()
		{
			return NCMBQuery<NCMBPush>.GetQuery ("push");
		}

		internal override string _getBaseUrl ()
		{
			return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/push";
		}
	}
}
