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

using System;

namespace NCMB
{
	/// <summary>
	/// 位置情報操作を扱います。
	/// </summary>
	public struct NCMBGeoPoint
	{
		private double latitude;
		private double longitude;

		/// <summary>
		/// 緯度の取得、または設定を行ないます。<br/>
		/// 設定の有効範囲は[-90.0~90.0]です。
		/// </summary>
		public double Latitude {
			get {
				return this.latitude;
			}
			set {
				if (value > 90.0 || value < -90.0) {
					throw new NCMBException (new ArgumentException ("Latitude must be within the range -90.0~90.0"));
				}
				this.latitude = value;
			}
		}

		/// <summary>
		/// 経度の取得、または設定を行ないます。<br/>
		///設定の有効範囲は[-180.0~180.0]です。
		/// </summary>
		public double Longitude {
			get {
				return this.longitude;
			}
			set {
				if (value > 180.0 || value < -180.0) {
					throw new NCMBException (new ArgumentException ("Longitude must be within the range -180~180"));
				}
				this.longitude = value;
			}
		}

		/// <summary>
		/// コンストラクター。
		/// </summary>
		/// <param name="latitude">緯度</param>
		/// <param name="longitude">経度</param>
		public NCMBGeoPoint (double latitude, double longitude)
		{
			this = default(NCMBGeoPoint);
			this.Latitude = latitude;
			this.Longitude = longitude;
		}
	}
}
