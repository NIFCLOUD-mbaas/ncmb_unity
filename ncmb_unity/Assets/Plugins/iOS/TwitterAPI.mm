//
//  TwitterAPI.m
//  demoTwitter
//
//  Created by Jimmy Huynh on 8/27/20.
//  Copyright © 2020 Jimmy Huynh. All rights reserved.
//

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
             NSLog(@"-- url: %@", url);
             NSLog(@"-- oauthToken: %@", oauthToken);
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
         [tmpAppleDic setValue:error.localizedFailureReason forKey: @"message"];

         NSDictionary *responseDictionary = [tmpAppleDic copy];
         char *serializedSession = serializedJSONFromNSDictionary(responseDictionary);
         UnitySendMessage("NCMBTwitterObject", "LoginFailed", serializedSession);
         
     }];
}
 
#pragma mark - Callback when response scheme

- (void)onOpenURL:(NSNotification *)notification
{
    NSURL *url = notification.userInfo[@"url"];
    if ([[url scheme] isEqualToString:@"myapp"] != NO) {
        NSDictionary *d = [self parametersDictionaryFromQueryString:[url query]];
        NSString *token = d[@"oauth_token"];
        NSString *verifier = d[@"oauth_verifier"];

        // Callback login success
        NSMutableDictionary *tmpAppleDic = [NSMutableDictionary dictionary];
        [tmpAppleDic setValue:token forKey: @"token"];
        [tmpAppleDic setValue:verifier forKey: @"secret"];
        NSDictionary *responseDictionary = [tmpAppleDic copy];
        char *serializedSession = serializedJSONFromNSDictionary(responseDictionary);
        UnitySendMessage("NCMBTwitterObject", "LoginComplete", serializedSession);
        
        
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
