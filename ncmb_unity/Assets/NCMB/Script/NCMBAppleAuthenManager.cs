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

 This file incorporates work covered by the following copyright and  
  permission notice:  
     https://github.com/lupidan/apple-signin-unity
     Copyright (c) 2019 Daniel Lupiañez Casares

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software
  and associated documentation files (the "Software"), to deal in the Software without restriction, 
  including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
  ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 **********/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace NCMB
{
    public class NCMBAppleAuthenManager
    {
        public NCMBAppleAuthenManager()
        {
        }

        /// <summary>
        /// Method call iOS native login with apple id.
        /// </summary>
        /// <param name="successCallback">return a NCMBAppleCredential after iOS native login with apple id successful.</param>
        /// <param name="errorCallback">return a NCMBAppleError after iOS native login with apple id failure.</param>
        public void NCMBiOSNativeLoginWithAppleId(Action<NCMBAppleCredential> successCallback,
            Action<NCMBAppleError> errorCallback)
        {
#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            var requestId = ControlCallbackAction.AddCallback(
                payloadResponse =>
                {
                    var response = JsonUtility.FromJson<NCMBAppleResponse>(payloadResponse);
                    if (response.Error != null)
                        errorCallback(response.Error);
                    else
                        successCallback(response.NCMBAppleCredential);
                });

            ExecuteInvoke.NCMBAppleAuth_LoginWithAppleId(requestId);
#endif
        }

        public void Update()
        {
#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            ControlCallbackAction.ExecuteCallbacks();
#endif
        }
#if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR

        /// <summary>
        /// ControlCallbackAction handle for callback.
        /// </summary>
        private static class ControlCallbackAction
        {
            private static readonly object SyncLock = new object();
            private static readonly Dictionary<uint,
                ActionEntry> CallbackDictionary = new Dictionary<uint, ActionEntry>();
            private static readonly List<Action>
                QueueActions = new List<Action>();

            private static uint callbackId = 1U;
            private static bool isInit = false;

            public static void QueueCallback(uint requestId, string payload)
            {
                lock (SyncLock)
                {
                    var callbackEntry = default(ActionEntry);
                    if (CallbackDictionary.TryGetValue(requestId, out callbackEntry))
                    {
                        var callback = callbackEntry.Callback;
                        QueueActions.Add(() => callback.Invoke(payload));
                        CallbackDictionary.Remove(requestId);
                    }
                }
            }

            /// <summary>
            /// ExecuteCallbacks
            /// </summary>
            public static void ExecuteCallbacks()
            {
                lock (SyncLock)
                {
                    while (QueueActions.Count > 0)
                    {
                        var action = QueueActions[0];
                        QueueActions.RemoveAt(0);
                        action.Invoke();
                    }
                }
            }

            /// <summary>
            /// AddCallback and return a callback id.
            /// </summary>
            /// <param name="callback">Action<string></param>
            /// <returns>an uint</returns>
            public static uint AddCallback(Action<string> callback)
            {
                // Add a callback if did not initialized.
                if (!isInit)
                {
                    ExecuteInvoke.NCMBAppleAuth_HandlerCallback(ExecuteInvoke.HandlerCallback);
                    isInit = true;
                }

                // throw an exception: can't add a null callback.
                if (callback == null)
                {
                    throw new Exception("Callback is null.");
                }

                var currentCallbackId = default(uint);
                lock (SyncLock)
                {
                    currentCallbackId = callbackId;
                    callbackId += 1;
                    if (callbackId >= uint.MaxValue)
                        callbackId = 1U;

                    var callbackEntry = new ActionEntry(callback);
                    CallbackDictionary.Add(currentCallbackId, callbackEntry);
                }
                return currentCallbackId;
            }

            /// <summary>
            /// ActionEntry
            /// </summary>
            private class ActionEntry
            {
                public readonly Action<string> Callback;

                public ActionEntry(Action<string> callback)
                {
                    this.Callback = callback;
                }
            }
        }

        /// <summary>
        /// Method call invoke.
        /// </summary>
        private static class ExecuteInvoke
        {
            public delegate void CallbackDelegate(uint requestId, string payload);

            [MonoPInvokeCallback(typeof(CallbackDelegate))]
            public static void HandlerCallback(uint requestId, string payload)
            {
                try
                {
                    ControlCallbackAction.QueueCallback(requestId, payload);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Exception: " + exception);
                }
            }

            [DllImport("__Internal")]
            public static extern void NCMBAppleAuth_HandlerCallback(CallbackDelegate callback);

            [DllImport("__Internal")]
            public static extern void NCMBAppleAuth_LoginWithAppleId(uint requestId);
        }
#endif
    }

    /// <summary>
    /// Apple data response from native iOS.
    /// </summary>
    [Serializable]
    public class NCMBAppleResponse : ISerializationCallbackReceiver
    {
        public bool isHasCredential;
        public bool isHasError;
        public NCMBAppleCredential credential;
        public NCMBAppleError error;

        public NCMBAppleError Error { get { return this.error; } }
        public NCMBAppleCredential NCMBAppleCredential { get { return this.credential; } }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (!this.isHasError)
            {
                this.error = default(NCMBAppleError);
            }
            if (!this.isHasCredential)
            {
                this.credential = default(NCMBAppleCredential);
            }
        }
    }

    /// <summary>
    /// Model for Apple error response.
    /// </summary>
    [Serializable]
    public class NCMBAppleError
    {
        public int code;
        public string domain;
        public string userInfo;

        public int Code { get { return this.code; } }
        public string Domain { get { return this.domain; } }
        public string UserInfo { get { return this.userInfo; } }

    }

    /// <summary>
    /// Model for Apple credential response.
    /// </summary>
    [Serializable]
    public class NCMBAppleCredential
    {
        public string authorizationCode;
        public string userId;

        public string AuthorizationCode { get { return this.authorizationCode; } }
        public string UserId { get { return this.userId; } }

    }

}
