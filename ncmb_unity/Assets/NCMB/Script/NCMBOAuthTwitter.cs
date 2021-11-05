using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace NCMB
{
    public class NCMBOAuthTwitter
    {
        private static string _consumerConsumerKey;
        private static string _consumerSecretConsumerKey;
        private static string _callbackScheme;
        static INCMBTwitterCallback _iNCMBTwitterCallback;

#if (UNITY_IOS && !UNITY_EDITOR && NCMB_ENABLE_TWITTER)
        private static GameObject NCMBTwitterObject;
#endif
#if (UNITY_ANDROID && !UNITY_EDITOR)
        private static AndroidJavaClass androidClass;
        private static AndroidJavaObject PluginInstance;

#endif
        public static void Init(string key, string secretKey, string scheme)
        {
            // Initial key
            _consumerConsumerKey = key;
            _consumerSecretConsumerKey = secretKey;
            _callbackScheme = scheme;
#if (UNITY_IOS && !UNITY_EDITOR && NCMB_ENABLE_TWITTER)
            NCMBTwitterObject = new GameObject("NCMBTwitterObject");
            NCMBTwitterObject.AddComponent<TwitterComponent>();
#endif
#if (UNITY_ANDROID && !UNITY_EDITOR)
            AndroidJNI.AttachCurrentThread();
            androidClass = new AndroidJavaClass("com.nifcloud.mbaas.twitterauthentication.TwitterAuthentication");
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            androidClass.SetStatic<AndroidJavaObject>("mainActivity", activity);
            PluginInstance = androidClass.CallStatic<AndroidJavaObject>("getInstance");
#endif
        }
        public static void LogIn(INCMBTwitterCallback iNCMBTwitterCallback)
        {
            _iNCMBTwitterCallback = iNCMBTwitterCallback;
#if (UNITY_IOS) && !UNITY_EDITOR && NCMB_ENABLE_TWITTER
            IOSTwitterLogin(
                success =>
                {

                    _iNCMBTwitterCallback.OnSuccess(success.Id, success.Username, success.Token, success.Secret);
                },
                failure =>
                {
                    _iNCMBTwitterCallback.OnFailure(failure.Message);
                }
                );

#endif
#if (UNITY_ANDROID && !UNITY_EDITOR)
            AndroidTwitterLogin();
#endif

        }

        private static void IOSTwitterLogin(Action<AuthToken> successCallback = null, Action<ApiError> failureCallback = null)
        {
#if (UNITY_IOS) && !UNITY_EDITOR && NCMB_ENABLE_TWITTER
            NCMBTwitterObject.GetComponent<TwitterComponent>().LoginSuccessAction = successCallback;
            NCMBTwitterObject.GetComponent<TwitterComponent>().LoginFailureAction = failureCallback;
            // Execute Twitter Login
            ExecuteInvoke.NCMB_LoginWithTwitter(_consumerConsumerKey, _consumerSecretConsumerKey, _callbackScheme);
#endif
        }

        private static void AndroidTwitterLogin()
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
            PluginInstance.Call("getTwitterAuthentication", _consumerConsumerKey, _consumerSecretConsumerKey, _callbackScheme, new AuThenCallback());
#endif
        }

#if (UNITY_ANDROID && !UNITY_EDITOR)
        class AuThenCallback : AndroidJavaProxy
        {

            public AuThenCallback() : base("com.nifcloud.mbaas.twitterauthentication.TwitterOAuthDialog$OnTwitterOAuth")
            {
            }
            public void success(string id, string userName, string token, string tokenSecret)
            {
                _iNCMBTwitterCallback.OnSuccess(id, userName, token, tokenSecret);
                // Debug.Log("id: " + id);
            }
            public void failure(string errorMessage)
            {
                _iNCMBTwitterCallback.OnFailure(errorMessage);
                // Debug.Log("errorMessage: " + errorMessage);
            }
        }
#endif
#if (UNITY_IOS) && !UNITY_EDITOR && NCMB_ENABLE_TWITTER
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
#endif
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

#if (UNITY_IOS) && !UNITY_EDITOR && NCMB_ENABLE_TWITTER
        private static class ExecuteInvoke
        {
            [DllImport("__Internal")]
            public static extern void NCMB_LoginWithTwitter(string consumerConsumerKey, string consumerSecretConsumerKey, string callbackScheme);
        }
#endif
    }

    public interface INCMBTwitterCallback
    {
        void OnSuccess(string id, string userName, string token, string tokenSecret);
        void OnFailure(string errorMessage);
    }

}

