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
			string userId,
			string screenName,
			string consumerKey,
			string consumerSecret,
			string accessToken,
			string accessTokenSecret
		)
		{
			if (string.IsNullOrEmpty (userId) ||
				string.IsNullOrEmpty (screenName) ||
				string.IsNullOrEmpty (consumerKey) ||
				string.IsNullOrEmpty (consumerSecret) ||
				string.IsNullOrEmpty (accessToken) ||
				string.IsNullOrEmpty (accessTokenSecret)
			)
			{
				throw new NCMBException (new ArgumentException ("constructor parameters must not be null."));
			}

			Dictionary<string, object> twitterParam = new Dictionary<string, object> () {
				{"id", userId},
				{"screen_name", screenName},
				{"oauth_consumer_key", consumerKey},
				{"consumer_secret", consumerSecret},
				{"oauth_token", accessToken},
				{"oauth_token_secret", accessTokenSecret}
			};

			param.Add ("twitter", twitterParam);
		}
	}
}
