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
#include "Unity/EAGLContextHelper.h"
#include "Unity/GlesHelper.h"
#include "PluginBase/AppDelegateListener.h"

// For Push
#include "NCMBRichPushView.h"
#include <stdio.h>

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
        if (NSFoundationVersionNumber > NSFoundationVersionNumber_iOS_7_1){
            UIUserNotificationType type = UIRemoteNotificationTypeAlert |
            UIRemoteNotificationTypeBadge |
            UIRemoteNotificationTypeSound;
            UIUserNotificationSettings *setting = [UIUserNotificationSettings settingsForTypes:type
                                                                                    categories:nil];
            [[UIApplication sharedApplication] registerUserNotificationSettings:setting];
            [[UIApplication sharedApplication] registerForRemoteNotifications];
        } else {
            [[UIApplication sharedApplication] registerForRemoteNotificationTypes:
             (UIRemoteNotificationTypeAlert |
              UIRemoteNotificationTypeBadge |
              UIRemoteNotificationTypeSound)];
        }
        
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
        if ([userInfo.allKeys containsObject:@"com.nifty.RichUrl"])
        {
            if ([[UIApplication sharedApplication] applicationState] != UIApplicationStateActive)
            {
                [NCMBRichPushView handleRichPush:userInfo];
            }
        }
        
        // NCMB Handle Analytics
        if ([userInfo.allKeys containsObject:@"com.nifty.PushId"])
        {
            if ([[UIApplication sharedApplication] applicationState] != UIApplicationStateActive)
            {
                NSString * pushId = [userInfo objectForKey:@"com.nifty.PushId"];
                const char *pushIdConstChar = [pushId UTF8String];
                notifyUnityWithClassName("NCMBManager","onAnalyticsReceived",pushIdConstChar);
            }
        }
        
        if([userInfo objectForKey:@"aps"]){
            if ([[(NSDictionary *)[userInfo objectForKey:@"aps"] objectForKey:@"sound"] isEqual:[NSNull null]]) {
                NSMutableDictionary *beforeUserInfo = [NSMutableDictionary dictionaryWithDictionary:userInfo];
                NSMutableDictionary *aps = [NSMutableDictionary dictionaryWithDictionary:[userInfo objectForKey:@"aps"]];
                [aps removeObjectForKey:@"sound"];
                [beforeUserInfo setObject:aps forKey:@"aps"];
                userInfo = (NSMutableDictionary *)beforeUserInfo;
            }
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
    NSMutableString *tokenId = [[NSMutableString alloc] initWithString:[NSString stringWithFormat:@"%@",deviceToken]];
    [tokenId setString:[tokenId stringByReplacingOccurrencesOfString:@" " withString:@""]]; //余計な文字を消す
    [tokenId setString:[tokenId stringByReplacingOccurrencesOfString:@"<" withString:@""]];
    [tokenId setString:[tokenId stringByReplacingOccurrencesOfString:@">" withString:@""]];
    const char * deviceTokenConstChar = [tokenId UTF8String];
    //Unityへデバイストークンを送り、UnityからmBaaS backendのinstallationクラスへ保存します
    notifyUnityWithClassName("NCMBManager", "onTokenReceived", deviceTokenConstChar);
}

- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error
{
    UnitySendRemoteNotificationError( error );
    
    notifyUnityError("OnRegistration", error);
}

- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo
{
    NCMBPushHandle(userInfo);
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler
{
    NCMBPushHandle(userInfo);
    if (handler)
    {
        handler(UIBackgroundFetchResultNoData);
    }
}


@end
