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
using NCMB.Internal;


namespace  NCMB
{
	/// <summary>
	/// プッシュ通知の配信端末を操作するクラスです。
	/// </summary>
	[NCMBClassName ("installation")]
	public class NCMBInstallation : NCMBObject
	{

		/// <summary>
		/// コンストラクター。<br/>
		/// installationsの作成を行います。
		/// </summary>
		public NCMBInstallation () : base ()
		{
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

	}
}
