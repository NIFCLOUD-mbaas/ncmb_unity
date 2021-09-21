/*
 Copyright 2017-2021 FUJITSU CLOUD TECHNOLOGIES LIMITED All Rights Reserved.

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

#import "NCMBRichPushView.h"
#import "NCMBCloseImageView.h"

#define SIZE_OF_STATUS_BAR 20.0
#define DEFAULT_MARGIN_WIDTH 10
#define DEFAULT_MARGIN_HEIGHT 10
#define CLOSE_IMAGE_FRAME_SIZE 20.0
#define CLOSE_BUTTON_WIDTH 70.0
#define CLOSE_BUTTON_HEIGHT 20.0
#define CLOSE_BUTTON_BOTTOM_MARGIN 5.0
#define CLOSE_BUTTON_LEFT_MARGIN 20.0

@interface NCMBRichPushView() <WKNavigationDelegate>

@property (nonatomic) UIView *cv; //clear view
@property (nonatomic) UIView *uv; //ui view
@property (nonatomic) WKWebView *wv; // web view
@property (nonatomic) UIButton* closeButton;

@end

@implementation NCMBRichPushView

enum{
    ActivityIndicatorBackgroundTag = 10000,
    ActivityIndicatorTag = 10001,
};

- (void)appearWebView:(UIInterfaceOrientation)interfaceOrientation url:(NSString*)richUrl{
    //Regist Notification for rotate event.
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(resizeWebViewWithNotification:)
                                                 name:UIDeviceOrientationDidChangeNotification
                                               object:nil];

    UIWindow* window = [[UIApplication sharedApplication].windows lastObject];
    [window makeKeyAndVisible];

    self.cv = [[UIView alloc] initWithFrame:window.frame];
    self.cv.backgroundColor = [UIColor clearColor];


    self.uv = [[UIView alloc]init];

    self.wv = [[WKWebView alloc]init];

    //make instance of closeImageView
    NCMBCloseImageView *closeImage = [[NCMBCloseImageView alloc]initWithFrame:CGRectMake(0, 5, CLOSE_BUTTON_WIDTH, CLOSE_IMAGE_FRAME_SIZE)];

    //allocate off screen for create UIImage
    UIGraphicsBeginImageContextWithOptions(closeImage.frame.size, NO, 0);
    CGContextRef context = UIGraphicsGetCurrentContext();

    //paste UIImage from off screen
    [closeImage.layer renderInContext:context];
    UIImage *renderedImage = UIGraphicsGetImageFromCurrentImageContext();

    self.uv.alpha = 0.0f;

    //set button title
    self.closeButton = [UIButton buttonWithType:UIButtonTypeCustom];
    [self.closeButton setTitle:@"close" forState:UIControlStateNormal];

    UIImageView *iv = [[UIImageView alloc] initWithImage:renderedImage];
    iv.userInteractionEnabled = YES;

    //set margin for close button text
    [self.closeButton setTitleEdgeInsets:UIEdgeInsetsMake(0, CLOSE_BUTTON_LEFT_MARGIN, CLOSE_BUTTON_BOTTOM_MARGIN, 0)];

    UIColor *tColorInHighlight = [UIColor colorWithRed:1.0 green:1.0 blue:1.0 alpha:0.4];
    [self.closeButton setTitleColor:tColorInHighlight forState:UIControlStateHighlighted];

    //set background image for close button
    [self.closeButton setBackgroundImage:renderedImage forState:UIControlStateNormal];

    //sizing rich push view
    [self sizingWebView:interfaceOrientation];

    //create instance for loading view's background
    UIView* bg = [[UIView alloc] initWithFrame:(CGRect){0,0,self.wv.frame.size}];
    bg.backgroundColor = [UIColor colorWithRed:0 green:0 blue:0 alpha:0.6];
    bg.tag = ActivityIndicatorBackgroundTag;

    //create instance for loading view
    UIActivityIndicatorView* activity = [[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge];
    activity.tag = ActivityIndicatorTag;
    activity.center = (CGPoint){bg.frame.size.width/2,bg.frame.size.height/2};
    [bg addSubview:activity];
    [self.wv addSubview:bg];

    //set method for close button
    [self.closeButton addTarget:self action:@selector(closeWebView:) forControlEvents:UIControlEventTouchUpInside];

    //add close button image to rich push view
    [self.uv addSubview:self.closeButton];

    //set color of rich push view
    UIColor *color = [UIColor blackColor];
    UIColor *alpha = [color colorWithAlphaComponent:0.7];
    self.uv.backgroundColor = alpha;

    //edit edge of view
    self.uv.layer.cornerRadius = 5;
    self.uv.clipsToBounds = YES;

    self.wv.navigationDelegate = self;

    //add subview to main view
    [window addSubview:self.cv];
    [self.uv addSubview:self.wv];
    [window addSubview:self.uv];

    [UIView animateWithDuration:0.4f animations:^{
        self.uv.alpha = 1.0f;
    }];

    NSURL *url = [NSURL URLWithString:richUrl];
    NSURLRequest *req = [NSURLRequest requestWithURL:url cachePolicy:NSURLRequestReloadIgnoringLocalCacheData timeoutInterval:5];
    [self.wv loadRequest:req];
}

-(WKNavigation *)loadRequest:(NSURLRequest *)request{
    return [self.wv loadRequest:request];
}

- (void)resizeWebViewWithNotification:(NSNotification *)notification {
    UIInterfaceOrientation orientation = [[UIApplication sharedApplication]statusBarOrientation];
    [UIView animateWithDuration:[[UIApplication sharedApplication]statusBarOrientationAnimationDuration] animations:^{
        [self sizingWebView:orientation];
    }];
}

- (void)transformUIVew:(CGFloat)angle{
    if ([[[UIDevice currentDevice] systemVersion] floatValue]  < 8.0){
    /* xcodeのバージョンでエラーになってしまうので、使うのを保留
    if (NSFoundationVersionNumber <= NSFoundationVersionNumber_iOS_7_1){
     */
        self.uv.transform = CGAffineTransformMakeRotation(angle);
    }
}

- (void)sizingWebView:(UIInterfaceOrientation)interfaceOrientation {

    //setting value of webview size
    CGRect windowSize = [[UIScreen mainScreen] bounds];
    float windowSizeWidth;
    float windowSizeHeight;

    windowSizeWidth = windowSize.size.width;
    windowSizeHeight = windowSize.size.height;

    switch (interfaceOrientation) {
        case UIInterfaceOrientationPortrait:{
            [self transformUIVew:0];
            self.uv.frame = CGRectMake(DEFAULT_MARGIN_WIDTH, DEFAULT_MARGIN_HEIGHT + SIZE_OF_STATUS_BAR, windowSizeWidth - (DEFAULT_MARGIN_WIDTH * 2), windowSizeHeight -  (DEFAULT_MARGIN_HEIGHT * 2) - SIZE_OF_STATUS_BAR);
            self.wv.frame = CGRectMake(DEFAULT_MARGIN_WIDTH, DEFAULT_MARGIN_HEIGHT, self.uv.bounds.size.width - DEFAULT_MARGIN_WIDTH * 2, self.uv.bounds.size.height - (DEFAULT_MARGIN_HEIGHT * 2) - CLOSE_IMAGE_FRAME_SIZE);

            break;
        }case UIInterfaceOrientationLandscapeLeft:{
            [self transformUIVew:(-M_PI / 2.0)];
            self.uv.frame = CGRectMake(DEFAULT_MARGIN_HEIGHT + SIZE_OF_STATUS_BAR, DEFAULT_MARGIN_WIDTH, windowSizeWidth - (DEFAULT_MARGIN_HEIGHT * 2) - SIZE_OF_STATUS_BAR, windowSizeHeight - (DEFAULT_MARGIN_WIDTH * 2));
            self.wv.frame = CGRectMake(DEFAULT_MARGIN_HEIGHT, DEFAULT_MARGIN_WIDTH, self.uv.bounds.size.width - DEFAULT_MARGIN_HEIGHT * 2, self.uv.bounds.size.height - (DEFAULT_MARGIN_WIDTH * 2) - CLOSE_IMAGE_FRAME_SIZE);
            break;
        }case UIInterfaceOrientationLandscapeRight:{
            [self transformUIVew:(M_PI/2.0)];
            self.uv.frame = CGRectMake(DEFAULT_MARGIN_HEIGHT, DEFAULT_MARGIN_WIDTH, windowSizeWidth - (DEFAULT_MARGIN_HEIGHT * 2) - SIZE_OF_STATUS_BAR, windowSizeHeight - (DEFAULT_MARGIN_WIDTH * 2));
            self.wv.frame = CGRectMake(DEFAULT_MARGIN_HEIGHT, DEFAULT_MARGIN_WIDTH, self.uv.bounds.size.width - DEFAULT_MARGIN_HEIGHT * 2, self.uv.bounds.size.height - (DEFAULT_MARGIN_WIDTH * 2) - CLOSE_IMAGE_FRAME_SIZE);
            break;
        }case UIInterfaceOrientationPortraitUpsideDown:{
            [self transformUIVew:M_PI];
            self.uv.frame = CGRectMake(DEFAULT_MARGIN_WIDTH, DEFAULT_MARGIN_HEIGHT, windowSizeWidth - (DEFAULT_MARGIN_WIDTH * 2), windowSizeHeight - (DEFAULT_MARGIN_HEIGHT * 2) - SIZE_OF_STATUS_BAR);
            self.wv.frame = CGRectMake(DEFAULT_MARGIN_WIDTH, DEFAULT_MARGIN_HEIGHT, self.uv.bounds.size.width - (DEFAULT_MARGIN_WIDTH * 2), self.uv.bounds.size.height - (DEFAULT_MARGIN_HEIGHT * 2) - CLOSE_IMAGE_FRAME_SIZE);
            break;
        }
        case UIInterfaceOrientationUnknown:{
            break;
        }
    }
    UIView *bg = [self.wv viewWithTag:ActivityIndicatorBackgroundTag];
    bg.frame = CGRectMake(0, 0, self.wv.frame.size.width, self.wv.frame.size.height);
    UIView *iv = [self.wv viewWithTag:ActivityIndicatorTag];
    iv.center = CGPointMake(bg.frame.size.width/2, bg.frame.size.height/2);
    self.closeButton.frame = CGRectMake(self.uv.bounds.size.width/2 - CLOSE_BUTTON_WIDTH/2, DEFAULT_MARGIN_WIDTH * 1.7 + self.wv.bounds.size.height , CLOSE_BUTTON_WIDTH, CLOSE_BUTTON_HEIGHT);

}

- (void)closeWebView:(id)sender{
    [[UIDevice currentDevice]endGeneratingDeviceOrientationNotifications];
    [[NSNotificationCenter defaultCenter]removeObserver:self];
    [self.uv removeFromSuperview];
    [self.wv removeFromSuperview];
    [self.cv removeFromSuperview];
    self.uv = nil;
    self.wv = nil;
    self.cv = nil;

    [rv removeFromParentViewController];
    rv = nil;
    // define selector
    SEL selector = NSSelectorFromString(@"resetRichPushView");
}

-(void)startWebViewLoading{
    UIView* bg = [self.wv viewWithTag:ActivityIndicatorBackgroundTag];
    bg.hidden = NO;
    UIActivityIndicatorView* activity = (UIActivityIndicatorView*)[bg viewWithTag:ActivityIndicatorTag];
    [activity startAnimating];
}

-(void)endWebViewLoading{
    UIView* bg = [self.wv viewWithTag:ActivityIndicatorBackgroundTag];
    bg.hidden = YES;
    UIActivityIndicatorView* activity = (UIActivityIndicatorView*)[bg viewWithTag:ActivityIndicatorTag];
    [activity stopAnimating];
}

- (void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation{
    [self endWebViewLoading];
}

# pragma webview delegate

- (void) actionSheet:(UIActionSheet *)actionSheet clickedButtonAtIndex:(NSInteger)buttonIndex
{
    switch (buttonIndex) {
        case 0:
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:actionSheet.title]];
            break;

        default:
            break;
    }
}

- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler{
    [self startWebViewLoading];
        decisionHandler(WKNavigationActionPolicyAllow);
}

- (void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error{
    if ([error code] != NSURLErrorCancelled){
        UIView* bg = [self.wv viewWithTag:ActivityIndicatorBackgroundTag];
        [bg removeFromSuperview];

        NSString *html = @"<html><body><h1>ページを開けません。</h1></body></html>";
        NSData *bodyData = [html dataUsingEncoding:NSUTF8StringEncoding];
        [self.wv loadData:bodyData MIMEType:@"text/html" characterEncodingName:@"utf-8" baseURL:nil];
    }
}

# pragma handleRichPush

static NCMBRichPushView *rv;

+ (void) handleRichPush:(NSDictionary *)userInfo {
    NSString *urlStr = [userInfo objectForKey:@"com.nifcloud.mbaas.RichUrl"];

    if ([urlStr isKindOfClass:[NSString class]]) {
        if (rv == nil){
            rv = [[NCMBRichPushView alloc]init];
        }
        // リッチビューが表示する
        UIInterfaceOrientation orientation = [[UIApplication sharedApplication]statusBarOrientation];
        [rv appearWebView:orientation url:urlStr];

    }
}

@end
