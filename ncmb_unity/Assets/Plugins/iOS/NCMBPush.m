/*
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
 */

#import "NCMBPush.h"
//#import "NCMBQuery.h"

//#import "NCMBObject+Private.h"
//#import "NCMBQuery+Private.h"
#import "NCMBRichPushView.h"

@interface NCMBPush ()

@property (nonatomic)NCMBQuery *query;

@end

@implementation NCMBPush

static NCMBRichPushView *rv;

+(NCMBQuery*)query{
}

+(instancetype)push{
}

-(instancetype)init{
    return self;
}

#pragma mark - handlilng

+(id)stringWithFormat:(NSString*)format arrayArguments:(NSArray*)argsArray{
    NSRange range = NSMakeRange(0, [argsArray count]);
    
    NSMutableData* data = [NSMutableData dataWithLength: sizeof(id) * [argsArray count]];
    
    [argsArray getObjects: (__unsafe_unretained id *)data.mutableBytes range:range];
    
    NSString *str =  [[NSString alloc] initWithFormat:format  arguments: data.mutableBytes];
    return str;
}

+ (void)handlePush:(NSDictionary *)userInfo{
    
    NSMutableDictionary *dicAps = [userInfo objectForKey:@"aps"];
    NSString *message = nil;
    NSString *cancelButtonTitle = @"Close";
    NSString *actionButtonTitle = nil;
    UIAlertView *alert = nil;
    UIAlertController *alertController = nil;
    if ([[dicAps objectForKey:@"alert"] isKindOfClass:[NSNull class]]) {
    }else if ([[dicAps objectForKey:@"alert"] isKindOfClass:[NSString class]]) {
        message = [dicAps objectForKey:@"alert"];
    }else{
        NSMutableDictionary *dicParams = [NSMutableDictionary dictionary];
        [dicParams setDictionary:[dicAps objectForKey:@"alert"]];
        if ([dicParams objectForKey:@"body"]) {
            message = [dicParams objectForKey:@"body"];
        }
        if ([dicParams objectForKey:@"loc-key"]) {
            message = [NCMBPush stringWithFormat:NSLocalizedString([dicParams objectForKey:@"loc-key"], @"loc-key") arrayArguments:[dicParams objectForKey:@"loc-args"]];
        }
        if ([dicParams objectForKey:@"action-loc-key"]) {
            actionButtonTitle = NSLocalizedString([dicParams objectForKey:@"action-loc-key"], @"action-loc-key");
        }
        alert = [[UIAlertView alloc] initWithTitle:nil message:message delegate:nil cancelButtonTitle:cancelButtonTitle otherButtonTitles:actionButtonTitle,nil];
    }
    
    if ([dicAps objectForKey:@"sound"]) {
        NSString *strSound = [dicAps objectForKey:@"sound"];
        if ([strSound isKindOfClass:[NSNull class]]) {
            
        }else{
            NSArray *ary = [strSound componentsSeparatedByString:@"."];
            if ([ary count]>2) {
                NSString *strSoundName = (NSString*)[ary objectAtIndex:0];
                NSString *strSoundType = (NSString*)[ary objectAtIndex:1];
                CFStringRef cfstr_sound_name = CFStringCreateWithCString(kCFAllocatorDefault, [strSoundName UTF8String],kCFStringEncodingUTF8 );
                CFStringRef cfstr_sound_type = CFStringCreateWithCString(kCFAllocatorDefault, [strSoundType UTF8String],kCFStringEncodingUTF8 );
                SystemSoundID mSound = 0;
                CFBundleRef mainBundle = CFBundleGetMainBundle();
                CFURLRef soundURL = CFBundleCopyResourceURL(mainBundle, cfstr_sound_name, cfstr_sound_type, NULL );
                AudioServicesCreateSystemSoundID( soundURL, &mSound );
                AudioServicesPlaySystemSound( mSound );
                CFRelease(soundURL);
                CFRelease(cfstr_sound_name);
                CFRelease(cfstr_sound_type);
            }
            
        }
    }
    if (![[dicAps objectForKey:@"badge"] isKindOfClass:[NSNull class]]) {
        
        if ([dicAps objectForKey:@"badge"]) {
            [UIApplication sharedApplication].applicationIconBadgeNumber= [[dicAps objectForKey:@"badge"] integerValue];
        }
    }
    
    if ([[UIDevice currentDevice].systemVersion floatValue] >= 8.0){
        UIViewController *baseView = [UIApplication sharedApplication].keyWindow.rootViewController;
        while (baseView.presentedViewController != nil && !baseView.presentedViewController.isBeingDismissed) {
            baseView = baseView.presentedViewController;
        }
        alertController = [UIAlertController alertControllerWithTitle:nil
                                                              message:message
                                                       preferredStyle:UIAlertControllerStyleAlert];
        [alertController addAction:[UIAlertAction actionWithTitle:cancelButtonTitle style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
            [baseView dismissViewControllerAnimated:YES completion:nil];
        }]];
        [baseView presentViewController:alertController animated:YES completion:nil];
    } else {
        alert = [[UIAlertView alloc] initWithTitle:nil
                                           message:message
                                          delegate:nil
                                 cancelButtonTitle:cancelButtonTitle
                                 otherButtonTitles:actionButtonTitle, nil];
        [alert show];
    }
}

+ (void) handleRichPush:(NSDictionary *)userInfo {
    NSString *urlStr = [userInfo objectForKey:@"com.nifty.RichUrl"];
    
    if ([urlStr isKindOfClass:[NSString class]]) {
        if (rv == nil){
            rv = [[NCMBRichPushView alloc]init];
            UIInterfaceOrientation orientation = [[UIApplication sharedApplication]statusBarOrientation];
            [rv appearWebView:orientation url:urlStr];
        }
        NSURL *url = [NSURL URLWithString:urlStr];
        NSURLRequest *req = [NSURLRequest requestWithURL:url cachePolicy:NSURLRequestReloadIgnoringLocalCacheData timeoutInterval:5];
        [rv loadRequest:req];
    }
}

+ (void) resetRichPushView {
    rv = nil;
}

#pragma mark - push notification configuration

- (void)setUserSettingValue:(NSDictionary *)userSettingValue{
    [self setObject:userSettingValue forKey:@"userSettingValue"];
}

- (void)setDialog:(BOOL)dialog{
    [self setObject:[NSNumber numberWithBool:dialog] forKey:@"dialog"];
}

- (void)setMessage:(NSString *)message{
    [self setObject:message forKey:@"message"];
}

- (void)setRichUrl:(NSString *)url{
    [self setObject:url forKey:@"richUrl"];
}

- (void)setTitle:(NSString *)title{
    [self setObject:title forKey:@"title"];
}

- (void)setAction:(NSString *)actionName{
    [self setObject:actionName forKey:@"action"];
}

- (void)setContentAvailable:(BOOL)contentAvailable{
    [self setObject:[NSNumber numberWithBool:contentAvailable] forKey:@"contentAvailable"];
    [self setObject:[NSNumber numberWithBool:NO] forKey:@"badgeIncrementFlag"];
    //[self removeObjectForKey:@"badgeIncrementFlag"];
}

- (void)setBadgeIncrementFlag:(BOOL)badgeIncrementFlag{
    [self setObject:[NSNumber numberWithBool:badgeIncrementFlag] forKey:@"badgeIncrementFlag"];
    [self setObject:[NSNumber numberWithBool:NO] forKey:@"contentAvailable"];
    //[self removeObjectForKey:@"contentAvailable"];
}

- (void)setSound:(NSString *)soundFileName{
    [self setObject:soundFileName forKey:@"sound"];
}

- (void)setCategory:(NSString *)category{
    [self setObject:category forKey:@"category"];
}


- (void)expireAtDate:(NSDate *)date{
    [self setObject:date forKey:@"deliveryExpirationDate"];
}

- (void)expireAfterTimeInterval:(NSString *)timeInterval{
    [self setObject:timeInterval forKey:@"deliveryExpirationTime"];
}

- (void)setData:(NSDictionary*)dic{
    for (NSString *key in [[dic allKeys] objectEnumerator]){
        if ([key isEqualToString:@"badgeSetting"]){
            [self setBadgeNumber:[[dic objectForKey:key] intValue]];
        } else if ([key isEqualToString:@"badgeIncrementFlag"]){
            [self setBadgeIncrementFlag:[[dic objectForKey:key] boolValue]];
        } else if ([key isEqualToString:@"contentAvailable"]){
            [self setContentAvailable:[[dic objectForKey:key] boolValue]];
        } else {
            [self setObject:[dic objectForKey:key] forKey:key];
        }
    }
}

#pragma mark - sendPush

- (void)sendPush:(NSError **)error{
}

- (void)sendPushInBackgroundWithTarget:(id)target selector:(SEL)selector{
}

#pragma mark - override

- (void)setObject:(id)object forKey:(NSString *)key{
    //既定フィールドの配列を作成
    NSArray *keys = @[@"deliveryTime",
                      @"immediateDeliveryFlag",
                      @"target",
                      @"searchCondition",
                      @"message",
                      @"userSettingValue",
                      @"deliveryExpirationDate",
                      @"deliveryExpirationTime",
                      @"action",
                      @"title",
                      @"dialog",
                      @"badgeIncrementFlag",
                      @"badgeSetting",
                      @"sound",
                      @"contentAvailable",
                      @"category",
                      @"richUrl",
                      @"acl"
                      ];
    if ([keys containsObject:key]){
    } else {
        //NCMBPushクラスは任意フィールドを設定できない
        [[NSException exceptionWithName:NSInternalInconsistencyException reason:@"UnsupportedOperation." userInfo:nil] raise];
    }
}

#pragma mark delegate



@end
