package com.nifcloud.mbaas.ncmbfcmplugin;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.NotificationManager;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.res.Resources;
import android.graphics.Color;
import android.graphics.drawable.GradientDrawable;
import android.graphics.drawable.GradientDrawable.Orientation;
import android.os.Bundle;
import android.os.Handler;
import android.os.PowerManager;
import android.text.TextUtils.TruncateAt;
import android.text.method.ScrollingMovementMethod;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnFocusChangeListener;
import android.view.View.OnTouchListener;
import android.view.ViewGroup;
import android.view.WindowManager.LayoutParams;
import android.view.animation.AlphaAnimation;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import java.util.Timer;
import java.util.TimerTask;

/**
 * Default Activity for dialaog push notification
 */
@SuppressLint({"InlinedApi", "NewApi", "Wakelock"})
public class NCMBDialogActivity extends Activity {
    private static final String TAG = "NCMBDialogActivity";

    static final String SMALL_ICON_KEY = "smallIcon";

    static final String INTENT_EXTRA_LAUNCH_CLASS = "STARTACTIVITY";
    static final String INTENT_EXTRA_THEME = "THEME";
    static final String INTENT_EXTRA_SUBJECT = "SUBJECT";
    static final String INTENT_EXTRA_MESSAGE = "MESSAGE";
    static final String INTENT_EXTRA_DISPLAYTYPE = "DISPLAY_TYPE";

    // ユーザ定義レイアウトファイル名NCMBDialogActivity
    final String USER_LAYOUT_FILE_NAME = "ncmb_notification_dialog";
    // ユーザ定義レイアウトファイル用　サブジェクト
    final String USER_DEFINE_SUBJECT = "ncmb_dialog_subject_id";
    // ユーザ定義レイアウトファイル用　メッセージ
    final String USER_DEFINE_MESSAGE = "ncmb_dialog_message_id";
    // ユーザ定義レイアウトファイル用　閉じるボタン
    final String USER_DEFINE_CLOSE_BUTTON = "ncmb_button_close";
    // ユーザ定義レイアウトファイル用　表示ボタン
    final String USER_DEFINE_OPEN_BUTTON = "ncmb_button_open";

    PowerManager.WakeLock mWakelock;
    MyTimerTask timerTask = null;
    Timer mTimer = null;
    Handler mHandler = new Handler();
    boolean charDialog = false;
    int displayType = 0;
    int backgroundImage = 0;

    private FrameLayout frameLayout;

    @SuppressLint("Wakelock")
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // 表示タイプ毎の処理を実行
        selectDialogType();
    }

    // ダイアログ表示中に新たなintentを受信した場合
    protected void onNewIntent(Intent intent) {
        setIntent(intent);

        // 表示タイプ毎の処理を実行
        selectDialogType();
    }

    @Override
    protected void onResume() {
        super.onResume();

        if ((displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_DIALOG) ||
                (displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_BACKGROUND)) {
            // ダイアログの表示形式がダイアログまたは背景付ダイアログの場合
            if (charDialog) {
                // 背景画像パスが設定されている場合
                setUpDialogChar();
            } else {
                // 背景画像が設定されていない場合
                setUpDialog();
            }
        } else if (displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_ORIGINAL) {
            // オリジナルレイアウトの場合
            try {
                createOriginalLayoutDialog();
            } catch (Exception e) {
                throw new RuntimeException("Error could not create original layout in onResume().");
            }
        } else {
            // 表示形式が不正
            throw new RuntimeException("Error displayType is invalid.");
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (mWakelock != null && mWakelock.isHeld()) {
            mWakelock.release();
        }

        getWindow().clearFlags(LayoutParams.FLAG_TURN_SCREEN_ON |
                LayoutParams.FLAG_SHOW_WHEN_LOCKED);
    }

    protected void turnOnScreen() {
        PowerManager pm = (PowerManager) getSystemService(POWER_SERVICE);
        mWakelock = pm.newWakeLock(PowerManager.FULL_WAKE_LOCK
                | PowerManager.ACQUIRE_CAUSES_WAKEUP
                | PowerManager.ON_AFTER_RELEASE, "NCMBDialogActivity");
        mWakelock.acquire();
        waitForLight();

        getWindow().setFlags(LayoutParams.FLAG_TURN_SCREEN_ON, LayoutParams.FLAG_TURN_SCREEN_ON);
        getWindow().setFlags(LayoutParams.FLAG_SHOW_WHEN_LOCKED, LayoutParams.FLAG_SHOW_WHEN_LOCKED);
    }

    protected Boolean waitForLight() {
        long curTime = System.currentTimeMillis();
        while (!((PowerManager) getSystemService(POWER_SERVICE)).isScreenOn()) {
            if (System.currentTimeMillis() - curTime > 9999) {
                return false;
            }
        }
        return true;
    }

    class MyTimerTask extends TimerTask {
        @Override
        public void run() {
            mHandler.post(new Runnable() {
                public void run() {
                    if (mWakelock.isHeld()) {
                        mWakelock.release();
                    }
                    //timeOut
                }
            });
        }
    }

    // 標準ダイアログ
    private void setUpDialog() {
        // 小枠の枠線作成
        GradientDrawable gradientDrawable = createFrameBorder();

        // 小枠作成
        LinearLayout linearLayout = createLinearLayout();
        linearLayout.setBackgroundDrawable(gradientDrawable);
        linearLayout.setMinimumHeight(convertDpToPixel(240));

        // 小枠の上側作成(アイコン,タイトルエリア)
        LinearLayout upperLayout = createUpperLayout();
        linearLayout.addView(upperLayout);

        // 小枠の上側(タイトルエリア)と中間(メッセージエリア)との境界線作成
        View dividingLine = this.createDividingLine();
        linearLayout.addView(dividingLine);

        // 小枠の中間作成(メッセージエリア)
        TextView message = createCenterLayout();
        linearLayout.addView(message);

        // 小枠の下側作成(閉じる,表示ボタンエリア)
        LinearLayout lowerLayout = createLowerLayout();
        linearLayout.addView(lowerLayout);
        this.frameLayout.addView(linearLayout);
    }

    // 背景画像指定ダイアログ
    private void setUpDialogChar() {
        // 背景作成
        //String imagepath = (String) getIntent().getExtras().get(INTENT_EXTRA_IMAGEPATH);
        //Bitmap bm = BitmapFactory.decodeFile(imagepath);
        ImageView backgroundView = new ImageView(this);
        FrameLayout.LayoutParams backgroundLayout = new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
        backgroundLayout.gravity = Gravity.CENTER_HORIZONTAL;
        backgroundView.setLayoutParams(backgroundLayout);
        backgroundView.setImageResource(backgroundImage);
        //backgroundView.setImageBitmap(bm);
        this.frameLayout.addView(backgroundView);

        // 小枠作成
        LinearLayout linearLayout = createLinearLayout();

        // メッセージエリアの枠線作成
        GradientDrawable gradientDrawable = createFrameBorder();


        // 小枠の上側(アイコン,タイトルエリア)は背景画像指定ダイアログには設定しない

        // 小枠の中間作成(メッセージエリア)
        TextView message = createCenterLayout();
        message.setBackgroundDrawable(gradientDrawable);
        linearLayout.addView(message);

        // 小枠の下側作成(閉じる,表示ボタンエリア)
        LinearLayout lowerLayout = createLowerLayout();
        linearLayout.addView(lowerLayout);
        this.frameLayout.addView(linearLayout);
    }

    // dpからpx変換メソッド
    private int convertDpToPixel(int dp) {
        final float scale = getResources().getDisplayMetrics().density;
        int px = (int) (dp * scale + 0.5f);
        return px;
    }

    // 表示タイプ毎の処理を実行
    private void selectDialogType(){
        // 表示形式を取得
        displayType = (Integer) getIntent().getExtras().get(INTENT_EXTRA_DISPLAYTYPE);

        if (displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_DIALOG) {
            // ダイアログの表示形式がダイアログの場合

            this.frameLayout = new FrameLayout(this);
            this.frameLayout.setBackgroundColor(Color.parseColor("#A0000000"));
            this.frameLayout.setLayoutParams(new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
            setContentView(this.frameLayout);

            charDialog = false;
        } else if (displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_BACKGROUND) {
            // ダイアログの表示形式が背景付ダイアログの場合

            // 背景画像パスを取得
            ApplicationInfo appInfo = null;
            try {
                appInfo = getPackageManager().getApplicationInfo(getPackageName(), PackageManager.GET_META_DATA);
            } catch (NameNotFoundException e) {
                throw new IllegalArgumentException(e);
            }
            backgroundImage = appInfo.metaData.getInt("dialogPushBackgroundImage");

            if (backgroundImage != 0) {
                // 背景画像パスが設定されている場合
                this.frameLayout = new FrameLayout(this);
                this.frameLayout.setLayoutParams(new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
                setContentView(this.frameLayout);

                charDialog = true;
            } else {
                // 背景画像が設定されていない場合
                this.frameLayout = new FrameLayout(this);
                this.frameLayout.setBackgroundColor(Color.parseColor("#A0000000"));
                this.frameLayout.setLayoutParams(new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT));
                setContentView(this.frameLayout);

                charDialog = false;
            }
        } else if (displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_ORIGINAL) {
            // オリジナルレイアウトの場合
            try {
                PackageManager pManager = getPackageManager();
                Resources parentResources = pManager.getResourcesForApplication(getPackageName());

                int resID = parentResources.getIdentifier(USER_LAYOUT_FILE_NAME, "layout", getPackageName());
                setContentView(resID);

            } catch (Exception e) {
                throw new RuntimeException("Error could not show original layout.");
            }
        }

        int theme = (Integer) getIntent().getExtras().get(INTENT_EXTRA_THEME);
        setTheme(theme);

        turnOnScreen();
    }

    // オリジナルレイアウトダイアログ
    public void createOriginalLayoutDialog() throws NameNotFoundException {
        PackageManager pManager = getPackageManager();
        Resources parentResources;
        parentResources = pManager.getResourcesForApplication(getPackageName());

        // サブジェクト設定
        int subjectResID = parentResources.getIdentifier(USER_DEFINE_SUBJECT, "id", getPackageName());
        TextView subject = (TextView) findViewById(subjectResID);
        if (subject != null) {
            // 要素が定義されていた場合
            String subjectString = (String) getIntent().getExtras().get(INTENT_EXTRA_SUBJECT);
            subject.setText(subjectString);
        }

        // メッセージ指定
        int messageResID = parentResources.getIdentifier(USER_DEFINE_MESSAGE, "id", getPackageName());
        TextView message = (TextView) findViewById(messageResID);
        if (message != null) {
            // 要素が定義されていた場合
            String messageString = (String) getIntent().getExtras().get(INTENT_EXTRA_MESSAGE);
            message.setText(messageString);
        }

        // 閉じるボタン設定
        int closeButtonResID = parentResources.getIdentifier(USER_DEFINE_CLOSE_BUTTON, "id", getPackageName());
        Button closeButton = (Button) findViewById(closeButtonResID);
        if (closeButton != null) {
            // 要素が定義されていた場合
            closeButton.setOnClickListener(new OnClickListener() {
                @Override
                public void onClick(View v) {
                    finish();
                }
            });
        }

        // 表示ボタン設定
        int openButtonResID = parentResources.getIdentifier(USER_DEFINE_OPEN_BUTTON, "id", getPackageName());
        Button openButton = (Button) findViewById(openButtonResID);
        if (openButton != null) {
            openButton.setOnClickListener(new OnClickListener() {
                @Override
                public void onClick(View v) {
                    onClickOpenButton(v);
                }
            });
        }
    }

    // 小枠作成
    private LinearLayout createLinearLayout() {
        LinearLayout linearLayout = new LinearLayout(this);
        linearLayout.setOrientation(LinearLayout.VERTICAL);
        FrameLayout.LayoutParams linearParams = new FrameLayout.LayoutParams(convertDpToPixel(300), ViewGroup.LayoutParams.WRAP_CONTENT);
        linearParams.gravity = Gravity.CENTER_VERTICAL | Gravity.CENTER_HORIZONTAL;
        linearLayout.setOrientation(LinearLayout.VERTICAL);
        linearLayout.setLayoutParams(linearParams);
        return linearLayout;
    }

    // 枠線作成
    private GradientDrawable createFrameBorder() {
        GradientDrawable gradientDrawable = new GradientDrawable();
        gradientDrawable.setColor(Color.parseColor("#F0F0F0"));
        gradientDrawable.setStroke(convertDpToPixel(2), Color.parseColor("#CCCCCC"));
        gradientDrawable.setCornerRadius(8f);
        return gradientDrawable;
    }

    // タイトルとメッセージエリアの境界線
    private View createDividingLine() {
        View dividingLine = new View(this);
        ViewGroup.LayoutParams params = new LayoutParams();
        dividingLine.setLayoutParams(params);
        dividingLine.setBackgroundColor(Color.parseColor("#ff00bfff"));
        LinearLayout.LayoutParams dividingParams = new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, convertDpToPixel(2));
        dividingParams.setMargins(convertDpToPixel(6), 0, convertDpToPixel(6), 0);
        dividingLine.setLayoutParams(dividingParams);
        return dividingLine;
    }

    // 上側作成(アイコン,タイトルエリア)
    private LinearLayout createUpperLayout() {
        LinearLayout upperLayout = new LinearLayout(this);
        upperLayout.setOrientation(LinearLayout.HORIZONTAL);
        LinearLayout.LayoutParams upperParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        upperParams.gravity = Gravity.CENTER;
        upperLayout.setLayoutParams(upperParams);


        // アプリアイコンエリア取得
        ApplicationInfo appInfo;
        int icon=0;
        try {
            appInfo = getPackageManager().getApplicationInfo(getPackageName(), PackageManager.GET_META_DATA);
            icon = appInfo.icon;
        } catch (NameNotFoundException e) {
            throw new RuntimeException("Error Could not get app icon.");
        }

        //SmallIconの設定があれば設定する
        int userSmallIcon = appInfo.metaData.getInt(SMALL_ICON_KEY);
        if(userSmallIcon != 0){
            icon= userSmallIcon;
        }

        // アプリアイコン生成
        ImageView appIcon = new ImageView(this);
        //appIcon.setImageDrawable(icon);
        appIcon.setImageResource(icon);
        appIcon.setLayoutParams(new LinearLayout.LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
        appIcon.setPadding(convertDpToPixel(4), convertDpToPixel(4), convertDpToPixel(4), convertDpToPixel(4));

        // サブジェクトエリア作成
        TextView subject = new TextView(this);
        subject.setSingleLine();
        subject.setFocusableInTouchMode(true);
        subject.setEllipsize(TruncateAt.END);
        subject.setPadding(convertDpToPixel(8), convertDpToPixel(8), convertDpToPixel(8), convertDpToPixel(8));
        subject.setTextSize(TypedValue.COMPLEX_UNIT_DIP, 22);
        subject.setTextColor(Color.parseColor("#404040"));
        subject.setLayoutParams(new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, convertDpToPixel(45)));
        String subjectString = (String) getIntent().getExtras().get(INTENT_EXTRA_SUBJECT);
        subject.setText(subjectString);

        upperLayout.addView(appIcon);
        upperLayout.addView(subject);
        return upperLayout;
    }

    // 中間作成(メッセージエリア)
    private TextView createCenterLayout() {
        TextView message = new TextView(this);
        LinearLayout.LayoutParams messageLayout = new LinearLayout.LayoutParams(convertDpToPixel(300), convertDpToPixel(140), 1);
        if (displayType == NCMBDialogPushConfiguration.DIALOG_DISPLAY_BACKGROUND) {
            //背景指定
            messageLayout.setMargins(0, convertDpToPixel(48), 0, 0);//メッセージエリアを下げる
            message.setLayoutParams(messageLayout);
        } else {
            //標準
            message.setLayoutParams(messageLayout);
        }

        message.setPadding(convertDpToPixel(8), convertDpToPixel(8), convertDpToPixel(8), convertDpToPixel(8));
        message.setTextSize(TypedValue.COMPLEX_UNIT_SP, 20);
        message.setTextColor(Color.parseColor("#404040"));
        String messageString = (String) getIntent().getExtras().get(INTENT_EXTRA_MESSAGE);
        message.setText(messageString);
        message.setMovementMethod(ScrollingMovementMethod.getInstance());
        return message;
    }

    // 下側作成(閉じる,表示ボタンエリア)
    private LinearLayout createLowerLayout() {
        LinearLayout lowerLayout = new LinearLayout(this);
        lowerLayout.setLayoutParams(new LinearLayout.LayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.WRAP_CONTENT));
        lowerLayout.setOrientation(LinearLayout.HORIZONTAL);
        lowerLayout.setPadding(convertDpToPixel(4), convertDpToPixel(4), convertDpToPixel(4), convertDpToPixel(4));

        // Start a waiting timer for phone wakeup
        timerTask = new MyTimerTask();
        mTimer = new Timer(true);
        mTimer.schedule(timerTask, 5000);

        // ボタンのレイアウト作成
        final GradientDrawable buttonDrawable = new GradientDrawable(Orientation.TOP_BOTTOM, new int[]{Color.parseColor("#666666"), Color.parseColor("#333333")});
        buttonDrawable.setStroke(convertDpToPixel(2), Color.parseColor("#9F9F9F"));
        buttonDrawable.setCornerRadius(8f);

        // 閉じるボタン作成
        final Button closeButton = new Button(this);
        closeButton.setLayoutParams(new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1));
        closeButton.setTextColor(Color.WHITE);
        closeButton.setTextSize(TypedValue.COMPLEX_UNIT_SP, 16);
        closeButton.setHeight(convertDpToPixel(40));
        closeButton.setBackgroundDrawable(buttonDrawable);
        closeButton.setPadding(convertDpToPixel(2), convertDpToPixel(2), convertDpToPixel(2), convertDpToPixel(2));
        closeButton.setText("閉じる");

        closeButton.setOnFocusChangeListener(new OnFocusChangeListener() {
            @Override
            public void onFocusChange(View v, boolean hasFocus) {
                if (hasFocus) {
                    String a = "1";
                    closeButton.setText("閉じる" + a);
                } else {
                    String b = "2";
                    closeButton.setText("閉じる" + b);
                }
            }
        });

        closeButton.setOnTouchListener(new OnTouchListener() {
            @Override
            public boolean onTouch(View arg0, MotionEvent event) {
                if (event.getAction() == MotionEvent.ACTION_DOWN) {
                    GradientDrawable pushButtonDrawable = new GradientDrawable();
                    pushButtonDrawable.setColor(Color.CYAN);
                    pushButtonDrawable.setStroke(convertDpToPixel(2), Color.parseColor("#9F9F9F"));
                    pushButtonDrawable.setCornerRadius(8f);
                    closeButton.setBackgroundDrawable(pushButtonDrawable);
                } else {
                    closeButton.setBackgroundDrawable(buttonDrawable);
                }
                return false;
            }
        });
        closeButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });

        // ボタン間のスペース
        View betweenCloseToOpen = new View(this);
        LinearLayout.LayoutParams betweenCloseToOpenParams = new LinearLayout.LayoutParams(convertDpToPixel(8), convertDpToPixel(1));
        betweenCloseToOpen.setLayoutParams(betweenCloseToOpenParams);
        // android2.3ではsetAlpha()が使用できないため、以下で代用
        AlphaAnimation animation = new AlphaAnimation(0.0f, 0.0f);
        animation.setDuration(0);
        animation.setFillAfter(true);
        betweenCloseToOpen.startAnimation(animation);

        // 表示ボタン作成
        final Button openButton = new Button(this);
        openButton.setLayoutParams(new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WRAP_CONTENT, 1));
        openButton.setTextColor(Color.WHITE);
        openButton.setTextSize(TypedValue.COMPLEX_UNIT_SP, 16);
        openButton.setHeight(convertDpToPixel(40));
        openButton.setBackgroundDrawable(buttonDrawable);
        openButton.setPadding(convertDpToPixel(2), convertDpToPixel(2), convertDpToPixel(2), convertDpToPixel(2));
        openButton.setText("表示");
        openButton.setOnTouchListener(new OnTouchListener() {
            @Override
            public boolean onTouch(View arg0, MotionEvent event) {
                if (event.getAction() == MotionEvent.ACTION_DOWN) {
                    GradientDrawable pushButtonDrawable = new GradientDrawable();
                    pushButtonDrawable.setColor(Color.CYAN);
                    pushButtonDrawable.setStroke(convertDpToPixel(2), Color.parseColor("#9F9F9F"));
                    pushButtonDrawable.setCornerRadius(8f);
                    openButton.setBackgroundDrawable(pushButtonDrawable);
                } else {
                    openButton.setBackgroundDrawable(buttonDrawable);
                }
                return false;
            }
        });
        openButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                onClickOpenButton(v);
            }
        });

        // 小枠の下側に追加
        lowerLayout.addView(closeButton);
        lowerLayout.addView(betweenCloseToOpen);
        lowerLayout.addView(openButton);
        return lowerLayout;
    }

    // 表示ボタン設定
    private void onClickOpenButton(View v){
        finish();

        // Start specified activity
        String str = getIntent().getExtras().getString(INTENT_EXTRA_LAUNCH_CLASS);
        Intent launch = new Intent();
        launch.putExtras(getIntent().getBundleExtra("com.nifcloud.mbaas.OriginalData"));
        launch.setClassName(getApplicationContext(), "com.nifcloud.mbaas.ncmbfcmplugin.UnityPlayerActivity");
        launch.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        getApplicationContext().startActivity(launch);

        // Delete the dialog
        NotificationManager nm;
        nm = (NotificationManager) v.getContext().getSystemService(NOTIFICATION_SERVICE);
        nm.cancelAll();
    }
}
