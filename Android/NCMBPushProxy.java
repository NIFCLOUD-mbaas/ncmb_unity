/*******
 Copyright 2014 NIFTY Corporation All Rights Reserved.
 
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

package com.nifty.cloud.mb;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import com.unity3d.player.UnityPlayer;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;

@SuppressLint("SimpleDateFormat")
public class NCMBPushProxy 
{
	public static final String NS = "NCMB_SPLITTER";
	
	private static boolean isInited = false;
	private static boolean analytics = false;
	
	
	public static void onCreate(Bundle a) {

	}
	
	public static void onResume() {
		
		if (isInited) {
		    // Get Intent
		    Intent intent = UnityPlayer.currentActivity.getIntent();
		    
		    // Set analytics
		    if(analytics == true){
		    	//開封通知がtrueかつintentにpush.IDが入っていた場合のみ開封通知実行する
		    	NCMBAnalytics.trackAppOpened(intent);
		    }
		    // Process Intent
		    processIntent(intent);
		}
	}
	
	public static void onDestroy() {
		NCMBPush.closeRichPush();
	}
	
	public static void initialize(String _applicationKey, String _clientKey) {
		final String applicationKey = _applicationKey;
		final String clientKey = _clientKey;
		// Looper prepare
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				NCMB.initialize(UnityPlayer.currentActivity, applicationKey, clientKey);
			}
		});
	}
	
	public static void registerNotification(String _senderId,boolean _analytics) {
		// Sender Id
		final String senderId = _senderId;
		
		// analytics flag
		analytics = _analytics;
		
		// Looper prepare
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				NCMBInstallation installation = NCMBInstallation.getCurrentInstallation();
				installation.put("osVersion", Build.VERSION.SDK_INT);
				getRegistrationId(installation, senderId);
			}
		});
	}
	
	//_analytics未対応
	public static void registerNotificationWithLocation(String _senderId) {
		// Sender Id
		final String senderId = _senderId;
		
		// Looper prepare
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			  public void run() {
					NCMBGeoPoint.getCurrentLocationInBackground(120000, new LocationCallback() {
					    @Override
					    public void done(NCMBGeoPoint geo, NCMBException e) {
					        if(e == null) {
					        	// Notify
					        	notifyUnity("OnGetLocationSucceeded", geo.getLatitude() + " " + geo.getLongitude());
					    		// Put GeoPoint
					    		NCMBInstallation installation = NCMBInstallation.getCurrentInstallation();
					    		installation.put("osVersion", Build.VERSION.SDK_INT);
					    		installation.put("Point", geo);
					    		// Register id
					    		getRegistrationId(installation, senderId);
					        }
					        else {
					        	notifyUnity("OnGetLocationFailed", e.getMessage());
					        }
					    }
					});
			  }
			});
	}
	
	public static void subscribe(String _channel) {
		// Sender Id
		final String channel = _channel;
		// Looper prepare
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				  NCMBPush.subscribe(UnityPlayer.currentActivity, channel, UnityPlayer.currentActivity.getClass());
			}
		});
	}
	
	public static void sendPush(String _json, String _message, int _delayByMilliseconds, boolean _dialog) {
		// Sender Id
		final String json = _json;
		final String message = _message;
		final int delayByMilliseconds = _delayByMilliseconds;
		final boolean dialog = _dialog;
		// Looper prepare
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
			    NCMBPush push = new NCMBPush();
			    try {
				    JSONObject data = new JSONObject(json);
				    
				    //Push項目の設定
				   	settingPush(data, push, delayByMilliseconds, dialog);
 
				    push.setData(data);
				    push.setMessage(message);
				   				    
				    // Send in background
				    push.sendInBackground(new SendCallback() {
				        @Override
				        public void done(NCMBException ne) {
				            if (ne == null) {
				                notifyUnity("OnSendPush", "");
				            } else {
				                notifyUnity("OnSendPush", ne.getMessage());
				            }
				        }
				    });
			    } catch (JSONException je) {
		            notifyUnity("OnSendPush", je.getMessage());
			    } catch (ParseException pe) {
			    	notifyUnity("OnSendPush", pe.getMessage());
				}
			}
		});
	}
	
	public static void settingPush(JSONObject data, NCMBPush push ,int delayByMilliseconds ,boolean dialog) throws JSONException, ParseException
    {		
	    // Set dialog
	    if (dialog) {
	    	push.setDialog(dialog);
	    }
	    
	    if(!data.isNull ("contentAvailable")){
	    	boolean contentAvailable = data.getBoolean("contentAvailable");
	    	boolean targetAndroidOnly = false; 
	    	
			if (!data.isNull ("target")) {
				JSONArray array = data.getJSONArray("target");
				if(array.length() == 1){
					for(int i=0; i<1;i++){
						if(array.getString(i).equals("android")){
							targetAndroidOnly = true; 
						}
					}
				}
			}
			
	    	if(contentAvailable == true && targetAndroidOnly == false){
	    		//contentAvailableとincrementBadgeFlagは同時に設定出来ない
	    		//incrementBadgeFlagのデフォルトがtrueのため、falseを指定
	    		data.put("badgeIncrementFlag", false);
	    	}
	    }
	    
	    // Set richUrl
	    if(!data.isNull ("richUrl")){
	    	String richUrl = data.getString("richUrl");
	    	push.setRichUrl(richUrl);
	    	data.remove("richUrl");
	    }
        
	    // Send immediately or delay
	    if(!data.isNull ("DeliveryDate")){
			String dateString;
			dateString = data.getString("DeliveryDate");
			SimpleDateFormat sdf = new SimpleDateFormat("MM/dd/yyyy HH:mm:ss");
			Date date = sdf.parse(dateString);
			push.setDeliveryTime(date);
			data.remove("DeliveryDate");
			}
	    else if (delayByMilliseconds == 0) {
	    	push.setImmediateDeliveryFlag(true);
	    	}
	    else {
			Date date = new Date();
			date.setTime(date.getTime() + delayByMilliseconds);
			push.setDeliveryTime(date);
		}
    }
    
	
	private static void processIntent(Intent intent)
	{
		// Check null
		if (intent.getExtras() == null) {
			return;
		}
		
    	// Get data
        String pushId = intent.getExtras().getString("com.nifty.PushId");
        String data = intent.getExtras().getString("com.nifty.Data");
        String title = intent.getExtras().getString("com.nifty.Title");
        String message = intent.getExtras().getString("com.nifty.Message");
        String channel = intent.getExtras().getString("com.nifty.Channel");
        boolean dialog = intent.getExtras().getBoolean("com.nifty.Dialog");
        String richUrl = intent.getExtras().getString("com.nifty.RichUrl");
	    
        // Notify Unity
        if(pushId != null && !pushId.isEmpty()) {
        	// Notify
        	notifyUnity("OnNotificationReceived", pushId + NS + data + NS + title + NS + message + NS + channel + NS + dialog + NS + richUrl);
    	    // Rich Push Handler
        	if (!richUrl.isEmpty()) {
        		NCMBPush.richPushHandler(UnityPlayer.currentActivity, intent, false);
        	}
        	// Remove pay-load from intent
        	removePayload(intent);
        }
	}
	
	private static void removePayload(Intent intent)
	{
    	intent.removeExtra("com.nifty.PushId");
    	intent.removeExtra("com.nifty.Data");
    	intent.removeExtra("com.nifty.Title");
    	intent.removeExtra("com.nifty.Message");
    	intent.removeExtra("com.nifty.Channel");
    	intent.removeExtra("com.nifty.Dialog");
    	intent.removeExtra("com.nifty.RichUrl");
	}
	
	private static void afterLaunch() {
		isInited = true;
		
		UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
			public void run() {
				Intent intent = UnityPlayer.currentActivity.getIntent();
				if (intent != null && intent.getExtras() != null) {
					NCMBAnalytics.trackAppOpened(intent);
					processIntent(intent);
				}
			}
		});
	}
	
	private static void getRegistrationId(NCMBInstallation _installation, String _senderId)
	{
		final NCMBInstallation installation = _installation;
		final String senderId = _senderId;
		
		installation.getRegistrationIdInBackground(senderId, new RegistrationCallback() {
		    @Override
		    public void done(NCMBException e) {
		        if (e == null) {
		            try {
		            	// Save
		                installation.save();
		                // Notify Unity
		                notifyRegistrationSucceeded();
		                // Process Saved Intent
		                afterLaunch();
		            } catch (NCMBException le) {
		            	// Case of re-install
		                if (NCMBException.DUPLICATE_VALUE.equals(le.getCode())) {
		                    NCMBQuery<NCMBInstallation> query = NCMBInstallation.getQuery();
		                    query.whereEqualTo("deviceToken", installation.get("deviceToken"));
		                    try {
		                        NCMBInstallation prevInstallation = query.getFirst();
		                        String objectId = prevInstallation.getObjectId();
		                        installation.setObjectId(objectId);
		                        // Save
		                        installation.save();
				                // Notify Unity
				                notifyRegistrationSucceeded();
				                // Process Saved Intent
				                afterLaunch();
		                    } catch(NCMBException le2) {
		                    	notifyRegistrationFailed(le2.getMessage());
		                    }
		                } else {
		                	notifyRegistrationFailed(le.getMessage());
		                }
		            }
		        } else {
		        	notifyRegistrationFailed(e.getMessage());
		        }
		    }
		});
		
		// Default Push Callback
		NCMBPush.setDefaultPushCallback(UnityPlayer.currentActivity, UnityPlayer.currentActivity.getClass());
	}
	
	private static void notifyUnity(String method, String message) {
        // Notify Unity
        UnityPlayer.UnitySendMessage("NCMBManager", method, message);
        // Log
        Log.v("NCMB", method + ":" + message);
	}
	
	private static void notifyRegistrationSucceeded() {
		notifyUnity("OnRegistration", "");
	}
	
	private static void notifyRegistrationFailed(String message) {
		notifyUnity("OnRegistration", message);
	}
}
