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
	/// Twitterアカウントのパラメータを受け取るクラスです。
	/// </summary>
	[NCMBClassName("twitterParameters")]
	public class NCMBTwitterParameters
	{

		internal Dictionary<string, object> param = new Dictionary<string, object> ();

		/// <summary>
		/// コンストラクター。
		/// </summary>	
		public NCMBTwitterParameters(
			string aUserId,
			string aScreenName,
			string aConsumerKey,
			string aConsumerSecret,
			string anAccessToken,
			string anAccessTokenSecret
		)
		{
			if (string.IsNullOrEmpty (aUserId) ||
				string.IsNullOrEmpty (aScreenName) ||
				string.IsNullOrEmpty (aConsumerKey) ||
				string.IsNullOrEmpty (aConsumerSecret) ||
				string.IsNullOrEmpty (anAccessToken) ||
				string.IsNullOrEmpty (anAccessTokenSecret)
			)
			{
				throw new NCMBException (new ArgumentException ("constructor parameters must not be null."));
			}

			Dictionary<string, object> twitterParam = new Dictionary<string, object> () {
				{"id", aUserId},
				{"screen_name", aScreenName},
				{"oauth_consumer_key", aConsumerKey},
				{"consumer_secret", aConsumerSecret},
				{"oauth_token", anAccessToken},
				{"oauth_token_secret", anAccessTokenSecret}
			};

			param.Add ("twitter", twitterParam);
		}
	}
}
