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
using NCMB;
using NCMB.Internal;
using System.IO;
using System.Runtime.InteropServices;

//System.IO.FileInfo, System.IO.StreamReader, System.IO.StreamWriter
using System;

//Exception
using System.Text;

//Encoding
using System.Collections.Generic;
using MiniJSON;

namespace NCMB
{
	/// <summary>
	/// プッシュ通知関連のイベントリスナーを操作するクラスです。
	/// </summary>
	public class NCMBManager : MonoBehaviour
	{
		#if UNITY_IOS
		[DllImport ("__Internal")]
		private static extern string getInstallationProperty ();
		#endif

		#region Const

		const string NS = "NCMB_SPLITTER";

		#endregion

		#region Static

		internal static bool Inited { get; set; }

		internal static string _token;
		internal static IDictionary<string, object> installationDefaultProperty = new Dictionary<string, object> ();
		#endregion

		#region Delegate

		/// <summary> 端末登録後のイベントリスナーです。</summary>
		public delegate void OnRegistrationDelegate (string errorMessage);
		/// <summary> メッセージ受信後のイベントリスナーです。</summary>
		public delegate void OnNotificationReceivedDelegate (NCMBPushPayload payload);
		// <summary> 位置情報成功。</summary>
		//public delegate void OnGetLocationSucceededDelegate(NCMBGeoPoint geo);
		// <summary> 位置情報失敗。</summary>
		//public delegate void OnGetLocationFailedDelegate(string errorMessage);

		/// <summary> 端末登録後のイベントリスナーです。</summary>
		public static OnRegistrationDelegate onRegistration;
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
			

		#endregion

		#region Process notification for iOS only

		#if UNITY_IOS
		void Start ()
		{
			ClearAfterOneFrame ();
		}

		void Update ()
		{
			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			if (NotificationServices.remoteNotificationCount > 0) {
			#else
			if (UnityEngine.iOS.NotificationServices.remoteNotificationCount > 0) {
			#endif
				ProcessNotification ();
				NCMBPush push = new NCMBPush ();
				push.ClearAll ();
			}
		}

		void ProcessNotification ()
		{
			// Payload data dictionary
			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			IDictionary dd = NotificationServices.remoteNotifications [0].userInfo;
			#else
			IDictionary dd = UnityEngine.iOS.NotificationServices.remoteNotifications [0].userInfo;
			#endif
			
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
				#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
				vl [im] = NotificationServices.remoteNotifications [0].alertBody;
				#else
				vl [im] = UnityEngine.iOS.NotificationServices.remoteNotifications [0].alertBody;
				#endif
			}

			// Create payload
			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			NCMBPushPayload pl = new NCMBPushPayload (vl [0], vl [1], vl [2], vl [3], vl [4], vl [5], vl [6], NotificationServices.remoteNotifications [0].userInfo);
			#else
			NCMBPushPayload pl = new NCMBPushPayload (vl [0], vl [1], vl [2], vl [3], vl [4], vl [5], vl [6], UnityEngine.iOS.NotificationServices.remoteNotifications [0].userInfo);
			#endif

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
			NCMBPush push = new NCMBPush ();
			push.ClearAll ();
		}
        #endif
		#endregion

		internal static string SearchPath ()
		{
			//currentInstallation保存パス設定
			try {
				string path = NCMBSettings.currentInstallationPath;
				//v1の場合
				#if UNITY_IOS && !UNITY_EDITOR
				//既存のcurrentInstallationパス
				path = Application.persistentDataPath;	//var/mobile/Applications/{GUID}/Documents
				path = path.Replace ("Documents", "");
				path += "Library/Private Documents/NCMB/currentInstallation";
				#elif UNITY_ANDROID && !UNITY_EDITOR
				//既存のcurrentInstallationパス
				path = Application.persistentDataPath;	//data/data/(PackageName)/files
				path = path.Replace ("files", "");
				path += "app_NCMB/currentInstallation";
				#endif
				if (!System.IO.File.Exists (path)) {
					//v2の場合
					path = NCMBSettings.currentInstallationPath;
				}
				return path;
			} catch (FileNotFoundException e) {
				throw e;
			}
		}


		//ネイティブでデバイストークン取得後に呼び出されます
		internal void onTokenReceived (string token)
		{
			_token = token;	//onAnalyticsReceivedで使用。

			string path = SearchPath ();	//currentInstallationのパスを設定

			//currentInstallationがあれば読み込み、更新の必要性を判定します
			string jsonText = "";
			NCMBInstallation installation = null;
			if ((jsonText = ReadFile (path)) != "") {	//currentInstallationあり
				installation = new NCMBInstallation (jsonText);
			} else {
				installation = new NCMBInstallation ();
			}

			installation.DeviceToken = _token;

			//端末情報をデータストアに登録
			installation.SaveAsync ((NCMBException saveError) => {	//更新実行
				if (saveError != null) {
					//対処可能なエラー
					if (saveError.ErrorMessage.Equals ("Duplication Error")) {
						//過去に登録したデバイストークンと衝突。アプリの再インストール後などに発生
						updateExistedInstallation (installation, path);
					} else {	
                        
						//想定外のエラー
						OnRegistration (saveError.ErrorMessage);
					}
				} else {
					OnRegistration ("");
				}
			});
		}
        
		private void updateExistedInstallation (NCMBInstallation installation, string path)
		{
			//デバイストークンを更新
			NCMBQuery<NCMBInstallation> query = NCMBInstallation.GetQuery ();	//ObjectId検索
			query.WhereEqualTo ("deviceToken", installation.DeviceToken);
			query.FindAsync ((List<NCMBInstallation> objList, NCMBException findError) => {
				if (findError != null) {
					OnRegistration (findError.ErrorMessage);
				} else if (objList.Count != 0) {
					installation.ObjectId = objList [0].ObjectId;
					installation.SaveAsync ((NCMBException installationUpdateError) => {
						if (installationUpdateError != null) {
							OnRegistration (installationUpdateError.ErrorMessage);
						} else {
							OnRegistration ("");
						}
					});
				}
			});
		}

		//ディスク入出力関数
		//書き込み
		private void SaveFile (string path, string text)
		{
			try {
				Encoding utfEnc = Encoding.GetEncoding ("UTF-8");
				StreamWriter writer =
					new StreamWriter (@path, false, utfEnc);
				writer.WriteLine (text);
				writer.Close ();
			} catch (Exception tryWriteError) {
				if (tryWriteError != null) {
					path = NCMBSettings.currentInstallationPath;//Unityからのアクセス権があり、環境に依存しないパスを設定
					try {
						Encoding utfEnc = Encoding.GetEncoding ("UTF-8");
						StreamWriter writer =
							new StreamWriter (@path, false, utfEnc);
						writer.WriteLine (text);
						writer.Close ();
					} catch (IOException writeError) {
						throw new IOException ("File save error" + writeError.Message);
					}
				}
			}
		}
		// 読み込み
		private string ReadFile (string path)
		{
			string text = "";
			if (System.IO.File.Exists (@path)) {	//ファイル存在確認
				try {
					StreamReader sr = new StreamReader (
						                  path, Encoding.GetEncoding ("UTF-8"));	
					text = sr.ReadToEnd ();
					sr.Close ();
				} catch (Exception tryReadError) {
					if (tryReadError != null) {
						path = NCMBSettings.currentInstallationPath;//Unityからのアクセス権があり、環境に依存しないパスを設定
						try {
							StreamReader sr = new StreamReader (path, Encoding.GetEncoding ("UTF-8"));	
							text = sr.ReadToEnd ();
							sr.Close ();
						} catch (FileNotFoundException readError) {
							throw readError;
						}
					}
				}
			}
			return text;
		}
		//ネイティブからプッシュIDを受け取り開封通知
		private void onAnalyticsReceived (string _pushId)
		{
			NCMBAnalytics.TrackAppOpened (_pushId);
		}

		//installation情報を削除
		internal static void DeleteCurrentInstallation (string path)
		{
			try {
				File.Delete (path);
			} catch (IOException e) {
				throw new IOException ("Delete currentInstallation failed.", e);
			}
		}
		internal string GetCurrentInstallation ()
		{
			string path = SearchPath ();
			return ReadFile (path);
		}

		//各ネイティブコードからInstallation情報を取得
		//applicationName,appVersion,deviceType,timeZone(Asia/Tokyo)を取得
		internal static void CreateInstallationProperty(){
			String jsonString = null;
			#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidJavaClass cls = new AndroidJavaClass("com.nifty.cloud.mb.ncmbgcmplugin.GCMInit");
			jsonString = cls.CallStatic<string>("getInstallationProperty");
			#elif UNITY_IOS && !UNITY_EDITOR
			jsonString = getInstallationProperty();
			#endif
			if(jsonString != null){
				installationDefaultProperty = Json.Deserialize (jsonString) as Dictionary<string, object>;
			}
		}

	}

	/// <summary>
	/// プッシュ通知のペイロードデータを操作するクラスです。
	/// </summary>
	public class NCMBPushPayload
	{
		/// <summary> プッシュIDの取得を行います。 </summary>
		public string PushId { get; protected set; }

		/// <summary> データの取得を行います。</summary>
		public string Data { get; protected set; }

		/// <summary> タイトルの取得を行います。</summary>
		public string Title { get; protected set; }

		/// <summary> メッセージの取得を行います。</summary>
		public string Message { get; protected set; }

		/// <summary> チャネルの取得を行います。</summary>
		public string Channel { get; protected set; }

		/// <summary> ダイアログの取得を行います。</summary>
		public bool Dialog { get; protected set; }

		/// <summary> リッチプッシュURLの取得を行います。</summary>
		public string RichUrl { get; protected set; }

		/// <summary>
		/// ペイロードのユーザー情報の取得を行います。 (iOSのみ)
		/// </summary>
		/// <value>The user info.</value>
		public IDictionary UserInfo { get; protected set; }

		internal NCMBPushPayload (string pushId, string data, string title, string message, string channel, string dialog, string richUrl, IDictionary userInfo = null)
		{
			PushId = pushId;
			Data = data;
			Title = title;
			Message = message;
			Channel = channel;
			Dialog = (dialog == "true" || dialog == "TRUE" || dialog == "True" || dialog == "1") ? true : false;
			RichUrl = richUrl;
			UserInfo = userInfo;
		}
	}
}
