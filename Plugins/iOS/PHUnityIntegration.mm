// COPYRIGHT(c) 2011, Medium Entertainment, Inc., a Delaware corporation, which operates a service
// called PlayHaven., All Rights Reserved
//  
// NOTICE:  All information contained herein is, and remains the property of Medium Entertainment, Inc.
// and its suppliers, if any.  The intellectual and technical concepts contained herein are 
// proprietary to Medium Entertainment, Inc. and its suppliers and may be covered by U.S. and Foreign
// Patents, patents in process, and are protected by trade secret or copyright law. Dissemination of this 
// information or reproduction of this material is strictly forbidden unless prior written permission 
// is obtained from Medium Entertainment, Inc.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// 
// Contact: support@playhaven.com

#import "PHUnityIntegration.h"
#import "PHPublisherMetadataRequest.h"
#import "PHAPIRequest.h"
#import "JSON.h"

#define UNITY_SDK_VERSION @"ios-unity-2.0.1"

static NSString *const kPHMessageIDKey = @"mi";
static NSString *const kPHContentIDKey = @"ci";
static NSString *const kPHURIKey = @"uri";

//#define OPENUDID_SUPPORT

#pragma mark - Unity Externs
extern void UnitySendMessage(const char *obj, const char *method, const char *msg);

#pragma mark -

static PHUnityIntegration *sharedIntegration;

@interface PHPublisherContentRequest (Unity)
@property (nonatomic, readonly) NSString *contentUnitID;
@property (nonatomic, readonly) NSString *messageID;
@end

@interface PHUnityIntegration()
+(void)cancelRequestWithHashCode:(int)hashCode;
-(void)productPurchaseResolution:(int)action;

@end

@implementation PHUnityIntegration

+ (void)load
{
    // forcing creation and subscription of sharedInstance on app launch
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(sharedIntegration) name:UIApplicationDidFinishLaunchingNotification object:[UIApplication sharedApplication]];
}

+(PHUnityIntegration *)sharedIntegration
{
    if (sharedIntegration == nil)
    {
        sharedIntegration = [PHUnityIntegration new];
        [PHAPIRequest setPluginIdentifier:UNITY_SDK_VERSION];
        [[PHPushProvider sharedInstance] setDelegate:sharedIntegration];
    }
    
    return sharedIntegration;
}

-(SBJsonWriterPH *)writer
{
    if (_writer == nil) {
        Class writerClass = NSClassFromString(@"SBJsonWriterPH");
        if (nil != writerClass) {
            _writer = [writerClass new];
        }
        else {
            NSLog(@"ERROR: SBJsonWriter class is not linked.");
        }
    }
  
    return _writer;
}

-(void)request:(PHAPIRequest *)request didSucceedWithResponse:(NSDictionary *)responseData
{
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"success", @"name",
                                       (!!responseData)? responseData: [NSDictionary dictionary],@"data",
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void)request:(PHAPIRequest *)request didFailWithError:(NSError *)error
{
    NSDictionary *errorData = [NSDictionary dictionaryWithObjectsAndKeys:
                               [NSNumber numberWithInt:error.code],@"code",
                               error.localizedDescription,@"description",
                               nil];
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"error", @"name",
                                       errorData,@"data", 
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);
    
}

-(void)request:(PHPublisherContentRequest *)request contentDidFailWithError:(NSError *)error
{
    NSDictionary *errorData = [NSDictionary dictionaryWithObjectsAndKeys:
                               [NSNumber numberWithInt:error.code],@"code",
                               error.localizedDescription,@"description",
                               nil];
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"error", @"name",
                                       errorData,@"data", 
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void)requestDidGetContent:(PHPublisherContentRequest *)request
{
    NSDictionary *pDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                 request.placement,@"placement",
                                 nil];
    
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"gotcontent", @"name",
                                       pDictionary,@"data",
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);
}

-(void)request:(PHPublisherContentRequest *)request contentWillDisplay:(PHContent *)content
{
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"willdisplay", @"name",
                                       [NSDictionary dictionary],@"data", 
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);    
}

-(void)request:(PHPublisherContentRequest *)request contentDidDisplay:(PHContent *)content
{
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"diddisplay", @"name",
                                       [NSDictionary dictionary],@"data", 
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);        
}

//-(void)requestContentDidDismiss:(PHPublisherContentRequest *)request
-(void)request:(PHPublisherContentRequest *)request contentDidDismissWithType:(PHPublisherContentDismissType*)type
{
	NSDictionary *dismissRepresentation = [NSDictionary dictionaryWithObjectsAndKeys:
											type, @"type",
											nil];
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       [NSNumber numberWithInt:request.hashCode],@"hash",
                                       @"dismiss", @"name",
                                       dismissRepresentation,@"data", 
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);  
}

-(void)request:(PHPublisherContentRequest *)request unlockedReward:(PHReward *)reward
{
	NSArray *keys = [NSArray arrayWithObjects:@"name",@"quantity",@"receipt",nil];
	NSDictionary *rewardRepresentation = [reward dictionaryWithValuesForKeys:keys];
	NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
								   [NSNumber numberWithInt:request.hashCode],@"hash",
								   @"reward", @"name",
								   rewardRepresentation,@"data", 
								   nil];
  NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
  UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]); 
}

-(void)request:(PHPublisherContentRequest *)request makePurchase:(PHPurchase *)purchase
{
	self->currentPurchase = [purchase retain];
	NSArray *keys = [NSArray arrayWithObjects:@"productIdentifier",@"quantity",@"receipt",nil];
	NSDictionary *purchaseRepresentation = [purchase dictionaryWithValuesForKeys:keys];
	NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
								   [NSNumber numberWithInt:request.hashCode],@"hash",
								   @"purchasePresentation", @"name",
								   purchaseRepresentation,@"data", 
								   nil];
	NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
	UnitySendMessage("PlayHavenManager", "HandleNativeEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]); 
}
      
-(void)productPurchaseResolution:(int)action
{
	if (currentPurchase != nil)
		[currentPurchase reportResolution:(PHPurchaseResolutionType)action];
	[currentPurchase release];
	currentPurchase = nil;
}
          
+(void)cancelRequestWithHashCode:(int)hashCode
{
	int result = [PHAPIRequest cancelRequestWithHashCode:hashCode];
	if (result == 1) // OK
	{
		UnitySendMessage("PlayHavenManager", "RequestCancelSuccess", [[NSString stringWithFormat:@"%d", hashCode] cStringUsingEncoding:NSUTF8StringEncoding]);
	}
	else // 0 - hash not found
	{
		UnitySendMessage("PlayHavenManager", "RequestCancelFailed", [[NSString stringWithFormat:@"%d", hashCode] cStringUsingEncoding:NSUTF8StringEncoding]);
	}
}

- (void)providerDidRegisterAPNSDeviceToken:(PHPushProvider *)aProvider
{
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       @"didregister", @"name",
                                       nil,@"data",
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeAPNSEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);    
}

- (void)provider:(PHPushProvider *)aProvider didFailToRegisterAPNSDeviceTokenWithError:(NSError *)anError
{
    NSDictionary *errorData = [NSDictionary dictionaryWithObjectsAndKeys:
                               [NSNumber numberWithInt:anError.code],@"code",
                               anError.localizedDescription,@"description",
                               nil];
    NSDictionary *messageDictionary = [NSDictionary dictionaryWithObjectsAndKeys:
                                       @"didfailregister", @"name",
                                       errorData,@"data",
                                       nil];
    NSString *messageJSON = [self.writer stringWithObject:messageDictionary];
    UnitySendMessage("PlayHavenManager", "HandleNativeAPNSEvent", [messageJSON cStringUsingEncoding:NSUTF8StringEncoding]);
}

- (BOOL)pushProvider:(PHPushProvider *)aProvider shouldSendRequest:(PHPublisherContentRequest *)aRequest
{
    UnitySendMessage("PlayHavenManager", "RegisterContentRequest",
                                             [[NSString stringWithFormat:@"%@:%@", aRequest.contentUnitID, aRequest.messageID] cStringUsingEncoding:NSUTF8StringEncoding]);
    return NO;
}

- (BOOL)pushProvider:(PHPushProvider *)aProvider shouldOpenURL:(NSURL *)anURL
{
    UnitySendMessage("PlayHavenManager", "OpenURL", [[anURL absoluteString] cStringUsingEncoding:NSUTF8StringEncoding]);
    return NO;
}

@end

NSString* CreatePHUnityNSString(const char* string){
    if (string) {
        return [NSString stringWithUTF8String:string];
    } else {
        return @"";
    }
}

extern "C" {
    void _PlayHavenNativeLog(const char* message)
    {
        NSLog(@"%@", CreatePHUnityNSString(message));
    }
    
    void _PlayHavenOpenURL(const char* url){
        NSString *urlString = CreatePHUnityNSString(url);
        NSURL *nsURL = [NSURL URLWithString:urlString];
        if (nil != nsURL)
        {
            dispatch_async(dispatch_get_main_queue(), ^{
                [[UIApplication sharedApplication] openURL:nsURL];
            });
        }
    }
    
    void _PlayHavenOpenRequest(const int hash, const char* token, const char* secret, const char* customUDID){
        PHPublisherOpenRequest *request = [PHPublisherOpenRequest 
                                           requestForApp:CreatePHUnityNSString(token)
                                           secret:CreatePHUnityNSString(secret)];
        request.delegate = [PHUnityIntegration sharedIntegration];
        request.hashCode = hash;
		#ifdef OPENUDID_SUPPORT
		if (customUDID != nil)
			request.customUDID = CreatePHUnityNSString(customUDID);
		#endif
        [request send];
    }
    
    void _PlayHavenMetadataRequest(const int hash, const char* token, const char* secret, const char* placement){
        PHPublisherMetadataRequest *request = [PHPublisherMetadataRequest 
                                               requestForApp:CreatePHUnityNSString(token)
                                               secret:CreatePHUnityNSString(secret)
                                               placement:CreatePHUnityNSString(placement)
                                               delegate:[PHUnityIntegration sharedIntegration]];
        request.hashCode = hash;
        [request send];
    }
    
    void _PlayHavenContentRequest(const int hash, const char* token, const char* secret, const char* placement, const bool showsOverlayImmediately)
    {
        PHPublisherContentRequest *request = [PHPublisherContentRequest requestForApp:CreatePHUnityNSString(token)
                                               secret:CreatePHUnityNSString(secret)
                                               placement:CreatePHUnityNSString(placement)
                                               delegate:[PHUnityIntegration sharedIntegration]];
        request.hashCode = hash;
    
        request.showsOverlayImmediately = showsOverlayImmediately;
        [request send];
    }
    
    void _PlayHavenContentRequestByContentID(const int hash, const char* token, const char* secret, const char* contentID, const char* messageID, const bool showsOverlayImmediately)
    {
        PHPublisherContentRequest *request = [PHPublisherContentRequest requestForApp:CreatePHUnityNSString(token)
                                                                               secret:CreatePHUnityNSString(secret)
                                                                        contentUnitID:CreatePHUnityNSString(contentID)
                                                                            messageID:CreatePHUnityNSString(messageID)];
                                              
        request.delegate = [PHUnityIntegration sharedIntegration];
        request.hashCode = hash;
        
        request.showsOverlayImmediately = showsOverlayImmediately;
        [request send];
    }

    
    void _PlayHavenPreloadRequest(const int hash, const char* token, const char* secret, const char* placement)
    {
        PHPublisherContentRequest *request = [PHPublisherContentRequest
                                              requestForApp:CreatePHUnityNSString(token)
                                              secret:CreatePHUnityNSString(secret)
                                              placement:CreatePHUnityNSString(placement)
                                              delegate:[PHUnityIntegration sharedIntegration]];
        request.hashCode = hash;        
        [request preload];
    }
    
	void _PlayHavenCancelRequest(const int hash)
	{
		[PHUnityIntegration cancelRequestWithHashCode:hash];
	}
	
	void _PlayHavenProductPurchaseResolution(const int action)
	{
		[[PHUnityIntegration sharedIntegration] productPurchaseResolution:action];
	}
	
	void _PlayHavenIAPTrackingRequest(const char* token, const char* secret, const char* productId, const int quantity, const int resolution, const char* receiptData, const int receiptLength)
	{
		PHPublisherIAPTrackingRequest *request;
        request = [PHPublisherIAPTrackingRequest requestForApp:CreatePHUnityNSString(token)
                                                        secret:CreatePHUnityNSString(secret)
                                                       product:CreatePHUnityNSString(productId)
                                                      quantity:quantity
                                                    resolution:(PHPurchaseResolutionType)resolution
                                                   receiptData:receiptLength > 0 ? [NSData dataWithBytes:(const void *)receiptData length:sizeof(char)*receiptLength] : nil];
        [request send];

	}
	
	bool _PlayHavenOptOutStatus()
	{
		return [PHAPIRequest optOutStatus];
	}
	
	void _PlayHavenSetOptOutStatus(bool yesOrNo)
	{
		[PHAPIRequest setOptOutStatus:yesOrNo];
	}
    
    void _PlayHavenRegisterAPNSDeviceToken(char* deviceToken, const int deviceTokenLength, const char* token, const char* secret)
    {
        [PHPushProvider sharedInstance].applicationToken = CreatePHUnityNSString(token);
        [PHPushProvider sharedInstance].applicationSecret = CreatePHUnityNSString(secret);
        [[PHPushProvider sharedInstance] registerAPNSDeviceToken:[NSData dataWithBytes:deviceToken length:deviceTokenLength]];
    }

    void _PlayHavenHandleRemoteNotification(const int messageID, const int contentID, const char* uri)
    {
        NSMutableDictionary *userInfo = @{kPHMessageIDKey:@(messageID)}.mutableCopy;
        NSString *URI = CreatePHUnityNSString(uri);

        if (contentID)
            userInfo[kPHContentIDKey] = @(contentID);

        if (URI.length)
            userInfo[kPHURIKey] = URI;

        [PHPushProvider.sharedInstance handleRemoteNotificationWithUserInfo:userInfo];
    }
    
    void _PlayHavenRegisterForPushNotifications()
    {
        [[PHPushProvider sharedInstance] registerForPushNotifications];
    }

    void _PlayHavenUnegisterForPushNotifications()
    {
        [[PHPushProvider sharedInstance] unregisterForPushNotifications];
    }
}
