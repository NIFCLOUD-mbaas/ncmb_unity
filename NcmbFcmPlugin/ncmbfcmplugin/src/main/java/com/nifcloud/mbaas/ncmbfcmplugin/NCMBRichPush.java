package com.nifcloud.mbaas.ncmbfcmplugin;

import android.annotation.SuppressLint;
import android.app.Dialog;
import android.app.ProgressDialog;
import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Color;
import android.graphics.Point;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.view.Display;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;

import com.unity3d.player.UnityPlayer;

/**
 * NCMBRichPush provide dialog for rich push notification
 */
public class NCMBRichPush extends Dialog {

    private static final FrameLayout.LayoutParams FILL = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
    private LinearLayout webBackView;
    private FrameLayout richPushHandlerContainer;
    private ImageView closeImage;
    private String requestUrl;
    private ProgressDialog progressDialog;

    public NCMBRichPush(Context context, String requestUrl) {
        super(context, context.getResources().getIdentifier("Theme.Translucent.NoTitleBar", "style", "android"));
        this.requestUrl = requestUrl;
    }

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        this.progressDialog = new ProgressDialog(getContext());
        this.progressDialog.requestWindowFeature(Window.FEATURE_NO_TITLE);
        this.progressDialog.setMessage("Loading...");

        requestWindowFeature(Window.FEATURE_NO_TITLE);
        this.richPushHandlerContainer = new FrameLayout(getContext());

        createCloseImage();

        setUpWebView();

        FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WRAP_CONTENT, FrameLayout.LayoutParams.WRAP_CONTENT);
        layoutParams.gravity = Gravity.RIGHT | Gravity.TOP;
        this.richPushHandlerContainer.addView(this.closeImage, layoutParams);
        addContentView(this.richPushHandlerContainer, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
    }


    @SuppressLint("SetJavaScriptEnabled")
    private void setUpWebView() {

        this.webBackView = new LinearLayout(getContext());
        LinearLayout webViewContainer = new LinearLayout(getContext());

        WebView webView = new WebView(getContext());
        webView.setVerticalScrollBarEnabled(false);
        webView.setHorizontalScrollBarEnabled(false);
        webView.setWebViewClient(new RichPushWebViewClient());
        webView.getSettings().setJavaScriptEnabled(true);
        webView.getSettings().setBuiltInZoomControls(true);
        webView.getSettings().setUseWideViewPort(true);
        webView.loadUrl(this.requestUrl);
        webView.setLayoutParams(FILL);

        this.webBackView.setLayoutParams(new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
        this.webBackView.setBackgroundColor(Color.DKGRAY);
        this.webBackView.setPadding(3, 3, 3, 3);
        this.webBackView.addView(webView);
        this.webBackView.setVisibility(View.INVISIBLE);

        //NoSuchMethodError
        WindowManager wm = (WindowManager) getContext().getSystemService(Context.WINDOW_SERVICE);
        Display disp = wm.getDefaultDisplay();
        Point size = new Point();
        //API14以上
        disp.getSize(size);
        //API14以下
        //int dispWidth = disp.getWidth() / 60;
        int dispWidth = size.x / 60;
        int closeImageWidth = this.closeImage.getDrawable().getIntrinsicWidth();
        webViewContainer.setPadding(dispWidth, closeImageWidth / 2, dispWidth, dispWidth);
        webViewContainer.addView(this.webBackView);

        this.richPushHandlerContainer.addView(webViewContainer);
    }

    private void createCloseImage() {

        this.closeImage = new ImageView(getContext());

        this.closeImage.setOnClickListener(new View.OnClickListener() {
            public void onClick(View v) {
                NCMBRichPush.this.cancel();
            }
        });
        int btnDialog = UnityPlayer.currentActivity.getResources().getIdentifier("btn_dialog", "drawable", "android");
        Drawable closeDrawable = getContext().getResources().getDrawable(btnDialog);
        this.closeImage.setImageDrawable(closeDrawable);

        this.closeImage.setVisibility(View.INVISIBLE);
    }

    private class RichPushWebViewClient extends WebViewClient {
        private RichPushWebViewClient() {
        }

        public boolean shouldOverrideUrlLoading(WebView view, String url) {
            return false;
        }

        public void onReceivedError(WebView view, int errorCode, String description, String failingUrl) {
            super.onReceivedError(view, errorCode, description, failingUrl);
        }

        public void onPageStarted(WebView view, String url, Bitmap favicon) {
            super.onPageStarted(view, url, favicon);
            NCMBRichPush.this.progressDialog.show();
        }

        public void onPageFinished(WebView view, String url) {
            super.onPageFinished(view, url);
            try {
                NCMBRichPush.this.progressDialog.dismiss();
            } catch (IllegalArgumentException localIllegalArgumentException) {
            }

            NCMBRichPush.this.richPushHandlerContainer.setBackgroundColor(0);
            NCMBRichPush.this.webBackView.setVisibility(View.VISIBLE);
            NCMBRichPush.this.closeImage.setVisibility(View.VISIBLE);
        }
    }
}
