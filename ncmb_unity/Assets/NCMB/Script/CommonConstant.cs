/*******
 Copyright 2017-2018 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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

namespace NCMB.Internal
{
	//通信種別
	internal enum ConnectType
	{
		//GET通信
		GET,
		//POST通信
		POST,
		//PUT通信
		PUT,
		//DELETE通信
		DELETE
	}

	/// <summary>
	/// 定数を定義する共通用のクラスです
	/// </summary>
	internal static class CommonConstant
	{
		//service
		public static readonly string DOMAIN = "mbaas.api.nifcloud.com";//ドメイン
		public static readonly string DOMAIN_URL = "https://mbaas.api.nifcloud.com";//ドメインのURL

		public static readonly string API_VERSION = "2013-09-01";//APIバージョン
		public static readonly string SDK_VERSION = "4.0.0"; //SDKバージョン
		//DEBUG LOG Setting: NCMBDebugにてdefine設定をしてください
	}
}
