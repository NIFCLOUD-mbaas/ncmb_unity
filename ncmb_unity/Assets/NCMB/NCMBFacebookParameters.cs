/*******
 Copyright 2017 NIFTY Corporation All Rights Reserved.
 
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
using System.Collections.Generic;
using System;
using NCMB.Internal;
using UnityEngine;

namespace NCMB
{

	/// <summary>
	/// Facebookアカウントのパラメータを受け取るクラスです。
	/// </summary>
	[NCMBClassName("facebookParameters")]
	public class NCMBFacebookParameters
	{
		
		internal Dictionary<string, object> param = new Dictionary<string, object> ();

		/// <summary>
		/// コンストラクター。
		/// </summary>	
		public NCMBFacebookParameters(string aUserId, string anAccessToken, DateTime anExpirationDate)
		{
			if (string.IsNullOrEmpty (aUserId) ||
				string.IsNullOrEmpty (anAccessToken) ||
				string.IsNullOrEmpty (NCMBUtility.encodeDate (anExpirationDate))
			) 
			{
				throw new NCMBException (new ArgumentException ("aUserId or anAccessToken or anExpirationDate must not be null."));
			}

			Dictionary<string, object> objDate = new Dictionary<string, object> () {
				{"__type", "Date"},
				{"iso", NCMBUtility.encodeDate (anExpirationDate)}
			};
			Dictionary<string, object> facebookParam = new Dictionary<string, object> () {
				{"id", aUserId},
				{"access_token", anAccessToken},
				{"expiration_date", objDate}
			};

			param.Add ("facebook", facebookParam);
		}
	}
}