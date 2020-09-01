//
//  TwitterAPI.h
//  demoTwitter
//
//  Created by Jimmy Huynh on 8/27/20.
//  Copyright © 2020 Jimmy Huynh. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "STTwitter.h"
#import "AppDelegateListener.h"

NS_ASSUME_NONNULL_BEGIN

@interface TwitterAPI : NSObject<AppDelegateListener>
    @property (nonatomic, retain) STTwitterAPI *twitter;
    @property (nonatomic, retain) NSString *callbackScheme;

    + (instancetype) sharedManager;
    -(void) loginTwitter:(NSString *) consumerConsumerKey consumerSecretConsumerKey:(NSString *) consumerSecretConsumerKey callbackScheme:(NSString *) callbackScheme;
@end

extern "C" void NCMB_LoginWithTwitter(char* consumerConsumerKey, char* consumerSecretConsumerKey, char* callbackScheme);

NS_ASSUME_NONNULL_END
