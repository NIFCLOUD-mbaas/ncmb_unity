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

This file incorporates work covered by the following copyright and  
  permission notice:  
     https://github.com/lupidan/apple-signin-unity
     Copyright (c) 2019 Daniel Lupiañez Casares

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software
  and associated documentation files (the "Software"), to deal in the Software without restriction, 
  including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
  IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
  ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

#import "NCMBAppleAuth.h"

// IOS/TVOS 13.0
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 130000 || __TV_OS_VERSION_MAX_ALLOWED >= 130000
#import <AuthenticationServices/AuthenticationServices.h>
#endif

@interface NCMBAppleAuth ()
@property (nonatomic, assign) CallbackDelegate callbackDelegate;
@property (nonatomic, weak) NSOperationQueue *operationQueue;
@end

API_AVAILABLE(ios(13.0), tvos(13.0))
@interface NCMBAppleAuth () <ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
@property (nonatomic, strong) NSMutableDictionary<NSValue *, NSNumber *> *authProgress;
@end

@implementation NCMBAppleAuth
+ (instancetype) sharedManager
{
    static NCMBAppleAuth *appleAuthManager = nil;
    static dispatch_once_t appleAuthManagerInit;
    
    dispatch_once(&appleAuthManagerInit, ^{
        appleAuthManager = [[NCMBAppleAuth alloc] init];
    });
    
    return appleAuthManager;
}

- (instancetype) init
{
    self = [super init];
    if (self)
    {
        if (@available(iOS 13.0, tvOS 13.0, *))
        {
            _authProgress = [NSMutableDictionary dictionary];
        }
    }
    return self;
}

- (void) loginWithAppleId:(uint)requestId
{
    if (@available(iOS 13.0, tvOS 13.0, *))
    {
        ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
        ASAuthorizationAppleIDRequest *request = [provider createRequest];
        [request setRequestedScopes: @[ASAuthorizationScopeEmail, ASAuthorizationScopeFullName]];
        ASAuthorizationController *controller = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
        NSValue *authControllerAsKey = [NSValue valueWithNonretainedObject:controller];
        [[self authProgress] setObject:@(requestId) forKey:authControllerAsKey];
        [controller setDelegate:self];
        [controller setPresentationContextProvider:self];
        [controller performRequests];
    }
    else
    {
        [self sendError:@"Do not support device version less than iOS 13.0"
              requestId:requestId];
    }

}

- (void) authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization
API_AVAILABLE(ios(13.0), tvos(13.0))
{
    NSValue *authKey = [NSValue valueWithNonretainedObject:controller];
    NSNumber *requestId = [[self authProgress] objectForKey:authKey];
    if (requestId)
    {
        NSDictionary *appleCredentialDic = nil;
        ASAuthorizationAppleIDCredential *appleIDCredential = authorization.credential;
        NSMutableDictionary *tmpAppleDic = [NSMutableDictionary dictionary];
        NSString *authorizationCode = [[NSString alloc] initWithData:appleIDCredential.authorizationCode encoding:NSUTF8StringEncoding];
        NSString *userId = appleIDCredential.user;
        [tmpAppleDic setValue:authorizationCode forKey: @"authorizationCode"];
        [tmpAppleDic setValue:userId forKey: @"userId"];
        appleCredentialDic = [tmpAppleDic copy];
        NSDictionary *responseDictionary = [self loginAppleIdResponseDictionary:appleCredentialDic
                                                                errorDic:nil];
        [self sendPayloadDictionary:responseDictionary requestId:[requestId unsignedIntValue]];
        [[self authProgress] removeObjectForKey:authKey];
    }
}

- (void) authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error
API_AVAILABLE(ios(13.0), tvos(13.0))
{
    NSValue *authKey = [NSValue valueWithNonretainedObject:controller];
    NSNumber *requestId = [[self authProgress] objectForKey:authKey];
    if (requestId)
    {
        NSMutableDictionary *tmpErrorDic = [NSMutableDictionary dictionary];
        [tmpErrorDic setValue:@([error code]) forKey:@"code"];
        [tmpErrorDic setValue:[error domain] forKey:@"domain"];
        [tmpErrorDic setValue:[error userInfo] forKey:@"userInfo"];
        NSDictionary *errorDic = [tmpErrorDic copy];
        NSDictionary *responseDictionary = [self loginAppleIdResponseDictionary:nil
                                                                errorDic:errorDic];
        [self sendPayloadDictionary:responseDictionary requestId:[requestId unsignedIntValue]];
        [[self authProgress] removeObjectForKey:authKey];
    }
}

- (NSDictionary *) loginAppleIdResponseDictionary:(NSDictionary *)appleCredentialDic
                                                         errorDic:(NSDictionary *)errorDic
{
    NSMutableDictionary *result = [[NSMutableDictionary alloc] init];
    [result setValue:@(appleCredentialDic != nil) forKey:@"isHasCredential"];
    [result setValue:@(errorDic != nil) forKey:@"isHasError"];
    [result setValue:appleCredentialDic forKey:@"credential"];
    [result setValue:errorDic forKey:@"error"];
    
    return [result copy];
}

- (void) sendPayloadDictionary:(NSDictionary *)payloadDictionary requestId:(uint)requestId
{
    NSError *error = nil;
    NSData *payloadData = [NSJSONSerialization dataWithJSONObject:payloadDictionary options:0 error:&error];
    NSString *payloadString = error ? [NSString stringWithFormat:@"Parse payload error: %@", [error localizedDescription]] : [[NSString alloc] initWithData:payloadData encoding:NSUTF8StringEncoding];
    [self sendPayloadString:payloadString forRequestId:requestId];
}

- (void) sendPayloadString:(NSString *)payloadString forRequestId:(uint)requestId
{
    if ([self callbackDelegate] == NULL)
        return;
    
    if ([self operationQueue])
    {
        [[self operationQueue] addOperationWithBlock:^{
            [self callbackDelegate](requestId, [payloadString UTF8String]);
        }];
    }
    else
    {
        [self callbackDelegate](requestId, [payloadString UTF8String]);
    }
}

- (void) sendError:(NSString *)message requestId:(uint)requestId
{
    NSMutableDictionary *tmpErrorDic = [NSMutableDictionary dictionary];
    [tmpErrorDic setValue:[NSNumber numberWithInt:-1] forKey:@"code"];
    [tmpErrorDic setValue:@"NCMBErrorDomain" forKey:@"domain"];
    [tmpErrorDic setValue:message forKey:@"userInfo"];
    NSDictionary *customErrorDic = [tmpErrorDic copy];
    NSDictionary *responseDictionary = [self loginAppleIdResponseDictionary:nil
                                                            errorDic:customErrorDic];
    [self sendPayloadDictionary:responseDictionary requestId:requestId];
}

- (ASPresentationAnchor) presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller
API_AVAILABLE(ios(13.0), tvos(13.0))
{
    return [[[UIApplication sharedApplication] delegate] window];
}
@end

void NCMBAppleAuth_HandlerCallback(CallbackDelegate callback)
{
    [[NCMBAppleAuth sharedManager] setCallbackDelegate:callback];
    [[NCMBAppleAuth sharedManager] setOperationQueue: [NSOperationQueue currentQueue]];
}

void NCMBAppleAuth_LoginWithAppleId(uint requestId)
{
    [[NCMBAppleAuth sharedManager] loginWithAppleId:requestId];
}
