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

using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace NCMB
{
	public class NCMBTwitter
	{
		private static GameObject NCMBTwitterObject;

		private static string _consumerConsumerKey;
		private static string _consumerSecretConsumerKey;
		private static string _callbackScheme;

		public static void Init(string key, string secretKey, string scheme)
		{
			NCMBTwitterObject = new GameObject("NCMBTwitterObject");
			NCMBTwitterObject.AddComponent<TwitterComponent>();
			_consumerConsumerKey = key;
			_consumerSecretConsumerKey = secretKey;
			_callbackScheme = scheme;
		}

		public static void LogIn(Action<AuthToken> successCallback = null, Action<ApiError> failureCallback = null)
		{
#if (UNITY_IOS) && !UNITY_EDITOR
			NCMBTwitterObject.GetComponent<TwitterComponent>().LoginSuccessAction = successCallback;
			NCMBTwitterObject.GetComponent<TwitterComponent>().LoginFailureAction = failureCallback;
			// Execute Twitter Login
			ExecuteInvoke.NCMB_LoginWithTwitter(_consumerConsumerKey, _consumerSecretConsumerKey, _callbackScheme);
#endif
		}

			private class TwitterComponent : MonoBehaviour
			{
				public Action<AuthToken> LoginSuccessAction { set; get; }

				public Action<ApiError> LoginFailureAction { set; get; }

				public void Awake()
				{
					MonoBehaviour.DontDestroyOnLoad(this);
				}

				public void LoginComplete(string session)
				{
					if (LoginSuccessAction != null)
					{
						LoginSuccessAction(JsonUtility.FromJson<AuthToken>(session));
					}
					else
					{
						UnityEngine.Debug.Log("FAILED calling login success action");
					}
				}

				public void LoginFailed(string error)
				{
					if (LoginFailureAction != null)
					{
						LoginFailureAction(JsonUtility.FromJson<ApiError>(error));
					}
					else
					{
						Debug.Log("FAILED calling login fail action");
					}
				}


			}

			/// <summary>
			/// Model for response AuthToken
			/// </summary>
			[Serializable]
			public class AuthToken
			{
				public string id;
				public string username;
				public string token;
				public string secret;

				public string Id { get { return this.id; } }
				public string Username { get { return this.username; } }
				public string Token { get { return this.token; } }
				public string Secret { get { return this.secret; } }

			}

			/// <summary>
			/// Model for error
			/// </summary>
			[Serializable]
			public class ApiError
			{
				public int code;
				public string message;

				public int Code { get { return this.code; } }
				public string Message { get { return this.message; } }

			}
	#if (UNITY_IOS) && !UNITY_EDITOR
			private static class ExecuteInvoke
			{
				[DllImport("__Internal")]
				public static extern void NCMB_LoginWithTwitter(string consumerConsumerKey, string consumerSecretConsumerKey, string callbackScheme);
			}
	#endif
	}
}