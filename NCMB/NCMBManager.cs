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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NCMB
{
	/// <summary>
	/// プッシュ操作の結果を扱います。
	/// </summary>
	public class NCMBManager : MonoBehaviour
	{
        #region Const
		const string NS = "NCMB_SPLITTER";
        #endregion

        #region Static
		internal static bool Inited { get; set; }
        #endregion

        #region Delegate
	




		/// <summary> 端末登録後のイベントリスナーです。</summary>
		public delegate void OnRegistrationDelegate (string errorMessage);
		/// <summary> プッシュ送信後のイベントリスナーです。</summary>
		public delegate void OnSendPushDelegate (string errorMessage);
		/// <summary> メッセージ受信後のイベントリスナーです。</summary>
		public delegate void OnNotificationReceivedDelegate (NCMBPushPayload payload);
		// <summary> 位置情報成功。</summary>
		//public delegate void OnGetLocationSucceededDelegate(NCMBGeoPoint geo);
		// <summary> 位置情報失敗。</summary>
		//public delegate void OnGetLocationFailedDelegate(string errorMessage);

		/*
		/// <summary>
		/// プッシュ送信後のイベントリスナーを登録します。
		/// </summary>
		static public void OnSendPush (OnSendPushDelegate callback)
		{
			onSendPush += callback;
		}

		/// <summary>
		/// 端末登録後のイベントリスナーを登録します。
		/// </summary>
		static public void OnRegistration (OnRegistrationDelegate callback)
		{
			onRegistration += callback;
		}

		/// <summary>
		/// プッシュ受信後のイベントリスナーを登録します。
		/// </summary>
		static public void OnNotificationReceived (OnNotificationReceivedDelegate callback)
		{
			onNotificationReceived += callback;
		}
		*/

		/// <summary> 端末登録後のイベントリスナーです。</summary>
		public static OnRegistrationDelegate onRegistration;
		/// <summary> プッシュ送信後のイベントリスナーです。</summary>
		public static OnSendPushDelegate onSendPush;
		/// <summary> メッセージ受信後のイベントリスナーです。</summary>
		public static OnNotificationReceivedDelegate onNotificationReceived;
		// <summary> 位置情報成功。</summary>
		//public static OnGetLocationSucceededDelegate onGetLocationSucceeded;
		// <summary> 位置情報失敗。</summary>
		//public static OnGetLocationFailedDelegate onGetLocationFailed;
        #endregion

        #region Messages which are sent from native

		void OnRegistration (string message)
		{
			Inited = true;
			
			if (onRegistration != null) {
				if (message == "") {
					message = null;
				}
				onRegistration (message);
			}
		}

		void OnNotificationReceived (string message)
		{
			if (onNotificationReceived != null) {
				string[] s = message.Split (new string[] { NS }, System.StringSplitOptions.None);
				NCMBPushPayload payload = new NCMBPushPayload (s [0], s [1], s [2], s [3], s [4], s [5], s [6]);
				onNotificationReceived (payload);
			}
		}

		void OnSendPush (string message)
		{
			if (onSendPush != null) {
				if (message == "") {
					message = null;
				}
				onSendPush (message);
			}
		}

		/*
        void OnGetLocationSucceeded(string message)
        {
            if (onGetLocationSucceeded != null)
            {
                string[] s = message.Split(' ');
                NCMBGeoPoint geo = new NCMBGeoPoint(double.Parse(s[0]), double.Parse(s[1]));
                onGetLocationSucceeded(geo);
            }
        }

        void OnGetLocationFailed(string message)
        {
            if (onGetLocationFailed != null)
            {
                onGetLocationFailed(message);
            }
        }
         */
        #endregion
 
        #region Process notification for iOS only
        #if UNITY_IOS
		void Start ()
		{
			ClearAfterOneFrame ();
		}

		void Update ()
		{
			if (NotificationServices.remoteNotificationCount > 0) {
				ProcessNotification ();
				NCMBPush.ClearAll ();
			}
		}

		void ProcessNotification ()
		{
			// Payload data dictionary
			IDictionary dd = NotificationServices.remoteNotifications [0].userInfo;

			// Payload key list
			string[] kl = new string[] { 
                "com.nifty.PushId",
                "com.nifty.Data",
                "com.nifty.Title",
                "com.nifty.Message",
                "com.nifty.Channel",
                "com.nifty.Dialog",
                "com.nifty.RichUrl",
            };

			// Payload value list
			string[] vl = new string[kl.Length];

			// Index of com.nifty.Message
			int im = 0;

			// Loop list
			for (int i = 0; i < kl.Length; i++) {
				// Get value by key, return empty string if not exist
				vl [i] = (dd.Contains (kl [i])) ? dd [kl [i]].ToString () : string.Empty;

				// Find index of com.nifty.message
				im = (kl [i] == "com.nifty.Message") ? i : im;
			}

			// Set message as alertBody
			if (string.IsNullOrEmpty (vl [im])) {
				vl [im] = NotificationServices.remoteNotifications [0].alertBody;
			}

			// Create payload
			NCMBPushPayload pl = new NCMBPushPayload (vl [0], vl [1], vl [2], vl [3], vl [4], vl [5], vl [6], NotificationServices.remoteNotifications [0].userInfo);

			// Notify
			if (onNotificationReceived != null) {
				onNotificationReceived (pl);
			}
		}

		void OnApplicationPause (bool pause)
		{
			if (!pause) {
				ClearAfterOneFrame ();
			}
		}

		void ClearAfterOneFrame ()
		{
			StartCoroutine (IEClearAfterAFrame ());
		}

		IEnumerator IEClearAfterAFrame ()
		{
			yield return 0;
			NCMBPush.ClearAll ();
		}
        #endif
        #endregion
	}

	/// <summary>
	/// プッシュのペイロードデータを扱います。
	/// </summary>
	public class NCMBPushPayload
	{
		/// <summary> プッシュIDの取得を行います。 </summary>
		public string PushId { get; protected set; }
		// <summary> データ。</summary>
		//public string Data { get; protected set; }
		/// <summary> タイトルの取得を行います。</summary>
		public string Title { get; protected set; }
		/// <summary> メッセージの取得を行います。</summary>
		public string Message { get; protected set; }
		// <summary> チャンネル。</summary>
		//public string Channel { get; protected set; }
		//<summary> ダイアログ。</summary>
		//public bool Dialog { get; protected set; }
		// <summary> リッチプッシュURL。</summary>
		//public string RichUrl { get; protected set; }

		/// <summary>
		/// ペイロードのユーザー情報 (iOSのみ)
		/// </summary>
		/// <value>The user info.</value>
		//public IDictionary UserInfo { get; protected set; }

		internal NCMBPushPayload (string pushId, string data, string title, string message, string channel, string dialog, string richUrl, IDictionary userInfo = null)
		{
			PushId = pushId;
			//Data = data;
			Title = title;
			Message = message;
			//Channel = channel;
			//Dialog = (dialog == "true" || dialog == "TRUE" || dialog == "True" || dialog == "1") ? true : false;
			//RichUrl = richUrl;
			//UserInfo = userInfo;
		}

		//public NCMBPushPayload()
		//{
		//}
	}
}
