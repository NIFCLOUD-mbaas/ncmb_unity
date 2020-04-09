
#import <Foundation/Foundation.h>

NS_ASSUME_NONNULL_BEGIN

typedef void (*NativeMessageHandlerDelegate)(uint requestId,  const char* payload);

@interface NCMBAppleAuth : NSObject

+ (instancetype) sharedManager;

- (void) loginWithAppleId:(uint)requestId;

@end

//bool AppleAuth_IOS_IsCurrentPlatformSupported();
void AppleAuth_IOS_SetupNativeMessageHandlerCallback(NativeMessageHandlerDelegate callback);
void AppleAuth_IOS_LoginWithAppleId(uint requestId);
void AppleAuth_IOS_RegisterCredentialsRevokedCallbackId(uint requestId);
NS_ASSUME_NONNULL_END
