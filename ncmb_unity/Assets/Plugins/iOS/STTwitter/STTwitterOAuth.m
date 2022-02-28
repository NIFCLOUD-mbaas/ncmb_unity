//
//  STTwitterRequest.m
//  STTwitterRequests
//
//  Created by Nicolas Seriot on 9/5/12.
//  Copyright (c) 2012 Nicolas Seriot. All rights reserved.
//

#import "STTwitterOAuth.h"
#import "STHTTPRequest.h"
#import "NSString+STTwitter.h"
#import "STHTTPRequest+STTwitter.h"

#include <CommonCrypto/CommonHMAC.h>

#if DEBUG
#   define STLog(...) NSLog(__VA_ARGS__)
#else
#   define STLog(...)
#endif

@interface NSData (Base64)
- (NSString *)base64Encoding; // private API
@end

@interface STTwitterOAuth ()

@property (nonatomic, retain) NSString *username;
@property (nonatomic, retain) NSString *password;

@property (nonatomic, retain) NSString *oauthConsumerName;
@property (nonatomic, retain) NSString *oauthConsumerKey;
@property (nonatomic, retain) NSString *oauthConsumerSecret;

@property (nonatomic, retain) NSString *oauthRequestToken;
@property (nonatomic, retain) NSString *oauthRequestTokenSecret;

@property (nonatomic, retain) NSString *oauthAccessToken;
@property (nonatomic, retain) NSString *oauthAccessTokenSecret;

@property (nonatomic, retain) NSString *testOauthNonce;
@property (nonatomic, retain) NSString *testOauthTimestamp;

@end

@implementation STTwitterOAuth

+ (instancetype)twitterOAuthWithConsumerName:(NSString *)consumerName
                                 consumerKey:(NSString *)consumerKey
                              consumerSecret:(NSString *)consumerSecret {
    
    STTwitterOAuth *to = [[STTwitterOAuth alloc] init];
    
    to.oauthConsumerName = consumerName;
    to.oauthConsumerKey = consumerKey;
    to.oauthConsumerSecret = consumerSecret;
    
    return to;
}

+ (instancetype)twitterOAuthWithConsumerName:(NSString *)consumerName
                                 consumerKey:(NSString *)consumerKey
                              consumerSecret:(NSString *)consumerSecret
                                  oauthToken:(NSString *)oauthToken
                            oauthTokenSecret:(NSString *)oauthTokenSecret {
    
    STTwitterOAuth *to = [self twitterOAuthWithConsumerName:consumerName consumerKey:consumerKey consumerSecret:consumerSecret];
    
    to.oauthAccessToken = oauthToken;
    to.oauthAccessTokenSecret = oauthTokenSecret;
    
    return to;
}

+ (instancetype)twitterOAuthWithConsumerName:(NSString *)consumerName
                                 consumerKey:(NSString *)consumerKey
                              consumerSecret:(NSString *)consumerSecret
                                    username:(NSString *)username
                                    password:(NSString *)password {
    
    STTwitterOAuth *to = [self twitterOAuthWithConsumerName:consumerName consumerKey:consumerKey consumerSecret:consumerSecret];
    
    to.username = username;
    to.password = password;
    
    return to;
}

+ (NSArray *)encodedParametersDictionaries:(NSArray *)parameters {
    
    NSMutableArray *encodedParameters = [NSMutableArray array];
    
    for(NSDictionary *d in parameters) {
        
        NSString *key = [[d allKeys] lastObject];
        NSString *value = [[d allValues] lastObject];
        
        NSString *encodedKey = [key st_urlEncodedString];
        NSString *encodedValue = [value st_urlEncodedString];
        
        [encodedParameters addObject:@{encodedKey : encodedValue}];
    }
    
    return encodedParameters;
}

+ (NSString *)stringFromParametersDictionaries:(NSArray *)parametersDictionaries {
    
    NSMutableArray *parameters = [NSMutableArray array];
    
    for(NSDictionary *d in parametersDictionaries) {
        
        NSString *encodedKey = [[d allKeys] lastObject];
        NSString *encodedValue = [[d allValues] lastObject];
        
        NSString *s = [NSString stringWithFormat:@"%@=\"%@\"", encodedKey, encodedValue];
        
        [parameters addObject:s];
    }
    
    return [parameters componentsJoinedByString:@", "];
}

+ (NSString *)oauthHeaderValueWithParameters:(NSArray *)parametersDictionaries {
    
    NSArray *encodedParametersDictionaries = [self encodedParametersDictionaries:parametersDictionaries];
    
    NSString *encodedParametersString = [self stringFromParametersDictionaries:encodedParametersDictionaries];
    
    NSString *headerValue = [NSString stringWithFormat:@"OAuth %@", encodedParametersString];
    
    return headerValue;
}

+ (NSArray *)parametersDictionariesSortedByKey:(NSArray *)parametersDictionaries {
    
    return [parametersDictionaries sortedArrayUsingComparator:^NSComparisonResult(id obj1, id obj2) {
        NSDictionary *d1 = (NSDictionary *)obj1;
        NSDictionary *d2 = (NSDictionary *)obj2;
        
        NSString *key1 = [[d1 allKeys] lastObject];
        NSString *key2 = [[d2 allKeys] lastObject];
        
        return [key1 compare:key2];
    }];
    
}

- (NSString *)consumerName {
    return _oauthConsumerName;
}

- (NSString *)loginTypeDescription {
    return @"OAuth";
}

- (NSString *)oauthNonce {
    if(_testOauthNonce) return _testOauthNonce;
    
    return [NSString st_random32Characters];
}

+ (NSString *)signatureBaseStringWithHTTPMethod:(NSString *)httpMethod url:(NSURL *)url allParametersUnsorted:(NSArray *)parameters {
    NSMutableArray *allParameters = [NSMutableArray arrayWithArray:parameters];
    
    NSArray *encodedParametersDictionaries = [self encodedParametersDictionaries:allParameters];
    
    NSArray *sortedEncodedParametersDictionaries = [self parametersDictionariesSortedByKey:encodedParametersDictionaries];
    
    /**/
    
    NSMutableArray *encodedParameters = [NSMutableArray array];
    
    for(NSDictionary *d in sortedEncodedParametersDictionaries) {
        NSString *encodedKey = [[d allKeys] lastObject];
        NSString *encodedValue = [[d allValues] lastObject];
        
        NSString *s = [NSString stringWithFormat:@"%@=%@", encodedKey, encodedValue];
        
        [encodedParameters addObject:s];
    }
    
    NSString *encodedParametersString = [encodedParameters componentsJoinedByString:@"&"];
    
    NSString *signatureBaseString = [NSString stringWithFormat:@"%@&%@&%@",
                                     [httpMethod uppercaseString],
                                     [[url st_normalizedForOauthSignatureString] st_urlEncodedString],
                                     [encodedParametersString st_urlEncodedString]];
    
    return signatureBaseString;
}

+ (NSString *)oauthSignatureWithHTTPMethod:(NSString *)httpMethod url:(NSURL *)url parameters:(NSArray *)parameters consumerSecret:(NSString *)consumerSecret tokenSecret:(NSString *)tokenSecret {
    /*
     The oauth_signature parameter contains a value which is generated by running all of the other request parameters and two secret values through a signing algorithm. The purpose of the signature is so that Twitter can verify that the request has not been modified in transit, verify the application sending the request, and verify that the application has authorization to interact with the user's account.
     https://dev.twitter.com/docs/auth/creating-signature
     */
    
    NSString *signatureBaseString = [[self class] signatureBaseStringWithHTTPMethod:httpMethod url:url allParametersUnsorted:parameters];
    
    /*
     Note that there are some flows, such as when obtaining a request token, where the token secret is not yet known. In this case, the signing key should consist of the percent encoded consumer secret followed by an ampersand character '&'.
     */
    
    NSString *encodedConsumerSecret = [consumerSecret st_urlEncodedString];
    NSString *encodedTokenSecret = [tokenSecret st_urlEncodedString];
    
    NSString *signingKey = [NSString stringWithFormat:@"%@&", encodedConsumerSecret];
    
    if(encodedTokenSecret) {
        signingKey = [signingKey stringByAppendingString:encodedTokenSecret];
    }
    
    NSString *oauthSignature = [signatureBaseString st_signHmacSHA1WithKey:signingKey];
    
    return oauthSignature;
}

- (void)verifyCredentialsLocallyWithSuccessBlock:(void(^)(NSString *username, NSString *userID))successBlock
                                      errorBlock:(void(^)(NSError *error))errorBlock {
    successBlock(nil, nil); // no local check
}

- (void)verifyCredentialsRemotelyWithSuccessBlock:(void(^)(NSString *username, NSString *userID))successBlock
                                       errorBlock:(void(^)(NSError *error))errorBlock {
    
    if(_username && _password) {
        
        [self postXAuthAccessTokenRequestWithUsername:_username password:_password successBlock:^(NSString *oauthToken, NSString *oauthTokenSecret, NSString *userID, NSString *screenName) {
            successBlock(screenName, userID);
        } errorBlock:^(NSError *error) {
            errorBlock(error);
        }];
        
    } else {
        
        [self fetchResource:@"account/verify_credentials.json"
                 HTTPMethod:@"GET"
              baseURLString:@"https://api.twitter.com/1.1"
                 parameters:nil
              oauthCallback:nil
        uploadProgressBlock:nil
      downloadProgressBlock:nil
               successBlock:^(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id response) {
                   
                   if([response isKindOfClass:[NSDictionary class]] == NO) {
                       NSString *errorDescription = [NSString stringWithFormat:@"Expected dictionary, found %@", response];
                       NSError *error = [NSError errorWithDomain:NSStringFromClass([self class]) code:0 userInfo:@{NSLocalizedDescriptionKey : errorDescription}];
                       errorBlock(error);
                       return;
                   }
                   
                   NSDictionary *dict = response;
                   successBlock(dict[@"screen_name"], dict[@"id_str"]);
                   
               } errorBlock:^(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error) {
                   errorBlock(error);
               }];
    }
}

- (NSString *)oauthSignatureMethod {
    return @"HMAC-SHA1";
}

- (NSString *)oauthTimestamp {
    /*
     The oauth_timestamp parameter indicates when the request was created. This value should be the number of seconds since the Unix epoch at the point the request is generated, and should be easily generated in most programming languages. Twitter will reject requests which were created too far in the past, so it is important to keep the clock of the computer generating requests in sync with NTP.
     */
    
    if(_testOauthTimestamp) return _testOauthTimestamp;
    
    NSTimeInterval timeInterval = [[NSDate date] timeIntervalSince1970];
    
    return [NSString stringWithFormat:@"%d", (int)timeInterval];
}

- (NSString *)oauthVersion {
    return @"1.0";
}

- (void)postTokenRequest:(void(^)(NSURL *url, NSString *oauthToken))successBlock authenticateInsteadOfAuthorize:(BOOL)authenticateInsteadOfAuthorize forceLogin:(NSNumber *)forceLogin screenName:(NSString *)screenName oauthCallback:(NSString *)oauthCallback errorBlock:(void(^)(NSError *error))errorBlock {
    
    NSString *theOAuthCallback = [oauthCallback length] ? oauthCallback : @"oob"; // out of band, ie PIN instead of redirect
    
    __weak __typeof(self) weakSelf = self;
    
    [self fetchResource:@"oauth/request_token"
             HTTPMethod:@"POST"
          baseURLString:@"https://api.twitter.com"
             parameters:@{}
          oauthCallback:theOAuthCallback
    uploadProgressBlock:nil
  downloadProgressBlock:nil
           successBlock:^(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id body) {
               
               __typeof(self) strongSelf = weakSelf;
               
               if(strongSelf == nil) return;
               
               NSMutableDictionary *md = [[body st_parametersDictionary] mutableCopy];
               
               if([forceLogin boolValue]) md[@"force_login"] = @"1";
               if(screenName) md[@"screen_name"] = screenName;
               
               //
               
               NSMutableArray *parameters = [NSMutableArray array];
               
               [md enumerateKeysAndObjectsUsingBlock:^(id key, id obj, BOOL *stop) {
                   NSString *s = [NSString stringWithFormat:@"%@=%@", key, obj];
                   [parameters addObject:s];
               }];
               
               NSString *parameterString = [parameters componentsJoinedByString:@"&"];
               
               NSString *authenticateOrAuthorizeString = authenticateInsteadOfAuthorize ? @"authenticate" : @"authorize";
               
               NSString *urlString = [NSString stringWithFormat:@"https://api.twitter.com/oauth/%@?%@", authenticateOrAuthorizeString, parameterString];
               
               //
               
               NSURL *url = [NSURL URLWithString:urlString];
               
               strongSelf.oauthRequestToken = md[@"oauth_token"];
               strongSelf.oauthRequestTokenSecret = md[@"oauth_token_secret"]; // unused
               
               successBlock(url, strongSelf.oauthRequestToken);
               
           } errorBlock:^(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error) {
               errorBlock(error);
           }];
}

- (void)postTokenRequest:(void(^)(NSURL *url, NSString *oauthToken))successBlock oauthCallback:(NSString *)oauthCallback errorBlock:(void(^)(NSError *error))errorBlock {
    [self postTokenRequest:successBlock authenticateInsteadOfAuthorize:NO forceLogin:nil screenName:nil oauthCallback:oauthCallback errorBlock:errorBlock];
}

- (void)postReverseOAuthTokenRequest:(void(^)(NSString *authenticationHeader))successBlock errorBlock:(void(^)(NSError *error))errorBlock {
    
    [self fetchResource:@"oauth/request_token"
             HTTPMethod:@"POST"
          baseURLString:@"https://api.twitter.com"
             parameters:@{@"x_auth_mode" : @"reverse_auth"}
          oauthCallback:nil
    uploadProgressBlock:nil
  downloadProgressBlock:nil
           successBlock:^(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id body) {
               
               successBlock(body);
               
           } errorBlock:^(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error) {
               errorBlock(error);
           }];
}

- (void)postXAuthAccessTokenRequestWithUsername:(NSString *)username
                                       password:(NSString *)password
                                   successBlock:(void(^)(NSString *oauthToken, NSString *oauthTokenSecret, NSString *userID, NSString *screenName))successBlock
                                     errorBlock:(void(^)(NSError *error))errorBlock {
    
    NSDictionary *d = @{@"x_auth_username" : username,
                        @"x_auth_password" : password,
                        @"x_auth_mode"     : @"client_auth"};
    
    
    __weak __typeof(self) weakSelf = self;
    
    [self postResource:@"oauth/access_token"
         baseURLString:@"https://api.twitter.com"
            parameters:d
          successBlock:^(STHTTPRequest *request, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSString *body) {
              NSDictionary *dict = [body st_parametersDictionary];
              
              __typeof(self) strongSelf = weakSelf;
              
              if(strongSelf == nil) return;
              
              // https://api.twitter.com/oauth/authorize?oauth_token=OAUTH_TOKEN&oauth_token_secret=OAUTH_TOKEN_SECRET&user_id=USER_ID&screen_name=SCREEN_NAME
              
              self.oauthAccessToken = dict[@"oauth_token"];
              self.oauthAccessTokenSecret = dict[@"oauth_token_secret"];
              
              successBlock(strongSelf.oauthAccessToken, strongSelf.oauthAccessTokenSecret, dict[@"user_id"], dict[@"screen_name"]);
          } errorBlock:^(STHTTPRequest *request, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error) {
              
              __typeof(self) strongSelf = weakSelf;
              
              if(strongSelf == nil) return;
              
              // add failure reason if we can
              if([[error domain] isEqualToString:@"STTwitterTwitterErrorDomain"] && ([error code] == 87)) {
                  NSMutableDictionary *extendedUserInfo = [[error userInfo] mutableCopy];
                  extendedUserInfo[NSLocalizedFailureReasonErrorKey] = @"The consumer tokens are probably not xAuth enabled.";
                  NSError *extendedError = [NSError errorWithDomain:[error domain] code:[error code] userInfo:extendedUserInfo];
                  NSLog(@"-- %@", extendedError);
                  errorBlock(extendedError);
                  return;
              };
            
              errorBlock(error);
          }];
}

- (void)postAccessTokenRequestWithPIN:(NSString *)pin
                         successBlock:(void(^)(NSString *oauthToken, NSString *oauthTokenSecret, NSString *userID, NSString *screenName))successBlock
                           errorBlock:(void(^)(NSError *error))errorBlock {
    
    if([pin length] == 0) {
        errorBlock([NSError errorWithDomain:NSStringFromClass([self class]) code:STTwitterOAuthCannotPostAccessTokenRequestWithoutPIN userInfo:@{NSLocalizedDescriptionKey : @"PIN needed"}]);
        return;
    }
    
    //NSParameterAssert(pin);
    
    NSDictionary *d = @{@"oauth_verifier" : pin};
    
    __weak __typeof(self) weakSelf = self;
    
    [self postResource:@"oauth/access_token"
         baseURLString:@"https://api.twitter.com"
            parameters:d
          successBlock:^(STHTTPRequest *request, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSString *body) {
              
              __typeof(self) strongSelf = weakSelf;
              
              if(strongSelf == nil) return;
              
              NSDictionary *dict = [body st_parametersDictionary];
              
              // https://api.twitter.com/oauth/authorize?oauth_token=OAUTH_TOKEN&oauth_token_secret=OAUTH_TOKEN_SECRET&user_id=USER_ID&screen_name=SCREEN_NAME
              
              strongSelf.oauthAccessToken = dict[@"oauth_token"];
              strongSelf.oauthAccessTokenSecret = dict[@"oauth_token_secret"];
              
              successBlock(strongSelf.oauthAccessToken, strongSelf.oauthAccessTokenSecret, dict[@"user_id"], dict[@"screen_name"]);
              
          } errorBlock:^(STHTTPRequest *request, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error) {
              
              if (request.responseStatus == 401) {
                  self.oauthRequestToken = nil;
              }
              
              errorBlock(error);
          }];
}

- (void)signRequest:(STHTTPRequest *)r isMediaUpload:(BOOL)isMediaUpload oauthCallback:(NSString *)oauthCallback {
    NSParameterAssert(_oauthConsumerKey);
    NSParameterAssert(_oauthConsumerSecret);
    
    NSMutableArray *oauthParameters = [NSMutableArray arrayWithObjects:
                                       @{@"oauth_consumer_key"     : [self oauthConsumerKey]},
                                       @{@"oauth_nonce"            : [self oauthNonce]},
                                       @{@"oauth_signature_method" : [self oauthSignatureMethod]},
                                       @{@"oauth_timestamp"        : [self oauthTimestamp]},
                                       @{@"oauth_version"          : [self oauthVersion]}, nil];
    
    if([oauthCallback length]) [oauthParameters addObject:@{@"oauth_callback" : oauthCallback}];
    
    if(_oauthAccessToken) { // missing while authenticating with XAuth
        [oauthParameters addObject:@{@"oauth_token" : [self oauthAccessToken]}];
    } else if(_oauthRequestToken) {
        [oauthParameters addObject:@{@"oauth_token" : [self oauthRequestToken]}];
    }
    
    NSMutableArray *oauthAndPOSTParameters = [oauthParameters mutableCopy];
    
    if(r.POSTDictionary) {
        [r.POSTDictionary enumerateKeysAndObjectsUsingBlock:^(id key, id obj, BOOL *stop) {
            [oauthAndPOSTParameters addObject:@{ key : obj }];
        }];
    }
    
    // "In the HTTP request the parameters are URL encoded, but you should collect the raw values."
    // https://dev.twitter.com/docs/auth/creating-signature
    
    NSMutableArray *oauthAndPOSTandGETParameters = [[r.url st_rawGetParametersDictionaries] mutableCopy];
    [oauthAndPOSTandGETParameters addObjectsFromArray:oauthAndPOSTParameters];
    
    [r.GETDictionary enumerateKeysAndObjectsUsingBlock:^(id key, id obj, BOOL *stop) {
        NSDictionary *d = @{key:obj};
        [oauthAndPOSTandGETParameters addObject:d];
    }];
    
    NSString *signature = [[self class] oauthSignatureWithHTTPMethod:r.HTTPMethod
                                                                 url:r.url
                                                          parameters:isMediaUpload ? oauthParameters : oauthAndPOSTandGETParameters
                                                      consumerSecret:_oauthConsumerSecret
                                                         tokenSecret:_oauthAccessTokenSecret];
    
    [oauthParameters addObject:@{@"oauth_signature" : signature}];
    
    NSString *s = [[self class] oauthHeaderValueWithParameters:oauthParameters];
    
    [r setHeaderWithName:@"Authorization" value:s];
}

- (void)signRequest:(STHTTPRequest *)r isMediaUpload:(BOOL)isMediaUpload {
    [self signRequest:r isMediaUpload:isMediaUpload oauthCallback:nil];
}

- (void)signRequest:(STHTTPRequest *)r {
    [self signRequest:r isMediaUpload:NO];
}

- (NSDictionary *)OAuthEchoHeadersToVerifyCredentials {
    NSString *verifyCredentialsURLString = @"https://api.twitter.com/1.1/account/verify_credentials.json";
    
    STHTTPRequest *r = [STHTTPRequest requestWithURLString:verifyCredentialsURLString];
    [self signRequest:r];
    NSString *authorization = [r.requestHeaders valueForKey:@"Authorization"];
    
    if(authorization == nil) return nil;
    
    return @{@"X-Auth-Service-Provider" : verifyCredentialsURLString,
             @"X-Verify-Credentials-Authorization" : authorization};
}

- (NSObject<STTwitterRequestProtocol> *)fetchResource:(NSString *)resource
                                           HTTPMethod:(NSString *)HTTPMethod
                                        baseURLString:(NSString *)baseURLString
                                           parameters:(NSDictionary *)params
                                  uploadProgressBlock:(void(^)(int64_t bytesWritten, int64_t totalBytesWritten, int64_t totalBytesExpectedToWrite))uploadProgressBlock
                                downloadProgressBlock:(void(^)(NSObject<STTwitterRequestProtocol> *request, NSData *data))progressBlock
                                         successBlock:(void(^)(NSObject<STTwitterRequestProtocol> *request, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id response))successBlock
                                           errorBlock:(void(^)(NSObject<STTwitterRequestProtocol> *request, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error))errorBlock {
    
    return [self fetchResource:resource
                    HTTPMethod:HTTPMethod
                 baseURLString:baseURLString
                    parameters:params
                 oauthCallback:nil
           uploadProgressBlock:uploadProgressBlock
         downloadProgressBlock:progressBlock
                  successBlock:successBlock
                    errorBlock:errorBlock];
}

- (STHTTPRequest *)fetchResource:(NSString *)resource
                      HTTPMethod:(NSString *)HTTPMethod
                   baseURLString:(NSString *)baseURLString
                      parameters:(NSDictionary *)params
                   oauthCallback:(NSString *)oauthCallback
             uploadProgressBlock:(void(^)(int64_t bytesWritten, int64_t totalBytesWritten, int64_t totalBytesExpectedToWrite))uploadProgressBlock
           downloadProgressBlock:(void(^)(STHTTPRequest *r, NSData *data))downloadProgressBlock
                    successBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id response))successBlock
                      errorBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error))errorBlock {
    
    if([baseURLString hasSuffix:@"/"]) {
        baseURLString = [baseURLString substringToIndex:[baseURLString length]-1];
    }
    
    NSString *urlString = [NSString stringWithFormat:@"%@/%@", baseURLString, resource];
    
    __block __weak STHTTPRequest *wr = nil;
    STHTTPRequest *r = [STHTTPRequest twitterRequestWithURLString:urlString
                                                       HTTPMethod:HTTPMethod
                                                 timeoutInSeconds:_timeoutInSeconds
                                     stTwitterUploadProgressBlock:uploadProgressBlock
                                   stTwitterDownloadProgressBlock:^(NSData *data, int64_t totalBytesReceived, int64_t totalBytesExpectedToReceive) {
                                       if(downloadProgressBlock) downloadProgressBlock(wr, data);
                                   } stTwitterSuccessBlock:^(NSDictionary *requestHeaders, NSDictionary *responseHeaders, id json) {
                                       successBlock(wr, requestHeaders, responseHeaders, json);
                                   } stTwitterErrorBlock:^(NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error) {
                                       errorBlock(wr, requestHeaders, responseHeaders, error);
                                   }];
    
    NSString *postKey = [params valueForKey:kSTPOSTDataKey];
    NSData *postData = [params valueForKey:postKey];
    
    if([HTTPMethod isEqualToString:@"GET"]) {
        r.GETDictionary = params;
        [self signRequest:r];
    } else {
        // https://dev.twitter.com/docs/api/1.1/post/statuses/update_with_media
        
        r.POSTDictionary = params;
        
        NSString *postMediaFileName = [params valueForKey:kSTPOSTMediaFileNameKey];
        
        NSMutableDictionary *mutableParams = [params mutableCopy];
        [mutableParams removeObjectForKey:kSTPOSTDataKey];
        [mutableParams removeObjectForKey:kSTPOSTMediaFileNameKey];
        if(postData) {
            [mutableParams removeObjectForKey:postKey];
            
            NSString *filename = postMediaFileName ? postMediaFileName : @"media.jpg";
            
            [r addDataToUpload:postData parameterName:postKey mimeType:@"application/octet-stream" fileName:filename];
        }
        
        [self signRequest:r isMediaUpload:(postData != nil) oauthCallback:oauthCallback];
        
        // POST parameters must not be encoded while posting media, or spaces will appear as %20 in the status
        r.encodePOSTDictionary = (postData == nil);
        
        r.POSTDictionary = mutableParams ? mutableParams : @{};
    }
    
    [r startAsynchronous];
    
    return r;
}

// convenience
- (STHTTPRequest *)postResource:(NSString *)resource
                  baseURLString:(NSString *)baseURLString // no trailing slash
                     parameters:(NSDictionary *)params
                  progressBlock:(void(^)(STHTTPRequest *r, NSData *data))progressBlock
                   successBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id response))successBlock
                     errorBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error))errorBlock {
    
    return [self fetchResource:resource
                    HTTPMethod:@"POST"
                 baseURLString:baseURLString
                    parameters:params
                 oauthCallback:nil
           uploadProgressBlock:nil
         downloadProgressBlock:progressBlock
                  successBlock:successBlock
                    errorBlock:errorBlock];
}

// convenience
- (STHTTPRequest *)postResource:(NSString *)resource
                  baseURLString:(NSString *)baseURLString // no trailing slash
                     parameters:(NSDictionary *)params
                  oauthCallback:(NSString *)oauthCallback
                   successBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id response))successBlock
                     errorBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error))errorBlock {
    
    return [self fetchResource:resource
                    HTTPMethod:@"POST"
                 baseURLString:baseURLString
                    parameters:params
                 oauthCallback:oauthCallback
           uploadProgressBlock:nil
         downloadProgressBlock:nil
                  successBlock:successBlock
                    errorBlock:errorBlock];
}

// convenience
- (STHTTPRequest *)postResource:(NSString *)resource
                  baseURLString:(NSString *)baseURLString // no trailing slash
                     parameters:(NSDictionary *)params
                   successBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, id response))successBlock
                     errorBlock:(void(^)(STHTTPRequest *r, NSDictionary *requestHeaders, NSDictionary *responseHeaders, NSError *error))errorBlock {
    
    return [self fetchResource:resource
                    HTTPMethod:@"POST"
                 baseURLString:baseURLString
                    parameters:params
                 oauthCallback:nil
           uploadProgressBlock:nil
         downloadProgressBlock:nil
                  successBlock:successBlock
                    errorBlock:errorBlock];
}

@end

@implementation NSURL (STTwitterOAuth)

- (NSArray *)st_rawGetParametersDictionaries {
    
    NSString *q = [self query];
    
    NSArray *getParameters = [q componentsSeparatedByString:@"&"];
    
    NSMutableArray *ma = [NSMutableArray array];
    
    for(NSString *s in getParameters) {
        NSArray *kv = [s componentsSeparatedByString:@"="];
        NSAssert([kv count] == 2, @"-- bad length");
        if([kv count] != 2) continue;
        NSString *value = [kv[1] stringByReplacingPercentEscapesUsingEncoding:NSUTF8StringEncoding]; // use raw parameters for signing
        [ma addObject:@{kv[0] : value}];
    }
    
    return ma;
}

- (NSString *)st_normalizedForOauthSignatureString {
    return [NSString stringWithFormat:@"%@://%@%@", [self scheme], [self host], [self path]];
}

@end

@implementation NSString (STTwitterOAuth)

+ (NSString *)st_randomString {
    CFUUIDRef cfuuid = CFUUIDCreate (kCFAllocatorDefault);
    NSString *uuid = (__bridge_transfer NSString *)(CFUUIDCreateString (kCFAllocatorDefault, cfuuid));
    CFRelease (cfuuid);
    return uuid;
}

+ (NSString *)st_random32Characters {
    NSString *randomString = [self st_randomString];
    
    NSAssert([randomString length] >= 32, @"");
    
    return [randomString substringToIndex:32];
}

- (NSString *)st_signHmacSHA1WithKey:(NSString *)key {
    
    unsigned char buf[CC_SHA1_DIGEST_LENGTH];
    CCHmac(kCCHmacAlgSHA1, [key UTF8String], [key length], [self UTF8String], [self length], buf);
    NSData *data = [NSData dataWithBytes:buf length:CC_SHA1_DIGEST_LENGTH];
    return [data base64Encoding];
}

- (NSDictionary *)st_parametersDictionary {
    
    NSArray *parameters = [self componentsSeparatedByString:@"&"];
    
    NSMutableDictionary *md = [NSMutableDictionary dictionary];
    
    for(NSString *parameter in parameters) {
        NSArray *keyValue = [parameter componentsSeparatedByString:@"="];
        if([keyValue count] != 2) {
            continue;
        }
        
        [md setObject:keyValue[1] forKey:keyValue[0]];
    }
    
    return md;
}

- (NSString *)st_urlEncodedString {
    // https://dev.twitter.com/docs/auth/percent-encoding-parameters
    // http://tools.ietf.org/html/rfc3986#section-2.1
    
    return [self st_stringByAddingRFC3986PercentEscapesUsingEncoding:NSUTF8StringEncoding];
}

@end

@implementation NSData (STTwitterOAuth)

- (NSString *)base64EncodedString {
    
#if TARGET_OS_IPHONE
    return [self base64Encoding]; // private API
#else
    
    CFDataRef retval = NULL;
    SecTransformRef encodeTrans = SecEncodeTransformCreate(kSecBase64Encoding, NULL);
    if (encodeTrans == NULL) return nil;
    
    if (SecTransformSetAttribute(encodeTrans, kSecTransformInputAttributeName, (__bridge CFTypeRef)self, NULL)) {
        retval = SecTransformExecute(encodeTrans, NULL);
    }
    CFRelease(encodeTrans);
    
    NSString *s = [[NSString alloc] initWithData:(__bridge NSData *)retval encoding:NSUTF8StringEncoding];
    
    if(retval) {
        CFRelease(retval);
    }
    
    return s;
    
#endif
    
}
@end

