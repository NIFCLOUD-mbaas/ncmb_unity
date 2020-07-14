using System;
using UnityEngine;

namespace NCMB
{
    public class NCMBOAuthTwitter
    {

#if (UNITY_ANDROID && !UNITY_EDITOR)
        AndroidJavaClass androidClass;
        AndroidJavaObject PluginInstance;
        static INCMBTwitterCallback _iNCMBTwitterCallback;
#endif
        public NCMBOAuthTwitter()
        {
            #if (UNITY_ANDROID && !UNITY_EDITOR)
            AndroidJNI.AttachCurrentThread();
            androidClass = new AndroidJavaClass("com.nifcloud.mbaas.twitterauthentication.TwitterAuthentication");
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            androidClass.SetStatic<AndroidJavaObject>("mainActivity", activity);
            PluginInstance = androidClass.CallStatic<AndroidJavaObject>("getInstance");
            #endif
            

        }
        public void NCMBTwitterLogin(string consumerApiKey, string consumerApiKeySecret, string callbackURL, INCMBTwitterCallback iNCMBTwitterCallback)
        {
#if (UNITY_ANDROID && !UNITY_EDITOR)
            _iNCMBTwitterCallback = iNCMBTwitterCallback;
            PluginInstance.Call("getTwitterAuthentication", consumerApiKey, consumerApiKeySecret, callbackURL, new AuThenCallback());
#endif
        }
#if (UNITY_ANDROID && !UNITY_EDITOR)
        class AuThenCallback : AndroidJavaProxy
        {

            public AuThenCallback() : base("com.nifcloud.mbaas.twitterauthentication.TwitterOAuthDialog$OnTwitterOAuth")
            {
            }
            public void success(long id, string userName, string token, string tokenSecret)
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
    }

    public interface INCMBTwitterCallback {
        void OnSuccess(long id, string userName, string token, string tokenSecret);
        void OnFailure(string errorMessage);
    }
}
    
