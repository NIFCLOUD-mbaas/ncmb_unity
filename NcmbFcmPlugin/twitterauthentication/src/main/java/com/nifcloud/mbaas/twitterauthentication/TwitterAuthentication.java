package com.nifcloud.mbaas.twitterauthentication;

import android.app.Activity;

public class TwitterAuthentication {
    private static TwitterAuthentication instance;
    public static TwitterAuthentication getInstance() {
        if (instance == null) {
            instance = new TwitterAuthentication();
        }
        return instance;
    }
    public static Activity mainActivity;
    public static TwitterOAuthDialog twitterOAuthDialog;
    private TwitterAuthentication() {}
    public void getTwitterAuthentication(String consumerApiKey, String consumerApiKeySecret, String calbackUrl, TwitterOAuthDialog.OnTwitterOAuth callback) {
        twitterOAuthDialog = new TwitterOAuthDialog(consumerApiKey, consumerApiKeySecret, calbackUrl, mainActivity, callback);
    }
}
