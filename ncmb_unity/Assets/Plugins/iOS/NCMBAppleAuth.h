//
//  NCMBAppleAuth.h
//  signinapple
//
//  Created by Jimmy Huynh on 4/7/20.
//  Copyright © 2020 Jimmy Huynh. All rights reserved.
//

#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef NS_OPTIONS(int, AppleAuthManagerLoginOptions) {
    AppleAuthManagerIncludeName = 1 << 0,
    AppleAuthManagerIncludeEmail = 1 << 1,
};

typedef void (*NativeMessageHandlerDelegate)(uint requestId,  const char* payload);

@interface NCMBAppleAuth : NSObject

+ (instancetype) sharedManager;

- (void) loginWithAppleId:(uint)requestId withOptions:(AppleAuthManagerLoginOptions)options andNonce:(NSString *)nonce;

@end

bool AppleAuth_IOS_IsCurrentPlatformSupported();
void AppleAuth_IOS_SetupNativeMessageHandlerCallback(NativeMessageHandlerDelegate callback);
void AppleAuth_IOS_LoginWithAppleId(uint requestId, int options, const char* _Nullable nonceCStr);
void AppleAuth_IOS_RegisterCredentialsRevokedCallbackId(uint requestId);
NS_ASSUME_NONNULL_END
