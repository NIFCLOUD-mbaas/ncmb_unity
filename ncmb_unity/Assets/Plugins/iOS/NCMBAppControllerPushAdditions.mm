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
 **********/

// For UnityAppController
#import "UnityAppController.h"
#import "UnityAppController+ViewHandling.h"
#import "UnityAppController+Rendering.h"
#import "iPhone_Sensors.h"

#import <CoreGraphics/CoreGraphics.h>
#import <QuartzCore/QuartzCore.h>
#import <QuartzCore/CADisplayLink.h>
#import <UIKit/UIKit.h>
#import <Availability.h>

#import <OpenGLES/EAGL.h>
#import <OpenGLES/EAGLDrawable.h>
#import <OpenGLES/ES2/gl.h>
#import <OpenGLES/ES2/glext.h>

#include "CrashReporter.h"
#if UNITY_VERSION >= 500
#include "Classes/UI/OrientationSupport.h"
#include "Unity/InternalProfiler.h"
#else
#include "iPhone_OrientationSupport.h"
#include "iPhone_Profiler.h"
#endif

#include "UI/Keyboard.h"
#include "UI/UnityView.h"
#include "UI/SplashScreen.h"
#include "Unity/DisplayManager.h"
#include "PluginBase/AppDelegateListener.h"

// For Push
#include "NCMBRichPushView.h"
#include <stdio.h>
#if __has_include(<UserNotifications/UserNotifications.h>)
#import  <UserNotifications/UserNotifications.h>
#endif

#define MAX_PUSH_DELAY_COUNT 20
// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Unity default methods
void UnitySendMessage( const char * className, const char * methodName, const char * param );
void UnitySendDeviceToken( NSData* deviceToken );
void UnitySendRemoteNotification( NSDictionary* notification );
void UnitySendRemoteNotificationError( NSError* error );
void UnitySendLocalNotification( UILocalNotification* notification );

// Notify & Log methods
void notifyUnityWithClassName(const char * objectName, const char * method, const char * message)
{
    UnitySendMessage(objectName, method, message);
    //NSLog(@"[NCMB] %s : %s", method, message);
}

// Notify & Log methods
void notifyUnity(const char * method, const char * message)
{
    UnitySendMessage("NCMBManager", method, message);
    //NSLog(@"[NCMB] %s : %s", method, message);
}

void notifyUnityError(const char * method, NSError * error)
{
    const char* message = [[error description] UTF8String];
    notifyUnity(method, message);
    //NSLog(@"[NCMB] %s : %s", method, message);
}

#pragma mark - C#から呼び出し
extern bool _unityAppReady;

// Native code
extern "C"
{
    
    // Use location or not
    bool getLocation;
    bool useAnalytics;
    
    // Save launch options for using later (after set key)
    NSDictionary * savedLaunchOptions;
    
    void registerCommon()
    {
        if ([[NSProcessInfo processInfo] isOperatingSystemAtLeastVersion:(NSOperatingSystemVersion){8, 0, 0}]){
            
            //iOS10未満での、DeviceToken要求方法
            
            //通知のタイプを設定したsettingを用意
            UIUserNotificationType type = UIUserNotificationTypeAlert |
            UIUserNotificationTypeBadge |
            UIUserNotificationTypeSound;
            UIUserNotificationSettings *setting;
            setting = [UIUserNotificationSettings settingsForTypes:type
                                                        categories:nil];
            
            //通知のタイプを設定
            [[UIApplication sharedApplication] registerUserNotificationSettings:setting];
            
            //DeviceTokenを要求
            [[UIApplication sharedApplication] registerForRemoteNotifications];
        } else {
            
            //iOS8未満での、DeviceToken要求方法
            [[UIApplication sharedApplication] registerForRemoteNotificationTypes:
             (UIRemoteNotificationTypeAlert |
              UIRemoteNotificationTypeBadge |
              UIRemoteNotificationTypeSound)];
        }
        
        #if __has_include(<UserNotifications/UserNotifications.h>)
        if ([[NSProcessInfo processInfo] isOperatingSystemAtLeastVersion:(NSOperatingSystemVersion){10, 0, 0}]){
            
            //iOS10以上での、DeviceToken要求方法
            UNUserNotificationCenter *center = [UNUserNotificationCenter currentNotificationCenter];
            [center requestAuthorizationWithOptions:(UNAuthorizationOptionAlert |
                                                     UNAuthorizationOptionBadge |
                                                     UNAuthorizationOptionSound)
                                  completionHandler:^(BOOL granted, NSError * _Nullable error) {
                                      if (error) {
                                          return;
                                      }
                                      if (granted) {
                                          //通知を許可にした場合DeviceTokenを要求
                                          [[UIApplication sharedApplication] registerForRemoteNotifications];
                                      }
                                  }];
            
        }
        #endif
    }
    
    void registerNotification(BOOL _useAnalytics)
    {
        useAnalytics = _useAnalytics;
        getLocation = false;
        registerCommon();
    }
    
    void registerNotificationWithLocation()
    {
        getLocation = true;
        registerCommon();
    }
    
    void clearAll()
    {
        [[UIApplication sharedApplication] setApplicationIconBadgeNumber: 0];
    }
    
    // installationのプロパティを生成して返却
    char* getInstallationProperty()
    {
        //プロパティを生成
        NSMutableDictionary *dic = [NSMutableDictionary dictionary];
        [dic setObject:[[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleName"] forKey:@"applicationName"];
        [dic setObject:[[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleVersion"] forKey:@"appVersion"];
        [dic setObject:@"ios" forKey:@"deviceType"];
        [dic setObject:[[NSTimeZone systemTimeZone] name] forKey:@"timeZone"];
        //JSON文字列に変換
        NSData* data=[NSJSONSerialization dataWithJSONObject:dic options:2 error:nil];
        NSString* jsonstr=[[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
        char* res = (char*)malloc(strlen([jsonstr UTF8String]) + 1);
        strcpy(res, [jsonstr UTF8String]);
        return res;
    }
    
    void NCMBPushHandle(NSDictionary *userInfo)
    {
        // NCMB Handle Rich Push
        if ([userInfo.allKeys containsObject:@"com.nifcloud.mbaas.RichUrl"])
        {
            [NCMBRichPushView handleRichPush:userInfo];
        }
        
        // NCMB Handle Analytics
        if ([userInfo.allKeys containsObject:@"com.nifcloud.mbaas.PushId"])
        {
            if ([[UIApplication sharedApplication] applicationState] != UIApplicationStateActive)
            {
                NSString * pushId = [userInfo objectForKey:@"com.nifcloud.mbaas.PushId"];
                const char *pushIdConstChar = [pushId UTF8String];
                notifyUnityWithClassName("NCMBManager","onAnalyticsReceived",pushIdConstChar);
            }
        }
        
        if([userInfo objectForKey:@"aps"]){
            NSMutableDictionary *beforeUserInfo = [NSMutableDictionary dictionaryWithDictionary:userInfo];
            NSMutableDictionary *aps = [NSMutableDictionary dictionaryWithDictionary:[userInfo objectForKey:@"aps"]];
            [beforeUserInfo setObject:aps forKey:@"aps"];
            if([[aps objectForKey:@"alert"] objectForKey:@"title"]){
                [beforeUserInfo setObject:[[aps objectForKey:@"alert"] objectForKey:@"title"] forKey:@"com.nifcloud.mbaas.Title"]; //Titleを追加
            }
            if([[aps objectForKey:@"alert"] objectForKey:@"body"]){
                [beforeUserInfo setObject:[[aps objectForKey:@"alert"] objectForKey:@"body"] forKey:@"com.nifcloud.mbaas.Message"]; //Messageを追加
            }
            userInfo = (NSMutableDictionary *)beforeUserInfo;
        }
        
        AppController_SendNotificationWithArg(kUnityDidReceiveRemoteNotification, userInfo);
        UnitySendRemoteNotification(userInfo);//userInfoの値にNullは許容しない
    }
    
}

// Implementation
#if UNITY_VERSION < 420

@implementation AppController(PushAdditions)

#else

@implementation UnityAppController(PushAdditions)

NSInteger pushDelayCount = 0;
#endif

///////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIApplicationDelegate

/*
 - (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
 {
 savedLaunchOptions = [launchOptions copy];
 
 printf_console("-> applicationDidFinishLaunching()\n");
 // get local notification
 if (&UIApplicationLaunchOptionsLocalNotificationKey != nil)
 {
 UILocalNotification *notification = [launchOptions objectForKey:UIApplicationLaunchOptionsLocalNotificationKey];
 if (notification)
 UnitySendLocalNotification(notification);
 }
 
 // get remote notification
 if (&UIApplicationLaunchOptionsRemoteNotificationKey != nil)
 {
 NSDictionary *notification = [launchOptions objectForKey:UIApplicationLaunchOptionsRemoteNotificationKey];
 if (notification)
 UnitySendRemoteNotification(notification);
 }
 
 if ([UIDevice currentDevice].generatesDeviceOrientationNotifications == NO)
 [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
 
 [DisplayManager Initialize];
 
 _mainDisplay	= [[[DisplayManager Instance] mainDisplay] createView:YES showRightAway:NO];
 _window			= _mainDisplay->window;
 
 [KeyboardDelegate Initialize];
 
 [self createViewHierarchy];
 [self preStartUnity];
 UnityInitApplicationNoGraphics([[[NSBundle mainBundle] bundlePath]UTF8String]);
 
 return YES;
 }
 */

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
    //deviceTokenをNSData *からconst char *へ変換します
    if ([deviceToken isKindOfClass:[NSData class]] && [deviceToken length] != 0){
        unsigned char *dataBuffer = (unsigned char*)deviceToken.bytes;
        NSMutableString *tokenId = [NSMutableString stringWithCapacity:(deviceToken.length * 2)];
        for (int i = 0; i < deviceToken.length; ++i) {
            [tokenId appendFormat:@"%02x", dataBuffer[i]];
        }
        const char * deviceTokenConstChar = [tokenId UTF8String];
        //Unityへデバイストークンを送り、UnityからmBaaS backendのinstallationクラスへ保存します
        notifyUnityWithClassName("NCMBManager", "onTokenReceived", deviceTokenConstChar);
    } else {
        NSLog(@"[NCMB]: 不正なデバイストークのため、端末登録を行いません");
    }
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    UnitySendRemoteNotificationError( error );
    
    notifyUnityError("OnRegistration", error);
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    [self handleRichPushIfReady:userInfo];
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler
{
    [self handleRichPushIfReady:userInfo];
    if (handler)
    {
        handler(UIBackgroundFetchResultNoData);
    }
}

-(void)handleRichPushIfReady:(NSDictionary*)userInfo
{
    //Limit to avoid infinite loop
    if(pushDelayCount < MAX_PUSH_DELAY_COUNT){
        if(_unityAppReady){
            //Remove null values (<null>) to avoid crash
            NSDictionary *notificationInfo = [self removeNullObjects:userInfo];
            NCMBPushHandle(notificationInfo);
            pushDelayCount = 0;
        } else {
            pushDelayCount++;
            //Delay for 100 miliseconds
            [self performSelector:@selector(handleRichPushIfReady:) withObject:userInfo afterDelay:0.1];
        }
    }
}

-(NSDictionary*) removeNullObjects:(NSDictionary*) dictionary {
    NSMutableDictionary *dict = [dictionary mutableCopy];
    NSArray *keysForNullValues = [dict allKeysForObject:[NSNull null]];
    [dict removeObjectsForKeys:keysForNullValues];
    for(NSString *key in dict.allKeys){
        if ([[dict valueForKey:key] isKindOfClass:[NSDictionary class]]){
            NSDictionary *childDict = [self removeNullObjects:[dict valueForKey:key]];
            if([childDict allKeys].count > 0){
                [dict setObject:childDict forKey:key];
            } else {
                [dict removeObjectForKey:key];
            }
        }
    }
    return dict;
}

@end
