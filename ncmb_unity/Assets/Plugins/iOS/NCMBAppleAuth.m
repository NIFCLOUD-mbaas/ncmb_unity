
#import "NCMBAppleAuth.h"

// IOS/TVOS 13.0 | MACOS 10.15
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 130000 || __TV_OS_VERSION_MAX_ALLOWED >= 130000
#define AUTHENTICATION_SERVICES_AVAILABLE true
#import <AuthenticationServices/AuthenticationServices.h>
#endif

@interface NCMBAppleAuth ()
@property (nonatomic, assign) NativeMessageHandlerDelegate mainCallback;
@property (nonatomic, weak) NSOperationQueue *callingOperationQueue;

- (void) sendNativeMessageForDictionary:(NSDictionary *)payloadDictionary forRequestId:(uint)requestId;
- (void) sendNativeMessageForString:(NSString *)payloadString forRequestId:(uint)requestId;
- (void) sendsLoginResponseInternalErrorWithCode:(NSInteger)code andMessage:(NSString *)message forRequestWithId:(uint)requestId;
@end

#if AUTHENTICATION_SERVICES_AVAILABLE
API_AVAILABLE(ios(13.0), tvos(13.0))
@interface NCMBAppleAuth () <ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
@property (nonatomic, strong) NSObject *credentialsRevokedObserver;
@property (nonatomic, strong) NSMutableDictionary<NSValue *, NSNumber *> *authorizationsInProgress;
@end
#endif

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
#if AUTHENTICATION_SERVICES_AVAILABLE
        if (@available(iOS 13.0, tvOS 13.0, *))
        {
            _authorizationsInProgress = [NSMutableDictionary dictionary];
        }
#endif
    }
    return self;
}
- (void) loginWithAppleId:(uint)requestId
{
#if AUTHENTICATION_SERVICES_AVAILABLE
    if (@available(iOS 13.0, tvOS 13.0, *))
    {
        ASAuthorizationAppleIDProvider* provider = [[ASAuthorizationAppleIDProvider alloc] init];
        
        ASAuthorizationAppleIDRequest *request = [provider createRequest];
        [request setRequestedScopes: @[ASAuthorizationScopeEmail, ASAuthorizationScopeFullName]];
        
        ASAuthorizationController *controller = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
        NSValue *authControllerAsKey = [NSValue valueWithNonretainedObject:controller];
        [[self authorizationsInProgress] setObject:@(requestId) forKey:authControllerAsKey];
        
        [controller setDelegate:self];
        [controller setPresentationContextProvider:self];
        [controller performRequests];
    }
    else
    {
        [self sendsLoginResponseInternalErrorWithCode:-100
                                           andMessage:@"Native AppleAuth is only available from iOS 13.0"
                                     forRequestWithId:requestId];
    }
#else
    [self sendsLoginResponseInternalErrorWithCode:-100
                                       andMessage:@"Native AppleAuth is only available from iOS 13.0"
                                 forRequestWithId:requestId];
#endif
}
#if AUTHENTICATION_SERVICES_AVAILABLE


#pragma mark ASAuthorizationControllerDelegate protocol implementation

- (void) authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization
API_AVAILABLE(ios(13.0), macos(10.15), tvos(13.0), watchos(6.0))
{
    NSValue *authControllerAsKey = [NSValue valueWithNonretainedObject:controller];
    NSNumber *requestIdNumber = [[self authorizationsInProgress] objectForKey:authControllerAsKey];
    if (requestIdNumber)
    {
        NSDictionary *appleIdCredentialDictionary = nil;
        // My Code
        ASAuthorizationAppleIDCredential *appleIDCredential = authorization.credential;
        NSMutableDictionary *result = [NSMutableDictionary dictionary];
        
        NSString *authorizationCode = [[NSString alloc] initWithData:appleIDCredential.authorizationCode encoding:NSUTF8StringEncoding];
        NSString *userId = appleIDCredential.user;
        [result setValue:authorizationCode forKey: @"authorizationCode"];
        [result setValue:userId forKey: @"userId"];
        appleIdCredentialDictionary = [result copy];
        // My Code
//
//        NSMutableDictionary *result2 = [[NSMutableDictionary alloc] init];
//
//        [result2 setValue:appleIdCredentialDictionary forKey:@"appleCredential"];
//        [result2 setValue:[NSNull null] forKey:@"error"];
//        [result2 setValue:@(appleIdCredentialDictionary != nil) forKey:@"_hasAppleIdCredential"];
//
//        NSDictionary *responseDictionary =  [result2 copy];
//        NSLog(@"didCompleteWithAuthorization %@", responseDictionary);
        
        NSDictionary *responseDictionary = [self loginResponseDictionaryForAppleIdCredentialDictionary:appleIdCredentialDictionary
                                                                                                   errorDictionary:nil];
        
        [self sendNativeMessageForDictionary:responseDictionary forRequestId:[requestIdNumber unsignedIntValue]];
        
        [[self authorizationsInProgress] removeObjectForKey:authControllerAsKey];
    }
}

- (void) authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error
API_AVAILABLE(ios(13.0), macos(10.15), tvos(13.0), watchos(6.0))
{
    NSValue *authControllerAsKey = [NSValue valueWithNonretainedObject:controller];
    NSNumber *requestIdNumber = [[self authorizationsInProgress] objectForKey:authControllerAsKey];
    if (requestIdNumber)
    {
        NSMutableDictionary *result = [NSMutableDictionary dictionary];
        [result setValue:@([error code]) forKey:@"code"];
        [result setValue:[error domain] forKey:@"domain"];
        [result setValue:[error userInfo] forKey:@"userInfo"];
        
        NSDictionary *errorDictionary = [result copy];
        
        NSMutableDictionary *result2 = [[NSMutableDictionary alloc] init];
        
        [result2 setValue:[NSNull null] forKey:@"appleCredential"];
        [result2 setValue:errorDictionary forKey:@"error"];
        
        NSDictionary *responseDictionary =  [result2 copy];
        NSLog(@"didCompleteWithError %@", responseDictionary);
//        NSDictionary *responseDictionary = [AppleAuthSerializer loginResponseDictionaryForAppleIdCredentialDictionary:nil
//                                                                                                      errorDictionary:errorDictionary];
        
        [self sendNativeMessageForDictionary:responseDictionary forRequestId:[requestIdNumber unsignedIntValue]];
        
        [[self authorizationsInProgress] removeObjectForKey:authControllerAsKey];
    }
}

- (NSDictionary *) loginResponseDictionaryForAppleIdCredentialDictionary:(NSDictionary *)appleIdCredentialDictionary
                                                         errorDictionary:(NSDictionary *)errorDictionary
{
    NSMutableDictionary *result = [[NSMutableDictionary alloc] init];
    
//    [result setValue:@(errorDictionary == nil) forKey:@"_success"];
    [result setValue:@(appleIdCredentialDictionary != nil) forKey:@"_hasAppleIdCredential"];
    [result setValue:@(errorDictionary != nil) forKey:@"_hasError"];
    
    [result setValue:appleIdCredentialDictionary forKey:@"appleCredential"];
    [result setValue:errorDictionary forKey:@"error"];
    
    return [result copy];
}

- (void) registerCredentialsRevokedCallbackForRequestId:(uint)requestId
{
#if AUTHENTICATION_SERVICES_AVAILABLE
    if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *))
    {
        if ([self credentialsRevokedObserver])
        {
            [[NSNotificationCenter defaultCenter] removeObserver:[self credentialsRevokedObserver]];
            [self setCredentialsRevokedObserver:nil];
        }
        
        if (requestId != 0)
        {
            NSObject *observer = [[NSNotificationCenter defaultCenter] addObserverForName:ASAuthorizationAppleIDProviderCredentialRevokedNotification
                                                                               object:nil
                                                                                queue:nil
                                                                           usingBlock:^(NSNotification * _Nonnull note) {
                                                                               [self sendNativeMessageForString:@"Credentials Revoked" forRequestId:requestId];
                                                                           }];
            [self setCredentialsRevokedObserver:observer];
        }
    }
#endif
}
- (void) sendNativeMessageForDictionary:(NSDictionary *)payloadDictionary forRequestId:(uint)requestId
{
    NSError *error = nil;
    NSData *payloadData = [NSJSONSerialization dataWithJSONObject:payloadDictionary options:0 error:&error];
    NSString *payloadString = error ? [NSString stringWithFormat:@"Serialization error %@", [error localizedDescription]] : [[NSString alloc] initWithData:payloadData encoding:NSUTF8StringEncoding];
    NSLog(@"payloadString: %@", payloadString);
    [self sendNativeMessageForString:payloadString forRequestId:requestId];
}

- (void) sendNativeMessageForString:(NSString *)payloadString forRequestId:(uint)requestId
{
    NSLog(@"sendNativeMessageForString payloadString: %@", payloadString);
    if ([self mainCallback] == NULL)
        return;
    
    if ([self callingOperationQueue])
    {
        [[self callingOperationQueue] addOperationWithBlock:^{
            [self mainCallback](requestId, [payloadString UTF8String]);
        }];
    }
    else
    {
        [self mainCallback](requestId, [payloadString UTF8String]);
    }
}


- (void) sendsLoginResponseInternalErrorWithCode:(NSInteger)code andMessage:(NSString *)message forRequestWithId:(uint)requestId
{
    NSError *customError = [NSError errorWithDomain:@"com.unity.AppleAuth" code:code userInfo:@{NSLocalizedDescriptionKey : message}];
    NSMutableDictionary *result = [NSMutableDictionary dictionary];
    [result setValue:@([customError code]) forKey:@"code"];
    [result setValue:[customError domain] forKey:@"domain"];
    [result setValue:[customError userInfo] forKey:@"userInfo"];
    
    NSDictionary *customErrorDictionary = [result copy];
    
    NSMutableDictionary *result2 = [[NSMutableDictionary alloc] init];
    
    [result2 setValue:[NSNull null] forKey:@"appleCredential"];
    [result2 setValue:customErrorDictionary forKey:@"error"];
    
    NSDictionary *responseDictionary =  [result2 copy];
    NSLog(@"sendsLoginResponseInternalErrorWithCode %@", responseDictionary);
    
//    NSDictionary *responseDictionary = [AppleAuthSerializer loginResponseDictionaryForAppleIdCredentialDictionary:nil
//                                                                                                  errorDictionary:customErrorDictionary];
    
    [self sendNativeMessageForDictionary:responseDictionary forRequestId:requestId];
}

#pragma mark ASAuthorizationControllerPresentationContextProviding protocol implementation

- (ASPresentationAnchor) presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller
API_AVAILABLE(ios(13.0), macos(10.15), tvos(13.0), watchos(6.0))
{
    return [[[UIApplication sharedApplication] delegate] window];
}

#endif
@end

void AppleAuth_IOS_SetupNativeMessageHandlerCallback(NativeMessageHandlerDelegate callback)
{
    [[NCMBAppleAuth sharedManager] setMainCallback:callback];
    [[NCMBAppleAuth sharedManager] setCallingOperationQueue: [NSOperationQueue currentQueue]];
}

void AppleAuth_IOS_LoginWithAppleId(uint requestId)
{
    [[NCMBAppleAuth sharedManager] loginWithAppleId:requestId];
}

void AppleAuth_IOS_RegisterCredentialsRevokedCallbackId(uint requestId)
{
    [[NCMBAppleAuth sharedManager] registerCredentialsRevokedCallbackForRequestId:requestId];
}
