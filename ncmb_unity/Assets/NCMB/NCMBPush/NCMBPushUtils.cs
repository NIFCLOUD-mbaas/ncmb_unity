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
// using UnityEngine;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Runtime.InteropServices;
// using MiniJSON;
using NCMB.Internal;

namespace NCMB
{
	/// <summary>
	/// プッシュ通知を操作するクラスです。
	/// </summary>
	[NCMBClassName ("pushUtils")]
	internal class NCMBPushUtils
	{
		#if UNITY_ANDROID
		static AndroidJavaClass m_AJClass;

		#elif UNITY_IOS
		[DllImport ("__Internal")]
		private static extern void registerNotification (bool useAnalytics);

		[DllImport ("__Internal")]
		private static extern void registerNotificationWithLocation ();

		[DllImport ("__Internal")]
		private static extern void clearAll ();
		#endif

		/*** 初期化 ***/
		static NCMBPushUtils ()
		{
			#if UNITY_ANDROID && !UNITY_EDITOR
			m_AJClass = new AndroidJavaClass("com.nifcloud.mbaas.ncmbfcmplugin.FCMInit");
			#endif
		}

		#if UNITY_ANDROID
		public static void Register ()
		{
			#if !UNITY_EDITOR
				m_AJClass.CallStatic("Init");
			#endif
		}

		#elif UNITY_IOS
		public static void Register (bool useAnalytics)
		{
			#if !UNITY_EDITOR
			registerNotification(useAnalytics);
			#endif
		}
		#endif

		#if UNITY_ANDROID
		public static void RegisterWithLocation ()
		{
			#if !UNITY_EDITOR
			m_AJClass.CallStatic("Init");
			#endif
		}

		#elif UNITY_IOS
		/// <summary>
		/// Register for receiving remote notifications (with current location).
		/// </summary>
		internal static void RegisterWithLocation ()
		{
			#if !UNITY_EDITOR
			registerNotificationWithLocation();
			#endif
		}
		#endif

    #region Process notification for iOS only

		#if UNITY_IOS
		// Clears all notifications.
		public void ClearAll ()
		{
			#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
			NotificationServices.ClearRemoteNotifications ();
			#else
			UnityEngine.iOS.NotificationServices.ClearRemoteNotifications ();
			#endif

			#if !UNITY_EDITOR
			clearAll();
			#endif
		}
		#endif
		#endregion

	}
}
