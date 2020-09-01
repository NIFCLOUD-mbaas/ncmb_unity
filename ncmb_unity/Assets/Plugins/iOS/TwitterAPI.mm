/*
 Copyright 2017-2020 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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

#import "TwitterAPI.h"

@implementation TwitterAPI
+ (instancetype) sharedManager
{
    static TwitterAPI *twitterAuthManager = nil;
    static dispatch_once_t twitterAuthManagerInit;
    
    dispatch_once(&twitterAuthManagerInit, ^{
        twitterAuthManager = [[TwitterAPI alloc] init];
    });
    
    return twitterAuthManager;
}

- (instancetype) init
{
    self = [super init];
    if (self) {
        UnityRegisterAppDelegateListener(self);
    }
    return self;
}

#pragma mark - Helpers

static char * serializedJSONFromNSDictionary(NSDictionary *dictionary)
{
    if (!dictionary) {
        return NULL;
    }
    
    NSData *serializedData = [NSJSONSerialization dataWithJSONObject:dictionary options:0 error:nil];
    NSString *serializedJSONString = [[NSString alloc] initWithData:serializedData encoding:NSUTF8StringEncoding];
    return cStringCopy([serializedJSONString UTF8String]);
}
static char * cStringCopy(const char *string)
{
    if (string == NULL)
        return NULL;
    
    char *res = (char *)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}
- (NSDictionary *)parametersDictionaryFromQueryString:(NSString *)queryString {

    NSMutableDictionary *md = [NSMutableDictionary dictionary];

    NSArray *queryComponents = [queryString componentsSeparatedByString:@"&"];

    for(NSString *s in queryComponents) {
        NSArray *pair = [s componentsSeparatedByString:@"="];
        if([pair count] != 2) continue;

        NSString *key = pair[0];
        NSString *value = pair[1];

        md[key] = value;
    }

    return md;
}

#pragma mark - Main method

-(void) loginTwitter:(NSString *) consumerConsumerKey consumerSecretConsumerKey:(NSString *) consumerSecretConsumerKey callbackScheme:(NSString *) callbackScheme {
     self.twitter = [STTwitterAPI twitterAPIWithOAuthConsumerKey: consumerConsumerKey consumerSecret:consumerSecretConsumerKey];
    self.callbackScheme = callbackScheme;
    NSLog(@"-- callbackScheme: %@", self.callbackScheme);
    
     [[TwitterAPI sharedManager].twitter postTokenRequest:^(NSURL *url, NSString *oauthToken) {
             [[UIApplication sharedApplication] openURL:url];
         } authenticateInsteadOfAuthorize:NO
                         forceLogin:@(YES)
                         screenName:nil
                        oauthCallback:[TwitterAPI sharedManager].callbackScheme
                         errorBlock:^(NSError *error) {
                             NSLog(@"-- error: %@", error);
         // Callback error
         NSMutableDictionary *tmpAppleDic = [NSMutableDictionary dictionary];
         [tmpAppleDic setObject:[NSNumber numberWithInt:error.code] forKey:@"code"];
         [tmpAppleDic setValue:error.localizedDescription forKey: @"message"];

         NSDictionary *responseDictionary = [tmpAppleDic copy];
         char *serializedSession = serializedJSONFromNSDictionary(responseDictionary);
         UnitySendMessage("NCMBTwitterObject", "LoginFailed", serializedSession);
         
     }];
}
 
#pragma mark - Callback when response scheme

- (void)onOpenURL:(NSNotification *)notification
{
    NSURL *url = notification.userInfo[@"url"];
    if ([[url scheme] isEqualToString:[TwitterAPI sharedManager].callbackScheme] != NO) {
        NSDictionary *d = [self parametersDictionaryFromQueryString:[url query]];
        NSString *token = d[@"oauth_token"];
        NSString *verifier = d[@"oauth_verifier"];
        
        [[TwitterAPI sharedManager].twitter postAccessTokenRequestWithPIN:verifier successBlock:^(NSString *oauthToken, NSString *oauthTokenSecret, NSString *userID, NSString *screenName) {
            
            // Callback login success
            NSMutableDictionary *tmpAppleDic = [NSMutableDictionary dictionary];
            [tmpAppleDic setValue:oauthToken forKey: @"token"];
            [tmpAppleDic setValue:oauthTokenSecret forKey: @"secret"];
            [tmpAppleDic setValue:screenName forKey: @"username"];
            [tmpAppleDic setValue:userID forKey: @"id"];
            NSDictionary *responseDictionary = [tmpAppleDic copy];
            char *serializedSession = serializedJSONFromNSDictionary(responseDictionary);
            UnitySendMessage("NCMBTwitterObject", "LoginComplete", serializedSession);
            
        } errorBlock:^(NSError *error) {
            // Callback error
            NSMutableDictionary *tmpAppleDic = [NSMutableDictionary dictionary];
            [tmpAppleDic setObject:[NSNumber numberWithInt:error.code] forKey:@"code"];
            [tmpAppleDic setValue:error.localizedDescription forKey: @"message"];

            NSDictionary *responseDictionary = [tmpAppleDic copy];
            char *serializedSession = serializedJSONFromNSDictionary(responseDictionary);
            UnitySendMessage("NCMBTwitterObject", "LoginFailed", serializedSession);
        }];
        
    }
}
@end

extern "C"
{
    void NCMB_LoginWithTwitter(char* consumerConsumerKey, char* consumerSecretConsumerKey, char* callbackScheme){
        [[TwitterAPI sharedManager] loginTwitter:[NSString stringWithUTF8String:consumerConsumerKey]
                       consumerSecretConsumerKey:[NSString stringWithUTF8String:consumerSecretConsumerKey] callbackScheme:[NSString stringWithUTF8String:callbackScheme]];
    }
}
