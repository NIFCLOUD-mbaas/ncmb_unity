package com.nifcloud.mbaas.twitterauthentication;

import android.app.Dialog;
import android.content.Context;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Build;
import android.view.ViewGroup;
import android.webkit.WebResourceRequest;
import android.webkit.WebView;
import android.webkit.WebViewClient;

import androidx.annotation.RequiresApi;

import twitter4j.Twitter;
import twitter4j.TwitterException;
import twitter4j.TwitterFactory;
import twitter4j.auth.AccessToken;
import twitter4j.auth.RequestToken;

public class TwitterOAuthDialog {
    private Context mContext;
    private OnTwitterOAuth mOnTwitterOAuth;
    private OAuthRequestToken mOAuthRequestToken;

    private Dialog dialog;
    private WebView webview;

    // Define Twitter variables
    private RequestToken mRequestToken;
    private Twitter mTwitter;

    private String mCalbackUrl;

    public interface OnTwitterOAuth {
        public void success(String id, String userName, String token, String tokenSecret);

        public void failure(String errorMessage);
    }

    interface OAuthRequestToken {
        void success(String authenticationURL);

        void failure(String errorMessage);
    }

    public TwitterOAuthDialog(String consumerApiKey, String consumerApiKeySecret, String calbackUrl, Context context, OnTwitterOAuth callback) {
        this.mContext = context;
        this.mCalbackUrl = calbackUrl;
        mOnTwitterOAuth = callback;

        mTwitter = new TwitterFactory().getInstance();
        mTwitter.setOAuthConsumer(consumerApiKey, consumerApiKeySecret);

        mOAuthRequestToken = new OAuthRequestToken() {
            @Override
            public void success(String authenticationURL) {
                if (authenticationURL != null) {
                    buildDialog(authenticationURL);
                } else {
                    mOnTwitterOAuth.failure("Get OAuth request token is null.");
                }

            }

            @Override
            public void failure(String errorMessage) {
                mOnTwitterOAuth.failure(errorMessage);
            }
        };
        new GetOAuthRequestToken().execute(calbackUrl);
    }

    private class GetOAuthRequestToken extends AsyncTask<String, String, String> {

        @Override
        protected String doInBackground(String... strings) {
            try {
                String calbackURL = strings[0];
                mRequestToken = mTwitter.getOAuthRequestToken(calbackURL);
            } catch (TwitterException e) {
                mOAuthRequestToken.failure(e.getErrorMessage());
            }
            String authenticationURL = null;
            if (mRequestToken != null) {
                authenticationURL = mRequestToken.getAuthenticationURL();
            }
            return authenticationURL;
        }

        @Override
        protected void onPostExecute(String result) {
            mOAuthRequestToken.success(result);
        }

    }

    public void setOnTwitterOAuth(OnTwitterOAuth onTwitterAuthen) {
        this.mOnTwitterOAuth = onTwitterAuthen;
    }

    private void buildDialog(String authenticationURL) {
        dialog = new Dialog(mContext, R.style.AppTheme_FullScreenDialog);

        dialog.setContentView(R.layout.twitter_authentication_dialog);
        dialog.setCanceledOnTouchOutside(true);
        dialog.setCancelable(true);
        int width = ViewGroup.LayoutParams.MATCH_PARENT;
        int height = ViewGroup.LayoutParams.MATCH_PARENT;
        dialog.getWindow().setLayout(width, height);

        webview = dialog.findViewById(R.id.webview);
        webview.getSettings().setJavaScriptEnabled(true);
        webview.setWebViewClient(webViewClient);
        webview.loadUrl(authenticationURL);

        // Show dialog
        if (dialog != null) {
            dialog.show();
        }
    }

    private WebViewClient webViewClient = new WebViewClient() {

        @Override
        public boolean shouldOverrideUrlLoading(WebView webView, String url) {
            return shouldOverrideUrlLoading(url);
        }

        @RequiresApi(api = Build.VERSION_CODES.LOLLIPOP)
        @Override
        public boolean shouldOverrideUrlLoading(WebView webView, WebResourceRequest request) {
            Uri uri = request.getUrl();
            return shouldOverrideUrlLoading(uri.toString());
        }

        private boolean shouldOverrideUrlLoading(final String url) {

            if (url.contains(mCalbackUrl)) {
                String oauthVerifier = url.substring(url.indexOf("&oauth_verifier=") + 16);
                new GetOAuthAccessToken().execute(oauthVerifier);
            }
            // hide dialog after done
            dialog.hide();
            return true; // Returning True means that application wants to leave the current WebView and handle the url itself, otherwise return false.
        }
    };

    private class GetOAuthAccessToken extends AsyncTask<String, String, Void> {

        @Override
        protected Void doInBackground(String... strings) {
            String oauth_verifier = strings[0];
            try {
                AccessToken accessToken = mTwitter.getOAuthAccessToken(mRequestToken, oauth_verifier);
                if (mOnTwitterOAuth != null) {
                    mOnTwitterOAuth.success(accessToken.getUserId()+"", accessToken.getScreenName(), accessToken.getToken(), accessToken.getTokenSecret());
                }
            } catch (Exception e) {
                if (mOnTwitterOAuth != null) {
                    mOnTwitterOAuth.failure(e.getMessage());
                }
            }
            return null;
        }
    }

}
