/*******
 Copyright 2017-2021 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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
    [NCMBClassName("appleParameters")]
    public class NCMBAppleParameters
    {
        internal Dictionary<string, object> param = new Dictionary<string, object> ();

        public NCMBAppleParameters(string userId, string accessToken, string clientId)
        {
            if (string.IsNullOrEmpty (userId) ||
				string.IsNullOrEmpty (accessToken) ||
				string.IsNullOrEmpty (clientId)
			)
			{
				throw new NCMBException (new ArgumentException ("userId or accessToken or clientId must not be null."));
			}
            Dictionary<string, object> appleParam = new Dictionary<string, object> () {
				{"id", userId},
				{"access_token", accessToken},
				{"client_id", clientId}
			};
            param.Add ("apple", appleParam);
        }
    }
}

