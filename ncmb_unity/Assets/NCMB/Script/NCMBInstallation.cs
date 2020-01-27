/*******
 Copyright 2017-2020 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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
using System.Runtime.InteropServices;
using System.Text;

namespace  NCMB
{
	/// <summary>
	/// プッシュ通知の配信端末を操作するクラスです。
	/// </summary>
	[NCMBClassName ("installation")]
	public class NCMBInstallation : NCMBObject
	{

		private void setDefaultProperty ()
		{
			IDictionary<string, object> dic = NCMBManager.installationDefaultProperty;
			object value;
			if (dic.TryGetValue ("applicationName", out value)) {
				ApplicationName = (string)value;
			}
			if (dic.TryGetValue ("appVersion", out value)) {
				AppVersion = (string)value;
			}
			if (dic.TryGetValue ("deviceType", out value)) {
				DeviceType = (string)value;
			}
			if (dic.TryGetValue ("timeZone", out value)) {
				TimeZone = (string)value;
			}

			#if UNITY_ANDROID && !UNITY_EDITOR
			this["pushType"] = "fcm";
			#endif

			SdkVersion = CommonConstant.SDK_VERSION;
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// installationsの作成を行います。
		/// </summary>
		public NCMBInstallation () : this ("")
		{
		}

		/// <summary>
		/// コンストラクター。<br/>
		/// JSONデータをセットしinstallationを作成する場合、こちらを使用します。
		/// </summary>
		internal NCMBInstallation (string jsonText): base ()//NCMBObjectのコンストラクタ実行
		{
			if (jsonText != null && jsonText != "") {
				Dictionary<string, object> dic = Json.Deserialize (jsonText) as Dictionary<string, object>;	//辞書形式へ変換
				object value;
				if (dic.TryGetValue ("data", out value)) {
					//iOSのみv1からアップデートした場合は{"data":{"objectId”:”xxxx…
					dic = (Dictionary<string, object>)value;
				}

				//各プロパティの設定
				_mergeFromServer (dic, false);
			}

			//固定値のため、internal化したsetter
			DeviceToken = NCMBManager._token;
			//applicationName,appVersion,deviceType,timeZone,SdkVersionを取得/設定
			#if !UNITY_EDITOR
			setDefaultProperty ();
			#endif
		}

		/// <summary>
		/// アプリ名の取得、または設定を行います。
		/// </summary>
		public string ApplicationName {
			get {
				return (string)this ["applicationName"];
			}
			internal set {
				this ["applicationName"] = value;
			}
		}

		/// <summary>
		/// アプリバージョンの取得、または設定を行います。
		/// </summary>
		public string AppVersion {
			get {
				return (string)this ["appVersion"];
			}
			internal set {
				this ["appVersion"] = value;
			}
		}

		/// <summary>
		/// デバイストークンの設定を行います。
		/// </summary>
		public string DeviceToken {
			set {
				this ["deviceToken"] = value;
			}
		}

		/// <summary>
		/// デバイストークンの取得を行います。 <br/>
		/// 通信結果が必要な場合はコールバックを指定するこちらを使用します。
		/// </summary>
		/// <param name="callback">コールバック</param>
		public void GetDeviceToken(NCMBGetCallback<String> callback){
			if(this.ContainsKey("deviceToken") && this["deviceToken"] != null ){
				callback((string)this["deviceToken"], null);
			} else {
				new Thread(() => {
					for (int i = 0; i < 10; i++){
						if (NCMBManager._token != null){
							this["deviceToken"] = NCMBManager._token;
							break;
						}
						Thread.Sleep(500);
					}
					if (callback != null){
						if (this.ContainsKey("deviceToken") && this["deviceToken"] != null){
							callback((string)this["deviceToken"], null);
						} else {
							callback(null, new NCMBException("Can not get device token"));
						}
					}
				}).Start();
			}
		}

		/// <summary>
		/// Android/iOSの取得、または設定を行います。
		/// </summary>
		public string DeviceType {
			get {
				return (string)this ["deviceType"];
			}
			internal set {
				this ["deviceType"] = value;
			}
		}

		/// <summary>
		/// SDKバージョンの取得、または設定を行います。
		/// </summary>
		public string SdkVersion {
			get {
				return (string)this ["sdkVersion"];
			}
			internal set {
				this ["sdkVersion"] = value;
			}
		}

		/// <summary>
		/// タイムゾーンの取得、または設定を行います。
		/// </summary>
		public string TimeZone {
			get {
				return (string)this ["timeZone"];
			}
			internal set {
				this ["timeZone"] = value;
			}
		}

		/// <summary>
		/// 現在の配信端末情報を取得します。
		/// </summary>
		/// <returns>配信端末情報 </returns>
		public static NCMBInstallation getCurrentInstallation ()
		{
			NCMBInstallation currentInstallation = null;
			try {
				//ローカルファイルに配信端末情報があれば取得、なければ新規作成
				string currentInstallationData = NCMBManager.GetCurrentInstallation ();
				if (currentInstallationData != "") {
					//ローカルファイルから端末情報を取得
					currentInstallation = new NCMBInstallation (currentInstallationData);
				} else {
					currentInstallation = new NCMBInstallation ();
				}
			} catch (SystemException error) {
				throw new NCMBException (error);
			}
			return currentInstallation;
		}

		/// <summary>
		/// installation内のオブジェクトで使用出来るクエリを取得します。
		/// </summary>
		/// <returns> クエリ</returns>
		public static NCMBQuery<NCMBInstallation> GetQuery ()
		{
			return NCMBQuery<NCMBInstallation>.GetQuery ("installation");
		}

		internal override string _getBaseUrl ()
		{
			return NCMBSettings.DomainURL + "/" + NCMBSettings.APIVersion + "/installations";
		}

		//SaveAsync通信後の処理
		internal override void _afterSave (int statusCode, NCMBException error)
		{
			if (error != null) {
				if (error.ErrorCode == "E404001") {
					//No data available時にcurrentInstallationを削除
					NCMBManager.DeleteCurrentInstallation (NCMBManager.SearchPath ());
				}
			} else if (statusCode == 201 || statusCode == 200) {
				string path = NCMBManager.SearchPath ();
				if (path != NCMBSettings.currentInstallationPath) {
					//成功時にv1のファイルを削除
					NCMBManager.DeleteCurrentInstallation (path);
				}
				_saveInstallationToDisk ("currentInstallation");
			}
		}

		internal void _saveInstallationToDisk (string fileName)
		{

			string path = NCMBSettings.filePath;
			string filePath = path + "/" + fileName;
			object obj;
			Monitor.Enter (obj = this.mutex);
			try {
				string jsonData = _toJsonDataForDataFile ();

				//save to file
				using (StreamWriter sw = new StreamWriter (@filePath, false, Encoding.UTF8)) {
					sw.Write (jsonData);
					sw.Close ();
				}
			} catch (Exception e) {
				throw new NCMBException (e);
			} finally {
				Monitor.Exit (obj);
			}
		}

	}
}
