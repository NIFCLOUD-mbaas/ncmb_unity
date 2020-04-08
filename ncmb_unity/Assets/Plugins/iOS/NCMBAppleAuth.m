
#import "NCMBAppleAuth.h"
#import "AppleAuthSerializer.h"

// IOS/TVOS 13.0 | MACOS 10.15
#if __IPHONE_OS_VERSION_MAX_ALLOWED >= 130000 || __TV_OS_VERSION_MAX_ALLOWED >= 130000 || __MAC_OS_X_VERSION_MAX_ALLOWED >= 101500
#define AUTHENTICATION_SERVICES_AVAILABLE true
#import <AuthenticationServices/AuthenticationServices.h>
#endif

@interface NCMBAppleAuth ()
@property (nonatomic, assign) NativeMessageHandlerDelegate mainCallback;
@property (nonatomic, weak) NSOperationQueue *callingOperationQueue;

- (void) sendNativeMessageForDictionary:(NSDictionary *)payloadDictionary forRequestId:(uint)requestId;
- (void) sendNativeMessageForString:(NSString *)payloadString forRequestId:(uint)requestId;
- (NSError *)internalErrorWithCode:(NSInteger)code andMessage:(NSString *)message;
- (void) sendsCredentialStatusInternalErrorWithCode:(NSInteger)code andMessage:(NSString *)message forRequestWithId:(uint)requestId;
- (void) sendsLoginResponseInternalErrorWithCode:(NSInteger)code andMessage:(NSString *)message forRequestWithId:(uint)requestId;
@end

#if AUTHENTICATION_SERVICES_AVAILABLE
API_AVAILABLE(ios(13.0), macos(10.15), tvos(13.0), watchos(6.0))
@interface NCMBAppleAuth () <ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
@property (nonatomic, strong) ASAuthorizationAppleIDProvider *appleIdProvider;
@property (nonatomic, strong) ASAuthorizationPasswordProvider *passwordProvider;
@property (nonatomic, strong) NSObject *credentialsRevokedObserver;
@property (nonatomic, strong) NSMutableDictionary<NSValue *, NSNumber *> *authorizationsInProgress;
@end
#endif

@implementation NCMBAppleAuth
+ (instancetype) sharedManager
{
    static NCMBAppleAuth *_defaultManager = nil;
    static dispatch_once_t defaultManagerInitialization;
    
    dispatch_once(&defaultManagerInitialization, ^{
        _defaultManager = [[NCMBAppleAuth alloc] init];
    });
    
    return _defaultManager;
}

- (instancetype) init
{
    self = [super init];
    if (self)
    {
#if AUTHENTICATION_SERVICES_AVAILABLE
        if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *))
        {
            _appleIdProvider = [[ASAuthorizationAppleIDProvider alloc] init];
            _passwordProvider = [[ASAuthorizationPasswordProvider alloc] init];
            _authorizationsInProgress = [NSMutableDictionary dictionary];
        }
#endif
    }
    return self;
}
- (void) loginWithAppleId:(uint)requestId withOptions:(AppleAuthManagerLoginOptions)options andNonce:(NSString *)nonce
{
#if AUTHENTICATION_SERVICES_AVAILABLE
    if (@available(iOS 13.0, tvOS 13.0, macOS 10.15, *))
    {
        ASAuthorizationAppleIDRequest *request = [[self appleIdProvider] createRequest];
        NSMutableArray *scopes = [NSMutableArray array];
        
        if (options & AppleAuthManagerIncludeName)
            [scopes addObject:ASAuthorizationScopeFullName];
            
        if (options & AppleAuthManagerIncludeEmail)
            [scopes addObject:ASAuthorizationScopeEmail];
        
        [request setRequestedScopes:[scopes copy]];
        [request setNonce:nonce];
        
        ASAuthorizationController *authorizationController = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
        [self performAuthorizationRequestsForController:authorizationController withRequestId:requestId];
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

- (void) performAuthorizationRequestsForController:(ASAuthorizationController *)authorizationController withRequestId:(uint)requestId
API_AVAILABLE(ios(13.0), macos(10.15), tvos(13.0), watchos(6.0))
{
    NSValue *authControllerAsKey = [NSValue valueWithNonretainedObject:authorizationController];
    [[self authorizationsInProgress] setObject:@(requestId) forKey:authControllerAsKey];
    
    [authorizationController setDelegate:self];
    [authorizationController setPresentationContextProvider:self];
    [authorizationController performRequests];
}

#pragma mark ASAuthorizationControllerDelegate protocol implementation

- (void) authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization
API_AVAILABLE(ios(13.0), macos(10.15), tvos(13.0), watchos(6.0))
{
    NSValue *authControllerAsKey = [NSValue valueWithNonretainedObject:controller];
    NSNumber *requestIdNumber = [[self authorizationsInProgress] objectForKey:authControllerAsKey];
    if (requestIdNumber)
    {
        NSDictionary *appleIdCredentialDictionary = nil;
        NSDictionary *passwordCredentialDictionary = nil;
        if ([[authorization credential] isKindOfClass:[ASAuthorizationAppleIDCredential class]])
        {
            appleIdCredentialDictionary = [AppleAuthSerializer dictionaryForASAuthorizationAppleIDCredential:(ASAuthorizationAppleIDCredential *)[authorization credential]];
        }
        else if ([[authorization credential] isKindOfClass:[ASPasswordCredential class]])
        {
            passwordCredentialDictionary = [AppleAuthSerializer dictionaryForASPasswordCredential:(ASPasswordCredential *)[authorization credential]];
        }

        NSDictionary *responseDictionary = [AppleAuthSerializer loginResponseDictionaryForAppleIdCredentialDictionary:appleIdCredentialDictionary
                                                                                      passwordCredentialDictionary:passwordCredentialDictionary
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
        NSDictionary *errorDictionary = [AppleAuthSerializer dictionaryForNSError:error];
        NSDictionary *responseDictionary = [AppleAuthSerializer loginResponseDictionaryForAppleIdCredentialDictionary:nil
                                                                                         passwordCredentialDictionary:nil
                                                                                                      errorDictionary:errorDictionary];
        
        [self sendNativeMessageForDictionary:responseDictionary forRequestId:[requestIdNumber unsignedIntValue]];
        
        [[self authorizationsInProgress] removeObjectForKey:authControllerAsKey];
    }
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
    [self sendNativeMessageForString:payloadString forRequestId:requestId];
}

- (void) sendNativeMessageForString:(NSString *)payloadString forRequestId:(uint)requestId
{
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

- (NSError *)internalErrorWithCode:(NSInteger)code andMessage:(NSString *)message
{
    return [NSError errorWithDomain:@"com.unity.AppleAuth"
                               code:code
                           userInfo:@{NSLocalizedDescriptionKey : message}];
}

- (void) sendsCredentialStatusInternalErrorWithCode:(NSInteger)code andMessage:(NSString *)message forRequestWithId:(uint)requestId
{
    NSError *customError = [self internalErrorWithCode:code andMessage:message];
    NSDictionary *customErrorDictionary = [AppleAuthSerializer dictionaryForNSError:customError];
    NSDictionary *responseDictionary = [AppleAuthSerializer credentialResponseDictionaryForCredentialState:nil
                                                                                           errorDictionary:customErrorDictionary];
    
    [self sendNativeMessageForDictionary:responseDictionary forRequestId:requestId];
}

- (void) sendsLoginResponseInternalErrorWithCode:(NSInteger)code andMessage:(NSString *)message forRequestWithId:(uint)requestId
{
    NSError *customError = [self internalErrorWithCode:code andMessage:message];
    NSDictionary *customErrorDictionary = [AppleAuthSerializer dictionaryForNSError:customError];
    NSDictionary *responseDictionary = [AppleAuthSerializer loginResponseDictionaryForAppleIdCredentialDictionary:nil
                                                                                     passwordCredentialDictionary:nil
                                                                                                  errorDictionary:customErrorDictionary];
    
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
void AppleAuth_IOS_LoginWithAppleId(uint requestId, int options, const char* _Nullable nonceCStr)
{
    NSString *nonce = nonceCStr != NULL ? [NSString stringWithUTF8String:nonceCStr] : nil;
    [[NCMBAppleAuth sharedManager] loginWithAppleId:requestId withOptions:options andNonce:nonce];
}
void AppleAuth_IOS_RegisterCredentialsRevokedCallbackId(uint requestId)
{
    [[NCMBAppleAuth sharedManager] registerCredentialsRevokedCallbackForRequestId:requestId];
}
