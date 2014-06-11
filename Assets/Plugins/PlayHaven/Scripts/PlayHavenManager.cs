//#define USE_NAMESPACE
//#define PH_USE_GENERICS

using UnityEngine;
using System;
using System.Collections;
#if PH_USE_GENERICS
using System.Collections.Generic;
#endif
using System.Runtime.InteropServices;

// COPYRIGHT(c) 2011-2013, Medium Entertainment, Inc., a Delaware corporation, which operates a service
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

#if !USE_NAMESPACE
using PlayHaven;
#else
namespace PlayHaven
{
#endif
	[AddComponentMenu("PlayHaven/Manager")]
	/** PlayHavenManager
	 * 
	 * Manages Interaction with the Playhaven server and device native code.
	 * 
	 * The PlayHavenManager (Manager) manages interactions between the project, the PlayHaven server and native code running on the target iOS or Android device.
	 * You must have a GameObject named 'PlayHavenManager' in your scene with this script attached in order for it to function properly. </p>
	 * There are various configuration options in the inspecter to customize the Manager's behaviour in your game or scene.
	 * 
	 * <list type="bullet">
	 * <item> <b>Keep</b>: If checked, then the PlayHavenManager will persist from scene to scene.
	 * <item> <b>Show by Default</b>: If checked, this option will darken the screen and prevent user interaction with the application immediatly unless specified when calling ContentRequest(string placement, bool showsOverlayImmediatly).
	 * <item> <b>Mask</b>: If checked, disables the darkening of the screen and preventing user interaction until the Content Unit is displayed regardless of ContentRequest and 'Show by Default' checkbox.
	 * <item> <b>Notify Open</b>: Specifies when PlayHaven will be notified of your game opening.  If this value is set to Manual then you must call OpenNotification() manually in your code.
	 * <item> <b>Content Units in Editor</b>: Will display placeholder Content Units while in the editor for in-editor testing.
	 * <item> <b>Fetch Badges</b>: Specifies when to update the More Games Placement badge.  If set to to 'Poll' then you can alter the initial 'Delay' and the polling 'Rate'.
	 * <item> <b>More Games Placement</b>: Specifies the Cross Promotion Widget Placement name.
	 * <item> <b>Cancel on Load</b>: Checking this box will cancel any pending requests when a new level is loaded.
	 * <item> <b>Launch Suppression</b>: If checked, this option will suppress content requests until the game has been launched a programmable number of times.
	 * <item> <b>Number of Launches</b>: If 'Launch Suppression' is checked, this option controls the number of launches that are required before Content Requests are made.
	 * </list>
	 */
	public class PlayHavenManager : MonoBehaviour
	{
		public static string KEY_LAUNCH_COUNT = "playhaven-launch-count";
		public const int NO_HASH_CODE = 0;
		public static string kPHMessageIDKey = @"mi";
		public static string kPHContentIDKey = @"ci";
		public static string kPHURIKey = @"uri";
		
		public bool paused = false;
		//! Settings for determining when the OpenNotification is sent to the Playhaven server.
		public enum WhenToOpen
		{
			Awake,				//!< Notify Playhaven of game initialization upon Awake.
			Start,				//!< Notify Playhaven of game initialization upon Start.
			Manual				//!< Notify Playhaven of game initialization manually; requires a manual calling of PlayHavenManager.OpenNotification.
		};
		
		//! Settings for determining when to send the BadgeRequest to the Playhaven server.
		public enum WhenToGetNotifications
		{
			Disabled,			//!< BadgeRequest will never be sent.
			Awake,				//!< BadgeRequest will be sent upon Awake.
			Start,				//!< BadgeRequest will be sent upon Start.
			OnEnable,			//!< BadgeRequest will be sent whenever this script is enabled.
			Manual,				//!< BadgeRequest will be sent manually; requies that you call PlayHavenManager.BadgeRequest manually to fetch Badge information.
			Poll				//!< BadgeRequest will be continually polled at the rate configured by PlayHavenManager.notificationPollRate.
		};

		//! Settings for determining when to register for push notifications if they are enabled.
		public enum WhenToRegisterForPushNotifications
		{
			Disabled,			//!< Never.
			Awake,				//!< Upon Awake.
			Start,				//!< Upon Start.
			OnEnable,			//!< Whenever this script is enabled.
			Manual,				//!< Manually; requies that you call PlayHavenManager.RegisterForPushNotifications manually t.
		};
		
		#region Attributes
		public string token = string.Empty;									//!< The Application's Token string, as provided by the Playhaven Dashboard.
		public string secret = string.Empty;								//!< The Application's Secret string, as provided by the Playhaven Dashboard.
		public string tokenAndroid = string.Empty;							//!< The Android Application's Token string, as provided by the Playhaven Dashboard.
		public string secretAndroid = string.Empty;							//!< The Android Application's Secret string, as provided by the Playhaven Dashboard.
		
		public bool enableAndroidPushNotifications = false;
		public string googleCloudMessagingProjectNumber = string.Empty;
		public bool enableApplePushNotifications = false;
		public WhenToRegisterForPushNotifications whenToRegisterForPush = WhenToRegisterForPushNotifications.Manual;
		
		/** Determines whether the containing GameObject will be destroyed on scene load.
		 * @value It is reccomended you set this value to <c>true</c>, and place the GameObject containing the
		 * manager in the first scene of your game; it will then persist through the lifetime of the game
		 * without needing to reinitialize on each scene load.
	 	 */
		public bool doNotDestroyOnLoad = true;
		
		/** Determines whether the Playhaven pop-up will be displayed immediatly to the player when content requests are made.
		 * @value It is reccomended that this value be set to <c>false</c> so that pop-ups are only displayed upon
		 * completion of successful content unit requests.
		 */
		public bool defaultShowsOverlayImmediately = false;
		
		/** Determines whether or not we want to ignore the behaviors of game-wide PlayHavenContentRequester values.
		 * @value If <c>true</c> then all individual pop-up behaviors will be masked; otherwise they will be respected.
		 */
		public bool maskShowsOverlayImmediately = false;

		//! Sets when to call PlayHavenManager.OpenNotification.
		public WhenToOpen whenToSendOpen = WhenToOpen.Awake;
		
		//! Sets when to call PlayHavenManager.BadgeRequest.
		public WhenToGetNotifications whenToGetNotifications = WhenToGetNotifications.Start;
		
		//! Defines what placement value the manager will use when performing badge requests in accordance with the whenToGetNotifications setting.
		public string badgeMoreGamesPlacement = "more_games";
		
		//! Sets the delay before badge updates begin when polling for badge updates is initiated.
		public float notificationPollDelay = 1f;

		//! Sets the frequency/interval of badge polling.
		public float notificationPollRate = 15f;
		
		/** Cancels all pending requests to the Playhaven server after a scene load.
		 * @value A value of <c>true</c> will ensure that CancelAllPendingRequests is called when a new scene is loaded, whereas 
		 * <c>false</c> will preserve requests made in previous scenes.
		 * @remarks Unity does not call OnLoadLevel when LoadLevelAdditive is used.
		 */
		public bool cancelAllOnLevelLoad = false;
		
		//! Suppress content requests until <i>n</i> game launches have occurred.
		public int suppressContentRequestsForLaunches = 0;
		
		//! Placements that should be suppressed before <i>n</i> number of game launches are defined here.
		public string[] suppressedPlacements;
		
		//! Placements that should never be suppressed regardless of global rules.
		public string[] suppressionExceptions;

		//! Option to allow drawing of content units in the editor.
		public bool showContentUnitsInEditor = true;

		public bool maskNetworkReachable = false; 			//!< Used for testing purposes only.
		#endregion
		
		#region Events
		/** Triggered when a request has been completed successfully.
		 * @param request The IPlayHavenRequest object which resulted in the error.
		 * @param responseData Hashtable representing the request response from the Playhaven server.
		 */
		public delegate void SuccessHandler(IPlayHavenRequest request, Hashtable responseData);
		
		/** Triggered when fetching a content unit has resulted in an error.
		 * @param request The successfully processed IPlayHavenRequest object.
		 * @param errorData Hashtable representing the error response from the Playhaven server.
		 */
		public delegate void ErrorHandler(IPlayHavenRequest request, Hashtable errorData);
		
		/** Triggered when a reward request has been completed successfully.
		 * @param request The successfully processed IPlayHavenRequest object.
		 * @param rewardData Hashtable representing the reward response from the Playhaven server.
		 */
		public delegate void RewardHandler(IPlayHavenRequest request, Hashtable rewardData);
		
		/** Triggered when a purchase request has been completed successfully.
		 * @param request The successfully processed IPlayHavenRequest object.
		 * @param purchaseData Hashtable representing the purchase response from the Playhaven server.
		 */
		public delegate void PurchaseHandler(IPlayHavenRequest request, Hashtable purchaseData);
		
		/** Triggered when dismissing a content unit has been completed.
		 * @param request The dismissed IPlayHavenRequest object.
		 * @param dismissData Hashtable representing the dismiss response from the Playhaven server.
		 */
		public delegate void DismissHandler(IPlayHavenRequest request, Hashtable dismissData);
		
		/** Triggered when a general request has been completed successfully.
		 * @param request The IPlayHavenRequest object.
		 * @remarks This delegate is used by IPlayHavenRequest.OnWillDisplay and IPlayHavenRequest.OnDidDisplay.
		 */
		public delegate void GeneralHandler(IPlayHavenRequest request);

		/** Triggered when a content request has been canceled either by the user or by design.
		 * @param requestId The hashcode of the content request that was canceled.
		 */
		public delegate void CancelRequestHandler(int requestId);
		
		/** Triggered when APNS registeration was successful
		 */
		public delegate void APNSDidRegisterHandler();
		
		/** Triggered when APNS registration failed.
		 * @param code The error code.
		 * @param description A localized description of the failure.
		 */
		public delegate void APNSDidFailToRegisterHandler(int code, string description);
		/* Asks for permittion to open URL received in push notification. */
        public delegate bool OpenUrlFromRemoteNotificationHandler(string url);
        public delegate bool OpenContentFromRemoteNotificationHandler(int hash);
		#if UNITY_IPHONE
		/* Triggerred when the application was launched using a custom URL scheme (iOS). */
        // public delegate void OpenUrlHandler(string url);
		
		/* Triggered when a remote push notification has been delivered. (iOS) */
		public delegate void RemoteLaunchPushNotificationHandler(RemoteNotification remoteNotification);
		#endif
		
		public event RequestCompletedHandler OnRequestCompleted;			//!< Raised when a request has been completed.
		public event BadgeUpdateHandler OnBadgeUpdate;						//!< Raised when the badge has been updated.
		public event RewardTriggerHandler OnRewardGiven;					//!< Raised when a reward has been given to the player.
		public event PurchasePresentedTriggerHandler OnPurchasePresented;	//!< Raised when the player has initiated the purchase process through the native store.
		public event WillDisplayContentHandler OnWillDisplayContent;		//!< Raised before content will be displayed to the player.
		public event DidDisplayContentHandler OnDidDisplayContent;			//!< Raised after content has been displayed to the player.
		public event PlayHaven.SuccessHandler OnSuccessOpenRequest;			//!< Raised upon successful OpenRequest having been made to the Playhaven server.
		public event PlayHaven.SuccessHandler OnSuccessPreloadRequest;		//!< Raised upon successful ContentPreloadRequest having been made to the Playhaven server.
		public event PlayHaven.DismissHandler OnDismissContent;				//!< Raised when content has been dismissed by the player.
		public event PlayHaven.ErrorHandler OnErrorOpenRequest;				//!< Raised when a OpenRequest to the Playhaven server resulted in an error.
		public event PlayHaven.ErrorHandler OnErrorContentRequest;			//!< Raised when a ContentRequester to the Playhaven server resulted in an error.
		public event PlayHaven.ErrorHandler OnErrorMetadataRequest;			//!< Raised when a MetadataRequest to the Playhaven server resulted in an error.
		public event CancelRequestHandler OnSuccessCancelRequest;			//!< Raised upon successful cancellation of a request.			
		public event CancelRequestHandler OnErrorCancelRequest;				//!< Raised when an error occured during a cancellation request.

		#if UNITY_IPHONE
		public event APNSDidRegisterHandler OnDidRegisterAPNSDeviceToken;	//<! Raised when APNS device token was successful
		public event APNSDidFailToRegisterHandler OnFailedToRegisterAPNSDeviceToken; //<! Raised when APNS device token registration failed.

		public OpenUrlFromRemoteNotificationHandler OnShouldOpenURLFromRemotePushNotification;  //<! Raised when application wants to open URL received in push notification.
		public OpenContentFromRemoteNotificationHandler OnShouldOpenContentFromRemotePushNotification; //<! Raised when application wants to open content received in push notification.

//		public event OpenUrlHandler OnApplicationLaunchedWithURL;			// <! Raised when the application was launched using a custom scheme URL.
		public event RemoteLaunchPushNotificationHandler OnReceivedRemotePushNotification; //<! Raised when a remote push notification is detected and processed.
		#endif
		
		[Obsolete("Make Cross Promotion requests with ContentRequest() and attach to the OnDismissContent event instead.",false)]
		public event SimpleDismissHandler OnDismissCrossPromotionWidget;	//!< Raised when the Cross-Promotion Widget has been dismissed.
		[Obsolete("Make Cross Promotion requests with ContentRequest() and attach to the OnDismissContent event instead.",false)]
		public event PlayHaven.ErrorHandler OnErrorCrossPromotionWidget;	//!< Raised when a ContentRequester for the Cross-Promotion Widget to the Playhaven server resulted in an error.
		#endregion
		
		#region Unity
	    void Awake()
		{	
			gameObject.name = "PlayHavenManager";
			_instance = FindInstance();
			
			DetectNetworkReachable();
			
			if (doNotDestroyOnLoad)
				DontDestroyOnLoad(this);
			
			#if UNITY_EDITOR
			DetermineInEditorDevice();
			integrationSkin = (GUISkin)Resources.Load("PlayHavenIntegrationSkin", typeof(GUISkin));
			#endif
			
			// launch counting
			if (suppressContentRequestsForLaunches > 0)
			{
				launchCount = PlayerPrefs.GetInt(KEY_LAUNCH_COUNT, 0);
				launchCount++;
				PlayerPrefs.SetInt(KEY_LAUNCH_COUNT, launchCount);
				PlayerPrefs.Save();
				if (Debug.isDebugBuild)
					Debug.Log("Launch count: "+launchCount);
			}
				
			#if UNITY_ANDROID
			InitializeAndroid();
			#endif			
			
			#if !ENABLE_MANUAL_PH_MANAGER_INSTANTIATION
			#if UNITY_IPHONE
			if (string.IsNullOrEmpty(token))
				Debug.LogError("PlayHaven token has not been specified in the PlayerHavenManager");
			if (string.IsNullOrEmpty(secret))
				Debug.LogError("PlayHaven secret has not been specified in the PlayerHavenManager");
			#elif UNITY_ANDROID
			if (string.IsNullOrEmpty(tokenAndroid))
				Debug.LogError("PlayHaven token has not been specified in the PlayerHavenManager");
			if (string.IsNullOrEmpty(secretAndroid))
				Debug.LogError("PlayHaven secret has not been specified in the PlayerHavenManager");
			#endif
			if (whenToSendOpen == WhenToOpen.Awake)
				OpenNotification();
	
			if (whenToGetNotifications == WhenToGetNotifications.Awake)
				BadgeRequest(badgeMoreGamesPlacement);
			
			if (whenToRegisterForPush == WhenToRegisterForPushNotifications.Awake)
				RegisterForPushNotifications();
			#endif
		}
		
		void OnEnable()
		{
			if (whenToGetNotifications == WhenToGetNotifications.OnEnable)
				BadgeRequest(badgeMoreGamesPlacement);
			if (whenToRegisterForPush == WhenToRegisterForPushNotifications.OnEnable)
				RegisterForPushNotifications();
		}
			
		void Start()
		{
			if (whenToSendOpen == WhenToOpen.Start)
				OpenNotification();
			
			#if UNITY_ANDROID
			ParseIntent();
			#endif

			if (whenToGetNotifications == WhenToGetNotifications.Start)
				BadgeRequest(badgeMoreGamesPlacement);
			else if (whenToGetNotifications == WhenToGetNotifications.Poll)
				PollForBadgeRequests();
			
			if (whenToRegisterForPush == WhenToRegisterForPushNotifications.Start)
				RegisterForPushNotifications();
	
			#if UNITY_IPHONE
			autoCallOpenUponUnpause = true;
//			if (OnApplicationLaunchedWithURL != null)
//				ProcessApplicationOpenURL();
			#endif
		}
		
		void OnApplicationPause(bool pause)
		{
			#if UNITY_IPHONE
			paused = pause;

			if (!pause)
			{
				DetectNetworkReachable();
				if (autoCallOpenUponUnpause)
					OpenNotification();
//				if (enableApplePushNotifications && OnReceivedLaunchRemotPushNotification != null)
//					ProcessRemoteNotifications();				
			}
			#elif UNITY_ANDROID
			if (!pause)
			{
				ParseIntent();
			}

			//RegisterActivityForTracking(!pause);
			#endif
		}
		
		#if UNITY_ANDROID
		void OnApplicationQuit()
		{
			if (Application.platform == RuntimePlatform.Android && obj_PlayHavenFacade != null)
				obj_PlayHavenFacade.Dispose();
		}
		#endif
		
		void ParseIntent()
		{
			#if UNITY_ANDROID
			string resptype = string.Empty;
			string json = string.Empty;
			
			if(Application.platform != RuntimePlatform.Android)
				return;
			
			if(obj_PlayHavenFacade == null)
				return;

			if (Debug.isDebugBuild)
				Debug.LogError("PlayHaven ParseIntent, checking for intent data.");
			
			json = obj_PlayHavenFacade.Call<string>("getCurrentIntentData");
			
			if (Debug.isDebugBuild)
				Debug.Log("received: " + json);
			
			if(json == null)
				return;
			
			ArrayList array = MiniJSON.jsonDecode(json) as ArrayList;
			foreach (Hashtable hash in array)
			{
				if(! hash.ContainsKey("type"))
					continue;
				
				resptype = hash["type"].ToString();
				if(resptype == "reward")
				{
					try
					{
						Reward reward = new Reward();
						reward.name = hash["name"].ToString();
						reward.quantity = int.Parse(hash["quantity"].ToString());
						reward.receipt = hash["receipt"].ToString();
						if (OnRewardGiven != null) OnRewardGiven(NO_HASH_CODE, reward);
					}
					#if PH_USE_GENERICS
					catch (KeyNotFoundException e)
					#else
					catch (Exception e)
					#endif
					{
						if (Debug.isDebugBuild)
							Debug.Log(e.Message);				
					}
				}else if(resptype == "purchase"){
					// TBD
				}else if(resptype == "optin"){
					// TBD		
				}else if(resptype == "uri"){
					if(Debug.isDebugBuild)
						Debug.Log("Deep linking detected: " + hash["uri"]);
				}
			}
			#endif			
		}
		
		void OnLevelWasLoaded(int level)
		{
			if (cancelAllOnLevelLoad)
				CancelAllPendingRequests();
		}
		#endregion
	
		#region Properties
		/** Gets the instance.
		 * Singleton pattern instance accessor; use this to get the instance of PlayHavenManager in the current scene.
		 * @returns The instance of the PlayHavenManager in the current scene.
		 */
		public static PlayHavenManager Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = FindInstance();
				}
				return _instance;
			}
		}
		
		/** Gets the instance.
		 * Singleton pattern instance accessor; use this to get the instance of PlayHavenManager in the current scene.
		 * @returns The instance of the PlayHavenManager in the current scene.
		 */
		[System.Obsolete("The lower case version of 'instance' is obsolete. Use the uppercase version 'Instance' instead.", false)]
		public static PlayHavenManager instance
		{
			get
			{
				if (!_instance)
				{
					_instance = FindInstance();
				}
				return _instance;
			}
		}

		/** Property for the optional custom UDID value. This should be set prior to to OpenNotification being sent.
		 * @returns The custom UDID that is currently in use.
		 * @remarks This value is always passed along with OpenNotification. This is useful for tracking individual
		 * devices with a custom unique identifier that is not the unique device identifier.
		 * You can set it directly, or supply it to the PlayHavenManager.OpenNotification(string customUDID) method
		 * which will set it for you.
		 */
		public string CustomUDID
		{
			get { return customUDID; }
			set { customUDID = value; }
		}

		/** Property for Opt-out status.
		 * Gets or sets a value indicating whether the user has opted-out of certain features.
		 * <c>true</c> if the user has opted-out; otherwise <c>false</c>.
		 */
		public bool OptOutStatus
		{
			get
			{
				if(Application.isEditor)
					return optOutStatus;
								
				#if UNITY_IPHONE
					return _PlayHavenOptOutStatus();
				#elif UNITY_ANDROID
					return obj_PlayHavenFacade.Call<bool>("getOptOut");
				#else
					return optOutStatus;
				#endif
			}
			set
			{
				if(Application.isEditor)
					optOutStatus = value;
				
				#if UNITY_IPHONE
					_PlayHavenSetOptOutStatus(value);
				#elif UNITY_ANDROID
					obj_PlayHavenFacade.Call("setOptOut", value);
				#else
					optOutStatus = value;
				#endif
			}
		}

		/** Property for the current badge value.
		 * @returns The current badge value.
		 */
		public string Badge
		{
			get { return badge; }
		}
		#endregion
		
		#region Actions
		/** Determines whether the specified placement will be suppressed.
		 * Suppression occurs when the launch count does not exceed the PlayHavenManager.suppressContentRequestsForLaunches value,
		 * and by adding the placement to the PlayHavenManager.suppressedPlacements Array either in the inspector or manually.
		 * @param placement The name of the placement to check.
		 * @returns <c>true</c> if the specified placement will be suppressed; otherwise <c>false</c>.
		 */
		public bool IsPlacementSuppressed(string placement)
		{
			if (suppressContentRequestsForLaunches > 0 && launchCount < suppressContentRequestsForLaunches)
			{
				if (suppressedPlacements != null && suppressedPlacements.Length > 0)
				{
					foreach (string suppressedPlacement in suppressedPlacements)
					{
						if (suppressedPlacement == placement)
							return true; 	// the placement is in the list, so it WILL be suppressed
					}
					return false; 			// the placement was not in the list
				}
				if (suppressionExceptions != null && suppressionExceptions.Length > 0)
				{
					foreach (string suppressionException in suppressionExceptions)
					{
						if (suppressionException == placement)
							return false; 	// the placement is in the exception list, so it WON'T be suppressed
					}
					return true; 			// the placement was not in the exception list, do suppress it
				}
				return true; 				// suppression is enabled but there are no specific suppressions or exceptions, so everything goes
			}
			return false; 					// suppression is not enabled
		}
		
		/** Set the Playhaven token and secret
		 * Sets the Playhaven token and secret to the supplied strings.
		 * @param token The token that will be used when making Playhaven requests
		 * @param secret The secret that will be used when making Playhaven requests
		 */
		public void SetKeys(string token, string secret)
		{
			#if UNITY_IPHONE
			this.token = token;
			this.secret = secret;
			#elif UNITY_ANDROID	
			SetKeys(token, secret, googleCloudMessagingProjectNumber);
			#endif
		}
		
		#if UNITY_ANDROID
		public void SetKeys(string token, string secret, string googleCloudMessagingProjectNumber)
		{
			this.googleCloudMessagingProjectNumber = googleCloudMessagingProjectNumber;
			this.tokenAndroid = token;
			this.secretAndroid = secret;
			
			if (Application.platform == RuntimePlatform.Android && obj_PlayHavenFacade != null)
			{
				obj_PlayHavenFacade.Call("setKeys", token, secret, (enableAndroidPushNotifications) ? googleCloudMessagingProjectNumber : string.Empty);
			}
		}
		#endif
		
		#if UNITY_ANDROID
		/* Registers the Android Activity.
		* If running on Android this method will optionally register or unregister
		* the Android Activity that your game is using with the Playhaven Plugin.
		* @param register <c>true</c> will register the Activity,
		* whereas <c>false</c<> will unregister.
		*/
		[Obsolete("This method is now obsolete and performs no action", false)]
		public void RegisterActivityForTracking(bool register)
		{
			//if (Application.platform == RuntimePlatform.Android)
			//	obj_PlayHavenFacade.Call((register) ? "register" : "unregister");
		}		
		#endif
		
		/* Sends an OpenRequest to the Playhaven server.
		 * Called when an application implementing Playhaven is launched or resumed from suspended state,
		 * or in accordance with the whenToSendOpen setting.
		 * @returns The hashcode of the open request.
		 */
		public int Open()
		{
			return SendRequest(RequestType.Open, string.Empty);
		}
		
		/* Sends an OpenRequest to the Playhaven server.
		 * Called when an application implementing Playhaven is launched or resumed from suspended state,
		 * or in accordance with the whenToSendOpen setting, using the specified UDID.
		 * @param customUDID Custom UDID value as defined by the developer.
		 * @returns The hashcode of the created request.
		 */
		public int Open(string customUDID)
		{
			return SendRequest(RequestType.Open, customUDID);
		}
		
		/* Makes a call to Open(string customUDID).
		 * Informs Playhaven that the game has been initialized using the specified UDID, and is called with respect to the PlayHavenManager.whenToSendOpen value.
		 * @params customUDID Optional custom UDID value to be defined by developers.
		 * @returns The hashcode of the requestId as an int.
		 * @remarks Will not call Open(string customUDID) if networkReachable is false,
		 * and sets custom UDID to the supplied string prior to calling sending the Open Request.
		 */
		public int OpenNotification(string customUDID)
		{
			if (networkReachable)
			{
				CustomUDID = customUDID;
				int requestId = Open(customUDID);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}
		
		/* Calls Open(string customUDID).
		 * Informs Playhaven that the game has been initialized, and is called with respect to the PlayHavenManager.whenToSendOpen value.
		 * @returns The hashcode of the requestId as an int.
		 * @remarks Will not call Open(string customUDID) if networkReachable is false,
		 * and sets custom UDID to the value of PlayHavenManager.CustomUDID.
		 */
		public int OpenNotification()
		{
			if (networkReachable)
			{
				int requestId = Open(CustomUDID);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}
			
		/* Cancels a specific pending request.
		 * Cancels the pending request which matches the supplied requestID.
		 * @param requestId The hashcode of the request to cancel.
		 */
		public void CancelRequest(int requestId)
		{
			#if UNITY_IPHONE
			if (Application.isEditor)
			{
				Debug.Log("PlayHaven: cancel request for request code = "+requestId);
				RequestCancelSuccess(requestId.ToString());
			}
			else
			{
				_PlayHavenCancelRequest(requestId);
			}
			#endif
		}
			
		/* Cancels all pending requests.
		 * Cancels all pending requests by calling CancelRequest for each.
		 */
		public void CancelAllPendingRequests()
		{
			foreach (int requestId in requestsInProgress)
			{
				CancelRequest(requestId);				
			}
			requestsInProgress.Clear();
		}

		/** Resolves a purchase request.
		 * Registers a purchase request with the Playhaven server for user-segmentation
		 * by passing only the PurchaseResolution type (Buy, Cancel, Error).
		 */
		#if UNITY_ANDROID
		[Obsolete("ProductPurchaseResolutionRequest no longer performs any action on Android; use ProductPurchaseTrackingRequest instead.", false)]
		#endif
		public void ProductPurchaseResolutionRequest(PurchaseResolution resolution)
		{
			SendProductPurchaseResolution(resolution);
		}
		
		/** Tracks an in-app purchase.
		 * Registers an in-app purchase with the Playhaven server for use in user-segmentation by passing 
		 * both the Purchase information and the purchase type (Buy, Cancel, Error).
		 */
		public void ProductPurchaseTrackingRequest(Purchase purchase, PurchaseResolution resolution)
		{
			SendIAPTrackingRequest(purchase, resolution, null);
		}
		
		/** Tracks an in-app purchase.
		 * Registers an in-app purchase with the Playhaven server for use in user-segmentation by passing 
		 * both the Purchase information and the purchase type (Buy, Cancel, Error).
		 */
		public void ProductPurchaseTrackingRequest(Purchase purchase, PurchaseResolution resolution, byte[] receiptData)
		{
			SendIAPTrackingRequest(purchase, resolution, receiptData);
		}
		
		/** Performs a content preload request.
		 * Preloads the content for the specified placement to the device prior to that content actually
		 * being displayed to the user. Useful for preparing a popup for immediate use when triggered.
		 * @returns The hashcode of the requestId as an int.
		 */
		public int ContentPreloadRequest(string placement)
		{
			#if UNITY_EDITOR
			inEditorContentUnitType = InEditorContentUnitType.None;
			#endif
			if (networkReachable)
			{
				int requestId = SendRequest(RequestType.Preload, placement);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}
		
		/** Performs a content request.
		 * Loads content, specified by the provided placement value.
		 * The placement is first checked to see if it has been suppressed and, if not, the request is made.
		 * @param placement Specifies which content is to be displayed to the user.
		 * @returns The hashcode of the request as an int.
		 */
		public int ContentRequest(string placement)
		{
			if (IsPlacementSuppressed(placement)) return NO_HASH_CODE;
			
			#if UNITY_EDITOR
			inEditorContentUnitType = InEditorContentUnitType.Generic;
			#endif
			if (networkReachable)
			{
				int requestId = SendRequest(RequestType.Content, placement, defaultShowsOverlayImmediately);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}
	
		/** Performs a content request.
		 * Loads content, specified by the provided placement value, to be displayed to the user immediatly.
		 * The placement is first checked to see if it has been suppressed and, if not, the request is made.
		 * @param placement Specifies which content is to be displayed to the user.
		 * @param showsOverlayImmediately Specifies whether the popup should be displayed immediatly.
		 * @returns The hashcode of the request as an int.
		 * @remarks It is advised that you either always supply a value of <c>false</c> for <c>showsOverlayImmediately</c>
		 * or call the ContentRequest(string placement) method instead, which will supply it for you.
		 */
		public int ContentRequest(string placement, bool showsOverlayImmediately)
		{
			if (IsPlacementSuppressed(placement)) return NO_HASH_CODE;
			
			#if UNITY_EDITOR
			inEditorContentUnitType = InEditorContentUnitType.Generic;
			#endif
			if (networkReachable)
			{
				int requestId = SendRequest(RequestType.Content, placement, showsOverlayImmediately && !maskShowsOverlayImmediately);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}
		
        public int ContentRequest(int requestId)
        {
            #if UNITY_EDITOR
            inEditorContentUnitType = InEditorContentUnitType.Generic;
            #endif
            if (networkReachable)
            {
                SendRequestForHashCode(requestId, defaultShowsOverlayImmediately);
                #if !UNITY_EDITOR
                requestsInProgress.Add(requestId);
                #endif
                return requestId;
            }
            return NO_HASH_CODE;
        }

        public int ContentRequest(int requestId, bool showsOverlayImmediately)
        {
            #if UNITY_EDITOR
            inEditorContentUnitType = InEditorContentUnitType.Generic;
            #endif
            if (networkReachable)
            {
                SendRequestForHashCode(requestId, showsOverlayImmediately && !maskShowsOverlayImmediately);
                #if !UNITY_EDITOR
                requestsInProgress.Add(requestId);
                #endif
                return requestId;
            }
            return NO_HASH_CODE;
        }
		
		#if UNITY_EDITOR
		/* Tests an in-editor content request.
		 * @returns The hashcode of the request as an int.
		 * @remarks This method is for editor use only, is in place for testing purposes, and will not be compiled to a device.
		 */
		public int ContentRequest(string placement, bool showsOverlayImmediately, PlayHavenContentRequester requester)
		{
			if (IsPlacementSuppressed(placement)) return NO_HASH_CODE;
			
			inEditorContentUnitType = (requester.rewardMayBeDelivered) ? InEditorContentUnitType.Reward : InEditorContentUnitType.Generic;
			if (networkReachable)
			{
				return SendRequest(RequestType.Content, placement, showsOverlayImmediately && !maskShowsOverlayImmediately);
			}
			return NO_HASH_CODE;
		}
		#endif
		
		/* Obsolete method of showing the cross-promotion widget.
		 * @returns The hashcode of the requestId as an int.
		 * @obsolete This method is obsolete because it assumes that you will have a placement called more_games; use ContentRequest with the relevant placement instead.
		 * 
		 * note: Because this call is obsolete it has been ommitted from the documentation.
		 */
		[Obsolete("This method is obsolete; it assumes that you will have a placement called more_games; instead, simply use ContentRequest() but with the relevant placement.",false)]
		public int ShowCrossPromotionWidget()
		{
			#if UNITY_EDITOR
			inEditorContentUnitType = InEditorContentUnitType.CrossPromotionWidget;
			#endif
			if (networkReachable)
			{
				int requestId = SendRequest(RequestType.CrossPromotionWidget, string.Empty, defaultShowsOverlayImmediately);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}

		/* Request updated badge data.
		 * Makes a request to the Playhaven server to update the Cross-Promotion Widget's badge indicator.
		 * @param placement Specifies which placement is to be used.
		 * @returns The hashcode of the requestId as an int.
		 */
		public int BadgeRequest(string placement)
		{
			if (networkReachable && whenToGetNotifications != PlayHavenManager.WhenToGetNotifications.Disabled)
			{
				int requestId = SendRequest(RequestType.Metadata, placement);
				#if !UNITY_EDITOR
				requestsInProgress.Add(requestId);
				#endif
				return requestId;
			}
			return NO_HASH_CODE;
		}
		
		/* Request updated badge data.
		 * Makes a request to the Playhaven server to update the Cross-Promotion Widget's badge indicator.
		 * @returns The hashcode of the requestId as an int.
		 */
		[Obsolete("This method is obsolete; it assumes that you will have a placement called more_games; instead, simply use BadgeRequest() but with the relevant placement.",false)]
		public int BadgeRequest()
		{
			return BadgeRequest("more_games");
		}
	
		/* Continuously polls the Playhaven server for badge updates.
		 * Initiates continuous polling for badge updates at an interval defined by notificationPollRate.
		 * @remarks The notificationPollRate value must be set prior to this method being called, or the method will print an
		 * error message to the console.
		 */
		public void PollForBadgeRequests()
		{
			CancelInvoke("BadgeRequestPolled");
			if (notificationPollRate > 0)
			{
				if (!string.IsNullOrEmpty(badgeMoreGamesPlacement))
					InvokeRepeating("BadgeRequestPolled", notificationPollDelay, notificationPollRate);
				else if (Debug.isDebugBuild)
					Debug.LogError("A more games badge placement is not defined.");
			}
			else if (Debug.isDebugBuild)
				Debug.LogError("cannot have a notification poll rate <= 0");
		}

		/* Stop polling for badge requests.
		 */
		public void StopPollingForBadgeRequests()
		{
			CancelInvoke("BadgeRequestPolled");	
		}

		//! Clears the current badge value.
		public void ClearBadge()
		{
			badge = string.Empty;
		}
		
		//! Register for push notifications.
		public void RegisterForPushNotifications()
		{
			if (whenToRegisterForPush != WhenToRegisterForPushNotifications.Disabled)
			{
				#if UNITY_ANDROID
				if (enableAndroidPushNotifications && Application.platform == RuntimePlatform.Android)
					obj_PlayHavenFacade.Call("registerForPushNotifications");
				#elif UNITY_IPHONE
				if (enableApplePushNotifications && Application.platform == RuntimePlatform.IPhonePlayer)
				{
					if (Debug.isDebugBuild)
					{
						Debug.Log("Registering for push notifications: YES");
						NativeLog("Registering for push notifications: YES");
					}
					_PlayHavenRegisterForPushNotifications();
					StartCoroutine("PollForAppleDeviceToken");
//					StartCoroutine("PollForAppleRemoteNotifications");
				}
				else if (Debug.isDebugBuild)
				{
					Debug.Log("Registering for push notifications: NO");
					NativeLog("Registering for push notifications: NO");
				}
				#endif
			}
			else if (Debug.isDebugBuild)
			{
				Debug.Log("Registering for push notifications: DISABLED");
				#if UNITY_IPHONE
				NativeLog("Registering for push notifications: DISABLED");
				#endif
			}
		}

		//! Deregister from push notifications.
		public void DeregisterFromPushNofications()
		{
			#if UNITY_ANDROID
			if (enableAndroidPushNotifications && Application.platform == RuntimePlatform.Android)
				obj_PlayHavenFacade.Call("deregisterFromPushNofications");
			#elif UNITY_IPHONE
			if (enableApplePushNotifications && Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_PlayHavenUnegisterForPushNotifications();
			}
			#endif
		}
		
		public static void NativeLog(string message)
		{
			#if UNITY_IPHONE
			if (Application.platform == RuntimePlatform.IPhonePlayer)
				_PlayHavenNativeLog(string.Format("(PlayHaven-Unity): {0}", message));
			#endif
		}
		
		#endregion

		#region Handlers
		#pragma warning disable 0618
		void HandleCrossPromotionWidgetRequestOnDismiss(IPlayHavenRequest request, Hashtable dismissData)
		{
			if (OnDismissCrossPromotionWidget != null) OnDismissCrossPromotionWidget();
		}
		#pragma warning restore 0618
		
		void HandleCrossPromotionWidgetRequestOnWillDisplay(IPlayHavenRequest request)
		{
			NotifyRequestCompleted(request.HashCode);
			if (OnWillDisplayContent != null) OnWillDisplayContent(request.HashCode);
		}
		
		void HandleCrossPromotionWidgetRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (OnDidDisplayContent != null) OnDidDisplayContent(request.HashCode);
		}
		
		#pragma warning disable 0618
		void HandleCrossPromotionWidgetRequestOnError (IPlayHavenRequest request, Hashtable errorData)
		{
			NotifyRequestCompleted(request.HashCode);
				
			Error error = CreateErrorFromJSON(errorData);
			if (OnErrorCrossPromotionWidget != null) OnErrorCrossPromotionWidget(request.HashCode, error);
		}
		#pragma warning restore 0618
		
		void HandleContentRequestOnDismiss(IPlayHavenRequest request, Hashtable dismissData)
		{
			DismissType dismissType = DismissType.Unknown;
			try
			{
//				#pragma warning disable 0219
				string dismissTypeString = dismissData["type"].ToString();
				dismissType = (DismissType)System.Enum.Parse(typeof(DismissType), dismissTypeString);
/*
				#if UNITY_IPHONE
				dismissType = (DismissType)System.Enum.Parse(typeof(DismissType), dismissTypeString);
				#elif UNITY_ANDROID
				if (dismissTypeString == "ApplicationTriggered")
					dismissType = DismissType.PHPublisherApplicationBackgroundTriggeredDismiss;
				else if (dismissTypeString == "ContentUnitTriggered")
					dismissType = DismissType.PHPublisherContentUnitTriggeredDismiss;
				else if (dismissTypeString == "CloseButtonTriggered")
					dismissType = DismissType.PHPublisherNativeCloseButtonTriggeredDismiss;
				else if (dismissTypeString == "NoContentTriggered")
					dismissType = DismissType.PHPublisherNoContentTriggeredDismiss;
				
				#endif
*/
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);				
			}
			
			if (OnDismissContent != null) OnDismissContent(request.HashCode, dismissType);
		}
		
		void HandleContentRequestOnWillDisplay(IPlayHavenRequest request)
		{
			NotifyRequestCompleted(request.HashCode);
			if (OnWillDisplayContent != null) OnWillDisplayContent(request.HashCode);
		}
		
		void HandleContentRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (OnDidDisplayContent != null) OnDidDisplayContent(request.HashCode);
		}
		
		void HandleContentRequestOnReward (IPlayHavenRequest request, Hashtable rewardData)
		{
			Reward reward = new Reward();
			try
			{
				reward.receipt = rewardData["receipt"].ToString();
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);
			}
			try
			{
				reward.name = rewardData["name"].ToString();
				reward.quantity = int.Parse(rewardData["quantity"].ToString());
				if (OnRewardGiven != null) OnRewardGiven(request.HashCode, reward);
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);				
			}
		}
		
		void HandleRequestOnPurchasePresented (IPlayHavenRequest request, Hashtable purchaseData)
		{
			Purchase purchase = new Purchase();
			try
			{
				purchase.receipt = purchaseData["receipt"].ToString();
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);				
			}
			try
			{
				purchase.productIdentifier = purchaseData["productIdentifier"].ToString();
				purchase.quantity = int.Parse(purchaseData["quantity"].ToString());
				#if UNITY_ANDROID
				if (purchaseData["price"] != null)
					purchase.price = double.Parse(purchaseData["price"].ToString());
				if (purchaseData["store"] != null)
					purchase.store = purchaseData["store"].ToString();
				#endif
				if (OnPurchasePresented != null) OnPurchasePresented(request.HashCode, purchase);
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);				
			}			
		}
			
		void HandleContentRequestOnError (IPlayHavenRequest request, Hashtable errorData)
		{
			NotifyRequestCompleted(request.HashCode);
				
			Error error = CreateErrorFromJSON(errorData);
			if (OnErrorContentRequest != null) OnErrorContentRequest(request.HashCode, error);
		}

		void HandlePreloadRequestOnDismiss (int requestId, DismissType dismissType)
		{
			NotifyRequestCompleted(requestId);
		}
	
		void HandleMetadataRequestOnError (IPlayHavenRequest request, Hashtable errorData)
		{
			NotifyRequestCompleted(request.HashCode);
				
			Error error = CreateErrorFromJSON(errorData);
			if (OnErrorMetadataRequest != null) OnErrorMetadataRequest(request.HashCode, error);
		}
	
		void HandleMetadataRequestOnSuccess (IPlayHavenRequest request, Hashtable responseData)
		{
			string type = string.Empty;
			try
			{
				if (responseData.ContainsKey("notification"))
				{
					Hashtable notification = responseData["notification"] as Hashtable;
					type = notification["type"].ToString();	
				}
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);
				type = string.Empty;
			}
			if (type == "badge")
			{
				try
				{
					Hashtable response = responseData["notification"] as Hashtable;
					badge = response["value"].ToString();
					if (OnBadgeUpdate != null) OnBadgeUpdate(request.HashCode, badge);
				}
				#if PH_USE_GENERICS
				catch (KeyNotFoundException e)
				#else
				catch (Exception e)
				#endif
				{
					if (Debug.isDebugBuild)
						Debug.Log(e.Message);					
				}
			}
		}
		
		void HandleMetadataRequestOnWillDisplay(IPlayHavenRequest request)
		{
			NotifyRequestCompleted(request.HashCode);
			if (OnWillDisplayContent != null) OnWillDisplayContent(request.HashCode);
		}
		
		void HandleMetadataRequestOnDidDisplay(IPlayHavenRequest request)
		{
			if (OnDidDisplayContent != null) OnDidDisplayContent(request.HashCode);
		}
	
		void HandleOpenRequestOnError (IPlayHavenRequest request, Hashtable errorData)
		{
			NotifyRequestCompleted(request.HashCode);
				
			Error error = CreateErrorFromJSON(errorData);
			if (OnErrorOpenRequest != null) OnErrorOpenRequest(request.HashCode, error);
		}
	
		void HandleOpenRequestOnSuccess (IPlayHavenRequest request, Hashtable responseData)
		{
			NotifyRequestCompleted(request.HashCode);
			if (OnSuccessOpenRequest != null) OnSuccessOpenRequest(request.HashCode);
		}
		
		void HandlePreloadRequestOnSuccess (IPlayHavenRequest request, Hashtable responseData)
		{
			NotifyRequestCompleted(request.HashCode);
			if (OnSuccessPreloadRequest != null) OnSuccessPreloadRequest(request.HashCode);
		}
		#endregion
		
		#region Private
		[HideInInspector] public bool lockToken = false;
		[HideInInspector] public bool lockSecret = false;
		[HideInInspector] public bool lockTokenAndroid = false;
		[HideInInspector] public bool lockSecretAndroid = false;
		
		private static PlayHavenManager _instance;
		private int launchCount;
		private string badge = string.Empty;
		private string customUDID = string.Empty;
		private static bool wasWarned;
		private bool optOutStatus;
		
		#if PH_USE_GENERICS
		private List<int> requestsInProgress = new List<int>(8);
		private static Dictionary<int, IPlayHavenRequest> sRequests = new Dictionary<int, IPlayHavenRequest>();	
		#else
		private ArrayList requestsInProgress = new ArrayList(8);
		private static Hashtable sRequests = new Hashtable();
		#endif
		
		#if UNITY_EDITOR
		#pragma warning disable 0414
		private GUISkin integrationSkin;
		#endif
				
		#if UNITY_IPHONE
		private bool autoCallOpenUponUnpause = false;
		#endif
		private bool networkReachable = true;
		
		#if ENABLE_INTERNAL_TESTING
		public INativeIntercepter NativeIntercepter { get; set; }
		public static bool noContentDelivered;
		#endif
		
		#if UNITY_IPHONE
		[DllImport("__Internal")] private static extern void _PlayHavenNativeLog(string message);		
		[DllImport("__Internal")] private static extern void _PlayHavenCancelRequest(int hash);		
		[DllImport("__Internal")] private static extern void _PlayHavenProductPurchaseResolution(int action);		
		[DllImport("__Internal")] private static extern void _PlayHavenIAPTrackingRequest(string token, string secret, string productId, int quantity, int resolution, byte[] receiptData, int receiptLength);
		[DllImport("__Internal")] private static extern bool _PlayHavenOptOutStatus();
		[DllImport("__Internal")] private static extern void _PlayHavenSetOptOutStatus(bool yesOrNo);
		[DllImport("__Internal")] private static extern void _PlayHavenRegisterAPNSDeviceToken(byte[] deviceToken, int deviceTokenLength, string token, string secret);
		[DllImport("__Internal")] private static extern void _PlayHavenRegisterForPushNotifications();
		[DllImport("__Internal")] private static extern void _PlayHavenUnegisterForPushNotifications();
		[DllImport("__Internal")] private static extern void _PlayHavenHandleRemoteNotification(int messageId, int contentId, string uri);
        [DllImport("__Internal")] private static extern void _PlayHavenOpenURL(string url);
		[DllImport("__Internal")] private static extern void _PlayHavenContentRequestByContentID(int hashCode, string token, string secret, string contentID, string messageID, bool showsOverlayImmediately);
		#elif UNITY_ANDROID
		private static AndroidJavaObject obj_PlayHavenFacade;
		#endif
		
		#if UNITY_ANDROID
		// Initializes the Android plugin; called in Awake, and omitted from documentation.
		private void InitializeAndroid()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
				{
					using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
					{
						obj_PlayHavenFacade = new AndroidJavaObject("com.playhaven.unity3d.PlayHavenFacade", obj_Activity, tokenAndroid, secretAndroid, (enableAndroidPushNotifications) ? googleCloudMessagingProjectNumber : string.Empty);
					}
				}
					
				//if (obj_PlayHavenFacade != null && !string.IsNullOrEmpty(tokenAndroid) && !string.IsNullOrEmpty(secretAndroid))
				//	obj_PlayHavenFacade.Call("setKeys", tokenAndroid, secretAndroid, (enableAndroidPushNotifications) ? googleCloudMessagingProjectNumber : string.Empty);
			}
		}
		#endif
		
		private void DetectNetworkReachable()
		{
			#if UNITY_IPHONE || UNITY_ANDROID
				#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				networkReachable = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || 
								   Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
				#else
				networkReachable = iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaCarrierDataNetwork || 
								   iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaWiFiNetwork;
				#endif
			#endif
			networkReachable &= !maskNetworkReachable;
		}
		
		private static PlayHavenManager FindInstance()
		{
			PlayHavenManager i = GameObject.FindObjectOfType(typeof(PlayHavenManager)) as PlayHavenManager;
			if (!i)
			{
				GameObject go = GameObject.Find("PlayHavenManager");
				if (go != null)
					i = go.GetComponent<PlayHavenManager>();
			}
			if (!i && !wasWarned)
			{
				Debug.LogWarning("unable to locate a PlayHavenManager in the scene");
				wasWarned = true;
			}
			return i;
		}
		
		#if UNITY_IPHONE
		private IEnumerator PollForAppleDeviceToken()
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				byte[] apnsToken = null;
				YieldInstruction oneSecond = new WaitForSeconds(1);
				while (apnsToken == null)
				{
					apnsToken = NotificationServices.deviceToken;
					yield return oneSecond;
				}
			
				if (Debug.isDebugBuild)
				{
					string hexToken = System.BitConverter.ToString(apnsToken).Replace("-", "");
					string log = string.Format("Registering device token {0}", hexToken);
					Debug.Log(log);
					NativeLog(log);
				}
				_PlayHavenRegisterAPNSDeviceToken(apnsToken, apnsToken.Length, token, secret);
				StartCoroutine("PollForAppleRemoteNotifications");
			}
		}
		
		private bool processRemoteNotifications()
		{
			for (int i = 0; i < NotificationServices.remoteNotificationCount; ++i)
			{
				RemoteNotification notification = NotificationServices.GetRemoteNotification(i);

				if (Debug.isDebugBuild)
				{
					string log = string.Format("Received notificaton:\nalert:{0}\nbadge:{1}\nsound:{2}\ninfo:{3}", notification.alertBody, notification.applicationIconBadgeNumber, notification.soundName, MiniJSON.jsonEncode(notification.userInfo));
					Debug.Log(log);
					NativeLog(log);
				}

				if (null != notification.userInfo[kPHMessageIDKey])  //check if this is a PlayHaven notification
				{
					Debug.Log("88888888888888888888888888888888888888888888888");
					PlayHavenManager.NativeLog("88888888888888888888888888888888888888888888888");
	
					
					if (OnShouldOpenContentFromRemotePushNotification == null)
						NativeLog("ERROR: You have not set an OnShouldOpenContentFromRemotePushNotification delegate (required by PlayHaven)");
					if (OnShouldOpenURLFromRemotePushNotification == null)
						NativeLog("ERROR: You have not set an OnShouldOpenURLFromRemotePushNotification delegate (required by PlayHaven)");
						
					if (0 != Convert.ToString(notification.userInfo[kPHURIKey]).Length && !this.paused)
					{
						LocalNotification localNotification = new LocalNotification();

						localNotification.userInfo = notification.userInfo;	
						NotificationServices.ScheduleLocalNotification(localNotification);
					}
					else
					{
						_PlayHavenHandleRemoteNotification((int)Convert.ToInt64(notification.userInfo[kPHMessageIDKey]), (int)Convert.ToInt64(notification.userInfo[kPHContentIDKey]), Convert.ToString(notification.userInfo[kPHURIKey]));

						if (OnReceivedRemotePushNotification != null)
							OnReceivedRemotePushNotification(notification);
					}
				}
				
				return true;
			}

			return false;
		}
		
		private bool processLocalNotifications()
		{
			for (int i = 0; i < NotificationServices.localNotificationCount; ++i)
			{
				LocalNotification notification = NotificationServices.GetLocalNotification(i);

				if (Debug.isDebugBuild)
				{
					string log = string.Format("Received notificaton:\nalert:{0}\nbadge:{1}\nsound:{2}\ninfo:{3}", notification.alertBody, notification.applicationIconBadgeNumber, notification.soundName, MiniJSON.jsonEncode(notification.userInfo));
					Debug.Log(log);
					NativeLog(log);
				}

				_PlayHavenHandleRemoteNotification((int)Convert.ToInt64(notification.userInfo[kPHMessageIDKey]), (int)Convert.ToInt64(notification.userInfo[kPHContentIDKey]), Convert.ToString(notification.userInfo[kPHURIKey]));

				return true;
			}

			return false;
		}

		private IEnumerator PollForAppleRemoteNotifications()
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				YieldInstruction oneSecond = new WaitForSeconds(1);

				for (;;)
				{	
					yield return oneSecond;  //wait for new notifications

					if (processRemoteNotifications())
						NotificationServices.ClearRemoteNotifications();

					if (processLocalNotifications())
						NotificationServices.ClearLocalNotifications();
				}
			}
		}

		#endif
		
		private void SendProductPurchaseResolution(PurchaseResolution resolution)
		{
			if (!Application.isEditor)
			{
				#if UNITY_IPHONE
				_PlayHavenProductPurchaseResolution((int)resolution);
				#endif
			}
		}
		
		private void SendIAPTrackingRequest(Purchase purchase, PurchaseResolution resolution, byte[] receiptData)
		{
			if (!Application.isEditor)
			{
				#if UNITY_IPHONE
				_PlayHavenIAPTrackingRequest(token, secret, purchase.productIdentifier, purchase.quantity, (int)resolution, receiptData, (receiptData==null) ? 0 : receiptData.Length);
				#elif UNITY_ANDROID
				obj_PlayHavenFacade.Call("iapTrackingRequest", purchase.productIdentifier, purchase.orderId, purchase.quantity, purchase.price, purchase.store, (int)resolution);
				#endif
			}
		}
		
        private void SendRequestForHashCode(int hash, bool showsOverlayImmediately)
        {
            if (sRequests.ContainsKey(hash))
            {
                ContentRequester request = (ContentRequester)sRequests[hash];
                request.Send(showsOverlayImmediately);
            }
        }
		
				
		private int SendRequest(RequestType type, string placement)
		{
			return SendRequest(type, placement, false);
		}
		
		private int SendRequest(RequestType type, string placement, bool showsOverlayImmediately)
		{
			IPlayHavenRequest request = null;
			
			switch (type)
			{
			case RequestType.Open:
				request = new OpenRequest(placement); 				// In this case, placement is actually the value of the customUDID.
				request.OnSuccess += HandleOpenRequestOnSuccess;
				request.OnError += HandleOpenRequestOnError;
				break;
			case RequestType.Metadata:
				request = new MetadataRequest(placement);
				request.OnSuccess += HandleMetadataRequestOnSuccess;
				request.OnError += HandleMetadataRequestOnError;
				request.OnWillDisplay += HandleMetadataRequestOnWillDisplay;
				request.OnDidDisplay += HandleMetadataRequestOnDidDisplay;
				break;
			case RequestType.Content:
				request = new ContentRequester(placement);
				request.OnError += HandleContentRequestOnError;
				request.OnDismiss += HandleContentRequestOnDismiss;
				request.OnReward += HandleContentRequestOnReward;
				request.OnPurchasePresented += HandleRequestOnPurchasePresented;
				request.OnWillDisplay += HandleContentRequestOnWillDisplay;
				request.OnDidDisplay += HandleContentRequestOnDidDisplay;
				break;
			case RequestType.Preload:
				request = new ContentPreloadRequester(placement);
				request.OnError += HandleContentRequestOnError;
				request.OnSuccess += HandlePreloadRequestOnSuccess;
				request.OnDismiss += HandleContentRequestOnDismiss;
				break;
			case RequestType.CrossPromotionWidget:
				request = new ContentRequester("more_games");
				request.OnError += HandleCrossPromotionWidgetRequestOnError;
				request.OnDismiss += HandleCrossPromotionWidgetRequestOnDismiss;
				request.OnWillDisplay += HandleCrossPromotionWidgetRequestOnWillDisplay;
				request.OnDidDisplay += HandleCrossPromotionWidgetRequestOnDidDisplay;
				break;
			}		
			
			if (request != null)
			{
				request.Send(showsOverlayImmediately);
				return request.HashCode;
			}
			return 0;
		}
		
		private IPlayHavenRequest GetRequestWithHash(int hash)
		{
			if (sRequests.ContainsKey(hash))
				#if PH_USE_GENERICS
				return sRequests[hash];
				#else
				return (IPlayHavenRequest)sRequests[hash];
				#endif
			return null;
		}
		
		private void ClearRequestWithHash(int hash)
		{
			if (sRequests.ContainsKey(hash))
			{
				sRequests.Remove(hash);
				if (Debug.isDebugBuild)
					Debug.Log(string.Format("Cleared request (id={0})", hash));
			}
		}
		
		private Error CreateErrorFromJSON(Hashtable errorData)
		{
			Error error = new Error();
			try
			{
				error.code = int.Parse(errorData["code"].ToString());
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);				
			}
			try
			{
				error.description = errorData["description"].ToString();
			}
			#if PH_USE_GENERICS
			catch (KeyNotFoundException e)
			#else
			catch (Exception e)
			#endif
			{
				if (Debug.isDebugBuild)
					Debug.Log(e.Message);				
			}
			return error;
		}
		
		private void NotifyRequestCompleted(int requestId)
		{
			#if UNITY_EDITOR
			Debug.Log("Request completed: "+requestId);
			#endif
			#if !UNITY_EDITOR
			requestsInProgress.Remove(requestId);
			#endif
			
			if (OnRequestCompleted != null) OnRequestCompleted(requestId);
		}
		
		private void BadgeRequestPolled()
		{
			BadgeRequest(badgeMoreGamesPlacement);
		}		
		
		#endregion
		
		#region Internal
		//! Playhaven request information implemented for built-in request types.
		public interface IPlayHavenRequest
		{
			event GeneralHandler OnWillDisplay;			//!< Raised when content will be displayed.
			event GeneralHandler OnDidDisplay;			//!< Raised when content has been displayed.
		    event SuccessHandler OnSuccess;				//!< Raised when request was successful.
		    event ErrorHandler OnError;					//!< Raised when request resulted in an error.
			event DismissHandler OnDismiss;				//!< Raised when content was dismissed.
		    event RewardHandler OnReward;				//!< Raised when a reward was presented to the player.
			event PurchaseHandler OnPurchasePresented;	//!< Raised when a player decides to purchase a Playhaven IAP content unit.
			
			//! Gets the hash code for this request.
			int HashCode { get; }
			
			//! Sends the OpenRequest to the Playhaven server.
			void Send();
			
			void Send(bool showsOverlayImmediately);
			
			void TriggerEvent(string eventName, Hashtable eventData);
		}
		
		//! The request sent to Playhaven server when starting and resuming the application.
		internal class OpenRequest: IPlayHavenRequest
		{
			private int hashCode;
			
			#if UNITY_IPHONE
			private string customUDID;
			
			[DllImport("__Internal")]
			private static extern void _PlayHavenOpenRequest(int hash, string token, string secret, string customUDID);
			#endif			
			
			//! Initializes a new instance of the OpenRequest class.
			public OpenRequest()
			{
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}
			
			/** Initializes a new instance of the OpenRequest class with the supplied UDID.
			 * @param customUDID The UDID of the requestor.
			 */
			public OpenRequest(string customUDID)
			{
				#if UNITY_IPHONE
				this.customUDID = customUDID;
				#endif			
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}
			
			//! Gets the hash code for this request.
			public int HashCode
			{
				get { return hashCode; }
			}
			
			/** Sends the OpenRequest to the Playhaven server.
			 * Convinience method for OpenRequest.Send(bool showsOverlayImmediately) that passes a value of <c>false</c>.
			 */
			public void Send()
			{
				Send(false);
			}
			
			/** Sends the OpenRequest to the Playhaven server.
			 * @param showsOverlayImmediately A value of <c>true</c> will display a pop-up immediatly;
			 * <c>false</c> will delay the pop-up until the request is complete.
			 * @remarks It is reccomended that you always provide a value of <c>false</c>,
			 * or use OpenRequest.Send which does it for you.
			 */
			public void Send(bool showsOverlayImmediately)
			{			
				#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: open request");
					// mimic a payload for in-editor integration validation
					// going to "dismiss"
					
					Hashtable data = new Hashtable();
					data["notification"] = new Hashtable();
					
					Hashtable result = new Hashtable();
					result["data"] = data;
					result["hash"] = hashCode;
					result["name"] = "success";
					
					string jsonResult = result.toJson();
					Instance.HandleNativeEvent(jsonResult);
				} 
				else 
				{
					if (Debug.isDebugBuild)
						Debug.Log(string.Format("PlayHaven: open request (id={0})", hashCode));
					#if UNITY_IPHONE
					_PlayHavenOpenRequest(hashCode, Instance.token, Instance.secret, customUDID);
					#elif UNITY_ANDROID
					obj_PlayHavenFacade.Call("openRequest", hashCode);
					#endif
				}
				#endif
			}
			
			// Events
		    public event SuccessHandler OnSuccess = delegate {};	//!< Raised when the OpenRequest has been completed successfully.
		    public event ErrorHandler OnError = delegate {};		//!< Raised when the OpenRequest has encountered an error.
			public event DismissHandler OnDismiss;
		    public event RewardHandler OnReward;
			public event PurchaseHandler OnPurchasePresented;
		    public event GeneralHandler OnWillDisplay;
		    public event GeneralHandler OnDidDisplay;
		    
			/** Raises the specifed event.
			 * Using the JSON data received from Playhaven, this method raises the appropriate event.
			 * @param eventName The event to raise.
			 * @param eventData Event data in JSON format.
			 */
		    public void TriggerEvent(string eventName, Hashtable eventData)
			{
		    	if (String.Compare(eventName,"success") == 0)
				{
		    		Debug.Log("PlayHaven: Open request success!");
	 				if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnSuccess(this, eventData);
		    	} 
				else if (String.Compare(eventName, "error") == 0)
				{
		    		Debug.LogError("PlayHaven: Open request failed!");
	 				if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
					OnError(this, eventData);
		    	}
		    }
		}
		
		/** The request sent to Playhaven server when updating the badge.
		 * @remarks This class is intended for requesting updated Badge information, and shouldn't need to be accessed directly by developers.
		 * Instead, simply call BadgeRequest, or setup badge polling in the PlayHavenManager inspector window.
		 */
		internal class MetadataRequest: IPlayHavenRequest
		{
			protected string mPlacement;
			private int hashCode;
			
			#if UNITY_IPHONE
			[DllImport("__Internal")]
			private static extern void _PlayHavenMetadataRequest(int hash, string token, string secret, string placement);
			#endif
			
			/** Initializes a new instance of the MetadataRequest class for the specified placement.
			 * @param placement The placement identifier as defined in the scene.
			 */
			public MetadataRequest(string placement)
			{
				mPlacement = placement;
				hashCode = GetHashCode();				
				sRequests.Add(hashCode, this);
			}
			
			//! Gets the hash code for this request.
			public int HashCode
			{
				get { return hashCode; }
			}
			
			/** Sends the MetadataRequest to the Playhaven server.
			 * Convinience method for MetadataRequest.Send(bool showsOverlayImmediately) that passes a value of <c>false</c>.
			 */
			public void Send()
			{
				Send(false);
			}
			
			/** Sends the MetadataRequest to the Playhaven server.
			 * @param showsOverlayImmediately A value of <c>true</c> will display a pop-up immediatly;
			 * <c>false</c> will delay the pop-up until the request is complete.
			 * @remarks It is reccomended that you always provide a value of <c>false</c>,
			 * or use MetadataRequest.Send which does it for you.
			 */
			public void Send(bool showsOverlayImmediately)
			{
				#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: metadata request ("+mPlacement+")");
					
					// mimic a payload for in-editor integration validation
					Hashtable notification = new Hashtable();
					notification["type"] = "badge";
					notification["value"] = "1";
					
					Hashtable data = new Hashtable();
					data["notification"] = notification;
					
					Hashtable result = new Hashtable();
					result["data"] = data;
					result["hash"] = hashCode;			// not part of a real result; for in-editor only
					result["name"] = "success";			// not part of a real result; for in-editor only
					result["content"] = mPlacement;						
					
					string jsonResult = result.toJson();
					Instance.HandleNativeEvent(jsonResult);
				} 
				else 
				{
					if (Debug.isDebugBuild)
						Debug.Log(string.Format("PlayHaven: metadata request (id={0}, placement={1})", hashCode, mPlacement));
					#if UNITY_IPHONE
					_PlayHavenMetadataRequest(hashCode, Instance.token, Instance.secret, mPlacement);
					#elif UNITY_ANDROID
					obj_PlayHavenFacade.Call("metaDataRequest", hashCode, mPlacement);
					#endif
				}
				#endif
			}
			
			// Events
			public event SuccessHandler OnSuccess = delegate {};		//!< Raised when the MetadataRequest has been completed successfully.
		    public event ErrorHandler OnError = delegate {};			//!< Raised when the MetadataRequest has encountered an error.
			public event DismissHandler OnDismiss;
		    public event RewardHandler OnReward;
			public event PurchaseHandler OnPurchasePresented;
		    public event GeneralHandler OnWillDisplay = delegate {};	//!< Raised when the MetadataRequest will be displayed on-screen.
		    public event GeneralHandler OnDidDisplay = delegate {}; 	//!< Raised when the MetadataRequest has been displayed on-screen.
		    
			/** Raises the specifed event.
			 * Using the JSON data received from Playhaven, this method raises the appropriate event.
			 * @param eventName The event to raise.
			 * @param eventData Event data in JSON format.
			 */
		    public void TriggerEvent(string eventName, Hashtable eventData)
			{
		    	if (String.Compare(eventName,"success") == 0)
				{
	    			Debug.Log("PlayHaven: Metadata request success!");
	 				if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): " +eventData.toJson());
					OnSuccess(this, eventData);
		    	} 
				else if (String.Compare(eventName, "willdisplay") == 0)
				{
					OnWillDisplay(this);
				}
				else if (String.Compare(eventName, "diddisplay") == 0)
				{
					OnDidDisplay(this);
				}
				else if (String.Compare(eventName, "error") == 0)
				{
	    			Debug.LogError("PlayHaven: Metadata request failed!");
	 				if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnError(this, eventData);
		    	}
		    }
		} 
		
		//! The request sent to the Playhaven server when a content unit has either been triggered by player interaction or by design.
		internal class ContentRequester: IPlayHavenRequest
		{
			protected string mPlacement;
			protected string mContentID;
			protected string mMessageID;
			private int hashCode;
			
			#if UNITY_IPHONE
			[DllImport("__Internal")]
			private static extern void _PlayHavenContentRequest(int hash, string token, string secret, string placement, bool showsOverlayImmediately);
			#endif
			/** Initializes a new instance of the ContentRequest class for the specified placement.
			 * @param placement The placement identifier as defined in the scene.
			 */
			public ContentRequester()
			{
				hashCode = GetHashCode();				
				sRequests.Add(hashCode, this);	
			}
			
			public ContentRequester(string placement)
			{
				mPlacement = placement;
				hashCode = GetHashCode();				
				sRequests.Add(hashCode, this);	
			}
			
			public ContentRequester(string contentID, string messageID)
			{
				mContentID = contentID;
				mMessageID = messageID;
				hashCode = GetHashCode();
				sRequests.Add(hashCode, this);
			}
			
			//! Gets the hash code for this request.
			public int HashCode
			{
				get { return hashCode; }
			}
			
			/** Sends the ContentRequest to the Playhaven server.
			 * Convinience method for ContentRequest.Send(bool showsOverlayImmediately) that passes a value of <c>false</c>.
			 */
			public void Send()
			{
				Send(false);
			}
			
			/** Sends the ContentRequest to the Playhaven server.
			 * @param showsOverlayImmediately A value of <c>true</c> will display a pop-up immediatly;
			 * <c>false</c> will delay the pop-up until the request is complete.
			 * @remarks It is reccomended that you always provide a value of <c>false</c>,
			 * or use ContentRequest.Send which does it for you.
			 */
			public void Send(bool showsOverlayImmediately)
			{
				#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: content request ("+mPlacement+")");
					
					// mimic a payload for in-editor integration validation
					// going to "dismiss"
					
					Hashtable data = new Hashtable();
					data["notification"] = new Hashtable();
					
					Hashtable result = new Hashtable();
					result["data"] = data;
					result["hash"] = hashCode;
					result["name"] = "dismiss";
					
					string jsonResult = result.toJson();
					Instance.HandleNativeEvent(jsonResult);
				} 
				else 
				{
					if (Debug.isDebugBuild)
						Debug.Log(string.Format("PlayHaven: content request (id={0}, placement={1})", hashCode, mPlacement));
					#if UNITY_IPHONE
					if (mContentID != null)
						_PlayHavenContentRequestByContentID(hashCode, Instance.token, Instance.secret, mContentID, mMessageID, showsOverlayImmediately);
					else
						_PlayHavenContentRequest(hashCode, Instance.token, Instance.secret, mPlacement, showsOverlayImmediately);
					#elif UNITY_ANDROID
					obj_PlayHavenFacade.Call("contentRequest", hashCode, mPlacement);
					#endif
				}
			#endif
			}
			
			// Events
			public event SuccessHandler OnSuccess;
			public event DismissHandler OnDismiss = delegate {};			//!< Raised when the ContentRequest has been dismissed.
		    public event ErrorHandler OnError = delegate {};				//!< Raised when the ContentRequest has encountered an error.
		    public event RewardHandler OnReward = delegate {};				//!< Raised when the ContentRequest has resulted in a Playhaven Reward.
		    public event PurchaseHandler OnPurchasePresented = delegate {};	//!< Raised when the ContentRequest has resulted in a player accepting a Playhaven IAP content unit.
		    public event GeneralHandler OnWillDisplay = delegate {};		//!< Raised when the ContentRequest will be displayed on-screen.
		    public event GeneralHandler OnDidDisplay = delegate {};			//!< Raised when the ContentRequest has been displayed on-screen.
		    
			/** Raises the specifed event.
			 * Using the JSON data received from Playhaven, this method raises the appropriate event.
			 * @param eventName The event to raise.
			 * @param eventData Event data in JSON format.
			 */
		    public void TriggerEvent(string eventName, Hashtable eventData)
			{
		    	if (String.Compare(eventName,"reward") == 0)
				{
		   			Debug.Log("PlayHaven: Reward unlocked");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnReward(this, eventData);
		    	} 
		    	else if (String.Compare(eventName,"purchasePresentation") == 0)
				{
		   			Debug.Log("PlayHaven: Purchase presented");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnPurchasePresented(this, eventData);
		    	} 
				else if (String.Compare(eventName,"dismiss") == 0)
				{
		    		Debug.Log("PlayHaven: Content was dismissed!");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnDismiss(this, eventData);
		    	} 
				else if (String.Compare(eventName, "willdisplay") == 0)
				{
					OnWillDisplay(this);
				}
				else if (String.Compare(eventName, "diddisplay") == 0)
				{
					OnDidDisplay(this);
				}
				else if (String.Compare(eventName, "error") == 0)
				{
		    		Debug.LogError("PlayHaven: Content error!");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnError(this, eventData);
		    	}
		    }
		} 
		
		//! The request sent to the Playhaven server to pre-load a content unit that has either been triggered by player interaction or by design.
		internal class ContentPreloadRequester: IPlayHavenRequest
		{
			protected string mPlacement;
			private int hashCode;
			
			#if UNITY_IPHONE
			[DllImport("__Internal")]
			private static extern void _PlayHavenPreloadRequest(int hash, string token, string secret, string placement);
			#endif
			/** Initializes a new instance of the ContentPreloadRequest class.
			 * @param placement The placement identifier as defined in the scene.
			 */
			public ContentPreloadRequester(string placement)
			{
				mPlacement = placement;
				hashCode = GetHashCode();				
				sRequests.Add(hashCode, this);	
			}
			
			//! Gets the hash code for this request.
			public int HashCode
			{
				get { return hashCode; }
			}
			
			/** Sends the ContentPreloadRequest to the Playhaven server.
			 * Convinience method for ContentPreloadRequest.Send(bool showsOverlayImmediately) that passes a value of <c>false</c>.
			 */
			public void Send()
			{
				Send(false);
			}
			
			/** Sends the ContentPreloadRequest to the Playhaven server.
			 * @param showsOverlayImmediately A value of <c>true</c> will display a pop-up immediatly;
			 * <c>false</c> will delay the pop-up until the request is complete.
			 * @remarks It is reccomended that you always provide a value of <c>false</c>,
			 * or use ContentPreloadRequest.Send which does it for you.
			 */
			public void Send(bool showsOverlayImmediately)
			{				
				#if UNITY_IPHONE || UNITY_ANDROID
				if (Application.isEditor)
				{
					Debug.Log("PlayHaven: content preload request ("+mPlacement+")");
					// mimic a payload for in-editor integration validation
					// going to "dismiss"
					
					Hashtable data = new Hashtable();
					data["notification"] = new Hashtable();
					
					Hashtable result = new Hashtable();
					result["data"] = data;
					result["hash"] = hashCode;
					result["name"] = "dismiss";
					
					string jsonResult = result.toJson();
					Instance.HandleNativeEvent(jsonResult);
				} 
				else 
				{				
					if (Debug.isDebugBuild)
						Debug.Log(string.Format("PlayHaven: content preload request (id={0}, placement={1})", hashCode, mPlacement));
					#if UNITY_IPHONE
					_PlayHavenPreloadRequest(hashCode, PlayHavenManager.Instance.token, PlayHavenManager.Instance.secret, mPlacement);
					#elif UNITY_ANDROID
					obj_PlayHavenFacade.Call("preloadRequest", hashCode, mPlacement);
					#endif
				}
			#endif
			}
			
			// Events
		    public event SuccessHandler OnSuccess = delegate {};	//!< Raised when the ContentPreloadRequest has been completed successfully.
			public event DismissHandler OnDismiss = delegate {};
		    public event ErrorHandler OnError = delegate {};		//!< Raised when the ContentPreloadRequest has encountered an error.
		    public event RewardHandler OnReward;
		    public event PurchaseHandler OnPurchasePresented;
		    public event GeneralHandler OnWillDisplay;
		    public event GeneralHandler OnDidDisplay;
		    
			/** Raises the specifed event.
			 * Using the JSON data received from Playhaven, this method raises the appropriate event.
			 * @param eventName The event to raise.
			 * @param eventData Event data in JSON format.
			 */
		    public void TriggerEvent(string eventName, Hashtable eventData)
			{
		    	if (String.Compare(eventName,"gotcontent") == 0)
				{
		   			Debug.Log("PlayHaven: Preloaded content");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnSuccess(this, eventData);
		    	}
				else if (String.Compare(eventName, "error") == 0)
				{
		    		Debug.LogError("PlayHaven: Content error!");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnError(this, eventData);
		    	}
				else if (String.Compare(eventName,"dismiss") == 0)
				{
		    		Debug.Log("PlayHaven: Content was dismissed!");
					if (Debug.isDebugBuild)
						Debug.Log("JSON (trigger event): "+eventData.toJson());
		    		OnDismiss(this, eventData);
		    	}
		    }
		}
				
		/* Handles native code events for APNS functionality.
		 * Handles calls from the native Playhaven plugin while running on the device or in the editor.
		 * There is no need to call this method manually from anywhere within your code.
		 * 
		 * note: Because this call is only made from native code it has been ommitted from the documentation.
		 */
		public void HandleNativeAPNSEvent(string json)
		{
			#if UNITY_IPHONE
			if (Debug.isDebugBuild)
				Debug.Log("JSON (native APNS event): "+json);
			Hashtable nativeData = MiniJSON.jsonDecode(json) as Hashtable;
			string eventName = nativeData["name"].ToString();
			Debug.Log("Received native event:");
			Debug.Log(eventName);
			NativeLog("Received native event:");
			NativeLog(eventName);
			if (eventName == "didregister")
			{
				if (OnDidRegisterAPNSDeviceToken != null) OnDidRegisterAPNSDeviceToken();
			}
			else if (eventName == "didfailregister")
			{
				if (OnFailedToRegisterAPNSDeviceToken != null)
				{
					Hashtable eventData = nativeData["data"] as Hashtable;
					OnFailedToRegisterAPNSDeviceToken(int.Parse(eventData["code"].ToString()), eventData["description"].ToString());
				}
			}
			else
			{

			}
			#endif
		}
		
		/* Handles native code events.
		 * Handles calls from the native Playhaven plugin while running on the device or in the editor.
		 * There is no need to call this method manually from anywhere within your code.
		 * 
		 * note: Because this call is only made from native code it has been ommitted from the documentation.
		 */
		public void HandleNativeEvent(string json)
		{
			if (Debug.isDebugBuild)
				Debug.Log("JSON (native event): "+json);
			
			#if ENABLE_INTERNAL_TESTING
			if (NativeIntercepter != null)
			{
				json = NativeIntercepter.Intercept(json);
				if (Debug.isDebugBuild)
					Debug.Log("JSON (injected event): "+json);
			}
			#endif
			
			Hashtable nativeData = MiniJSON.jsonDecode(json) as Hashtable;
			#if ENABLE_INTERNAL_TESTING
			Hashtable data = nativeData["data"] as Hashtable;
			if ((data != null && data.ContainsKey("type")) && (string)data["type"] == "PHPublisherNoContentTriggeredDismiss")
				noContentDelivered = true;
			#endif
			int hash = int.Parse(nativeData["hash"].ToString());
	
			string eventName = nativeData["name"].ToString();
			Hashtable eventData = nativeData["data"] as Hashtable;
			
			IPlayHavenRequest request = GetRequestWithHash(hash);
			if (request != null)
			{
				if (Debug.isDebugBuild)
					Debug.Log(string.Format("PlayHaven event={0} (id={1})", eventName, hash));
				
				/* POSSIBLE EVENTS:
				 * willdisplay
				 * diddisplay
				 * dismiss
				 * success
				 * reward
				 * error
				 * purchasePresentation
				 * gotcontent
				 */
				
				request.TriggerEvent(eventName, eventData);
				if (eventName == "dismiss" || eventName == "error")
				{
					ClearRequestWithHash(hash);
				}
			}
			else if (Debug.isDebugBuild)
			{
				Debug.LogError("Unable to locate request with id="+hash);
			}
		}
		
		//private IEnumerator DelayedClearRequestWithHash(float delay, int hash)
		//{
		//	yield return new WaitForSeconds(delay);
		//	ClearRequestWithHash(hash);
		//}
		
		private void RequestCancelSuccess(string hashCodeString)
		{
			int hashCode = System.Convert.ToInt32(hashCodeString);
			ClearRequestWithHash(hashCode);
			if (OnSuccessCancelRequest != null)
				OnSuccessCancelRequest(hashCode);
		}
	
		/* Handles calls from native code.
		 * Handles calls from the native Playhaven plugin or when running in editor.
		 * There is no need to call this method manually from anywhere within your code.
		 * 
		 * note: Because this call is only made from native code it has been ommitted from the documentation.
		 */
		public void RequestCancelFailed(string hashCodeString)
		{
			if (OnErrorCancelRequest != null)
			{
				int hashCode = System.Convert.ToInt32(hashCodeString);
				OnErrorCancelRequest(hashCode);
			}
		}
		
		#if UNITY_IPHONE
		private void RegisterContentRequest(string IDString)
		{
			string[] IDs = IDString.Split(':');
			
			if (IDs.Length != 2)
			{
				if (Debug.isDebugBuild)
					Debug.Log("RegisterContentRequest: unexpected arguments count:" + IDs.Length);			
				return;
			}
			
			string contentID = IDs[0];
			string messageID = IDs[1];
			
			if (Debug.isDebugBuild)
				Debug.Log("JSON (Registering request)");
			
			ContentRequester request = new ContentRequester(contentID, messageID);
			
			request.OnError += HandleContentRequestOnError;
			request.OnDismiss += HandleContentRequestOnDismiss;
			request.OnReward += HandleContentRequestOnReward;
			request.OnPurchasePresented += HandleRequestOnPurchasePresented;
			request.OnWillDisplay += HandleContentRequestOnWillDisplay;
			request.OnDidDisplay += HandleContentRequestOnDidDisplay;
			
		    if (null != OnShouldOpenContentFromRemotePushNotification)
			{
			    if (!OnShouldOpenContentFromRemotePushNotification(request.GetHashCode()))
			        return;
			}
			
		    ContentRequest(request.GetHashCode());
		}
		#endif
        
		#if UNITY_IPHONE
        private void OpenURL(string url)
        {
            if (null != OnShouldOpenURLFromRemotePushNotification)
            {
                if (OnShouldOpenURLFromRemotePushNotification(url))
                    _PlayHavenOpenURL(url);
            }
            else
            {
            	_PlayHavenOpenURL(url);
            }
        }
		#endif

		#endregion
		
		#region In-editor Testing
		#if UNITY_EDITOR			
		private enum InEditorContentUnitType { None, Generic, CrossPromotionWidget, Announcement, Advertisement, Reward };
		private InEditorContentUnitType inEditorContentUnitType = InEditorContentUnitType.None;
		
		private const int STYLE_INEDITOR_OVERLAY = 0;
		private const int STYLE_INEDITOR_DISMISS_BUTTON = 1;
		private const int STYLE_INEDITOR_CONTENTUNIT_LANDSCAPE_GENERIC = 2;
		private const int STYLE_INEDITOR_CONTENTUNIT_PORTRAIT_GENERIC = 3;
		private const int STYLE_INEDITOR_CONTENTUNIT_LANDSCAPE_REWARD = 4;
		private const int STYLE_INEDITOR_CONTENTUNIT_PORTRAIT_REWARD = 5;
		
		private ScreenOrientation screenOrientation = ScreenOrientation.Unknown;
		private Vector3 scaleVector = Vector3.one;
		private Matrix4x4 guiMatrix;
		private Vector2 guiScaler;
		private Rect dismissButtonRect;
		private Rect backgroundRect;
			
		void DetermineInEditorDevice()
		{
			const float DEFAULT_SCREEN_WIDTH = 640;
			const float DEFAULT_SCREEN_HEIGHT = 480;
			
			// determine orientation
			if (Screen.width > Screen.height)
				screenOrientation = ScreenOrientation.Landscape;
			else
				screenOrientation = ScreenOrientation.Portrait;
			
			// setup scaler
			guiScaler.x = screenOrientation == ScreenOrientation.Landscape ? Screen.width / DEFAULT_SCREEN_WIDTH : Screen.width / DEFAULT_SCREEN_HEIGHT;
			guiScaler.y = screenOrientation == ScreenOrientation.Landscape ? Screen.height / DEFAULT_SCREEN_HEIGHT : Screen.height / DEFAULT_SCREEN_WIDTH;
			guiMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(guiScaler.x, guiScaler.y, 1));
			
			// setup rects
			if (screenOrientation == ScreenOrientation.Landscape)
			{
				backgroundRect = new Rect(50, 50, DEFAULT_SCREEN_WIDTH - 100, DEFAULT_SCREEN_HEIGHT - 100);
				dismissButtonRect = new Rect(430, 400, 150, 64);
			}
			else
			{
				backgroundRect = new Rect(50, 50, DEFAULT_SCREEN_HEIGHT - 100, DEFAULT_SCREEN_WIDTH - 100);
				dismissButtonRect = new Rect(275, 555, 150, 64);
			}
		}
		
		void OnGUI()
		{
			if (!showContentUnitsInEditor) return;
			#if UNITY_IPHONE || UNITY_ANDROID
			
			if (integrationSkin == null) return;
			
			if (inEditorContentUnitType != InEditorContentUnitType.None)
			{
				GUI.depth = -10;
				
				// overlay
				GUI.Label(new Rect(0,0,Screen.width,Screen.height), string.Empty, integrationSkin.customStyles[STYLE_INEDITOR_OVERLAY]);
				
				// set matrix
				GUI.matrix = guiMatrix;
				
				// set background image
				int backgroundStyleIndex = (inEditorContentUnitType == InEditorContentUnitType.Reward) ? STYLE_INEDITOR_CONTENTUNIT_LANDSCAPE_REWARD : STYLE_INEDITOR_CONTENTUNIT_LANDSCAPE_GENERIC;
				if (screenOrientation == ScreenOrientation.Portrait)
					backgroundStyleIndex = (inEditorContentUnitType == InEditorContentUnitType.Reward) ? STYLE_INEDITOR_CONTENTUNIT_PORTRAIT_REWARD : STYLE_INEDITOR_CONTENTUNIT_PORTRAIT_GENERIC;
				
				// draw background rect
				GUI.Label(backgroundRect, string.Empty, integrationSkin.customStyles[backgroundStyleIndex]);
				
				// set label text based on content type
				string buttonLabel = "Dismiss";
				switch (inEditorContentUnitType)
				{
				case InEditorContentUnitType.Generic:
					break;
				case InEditorContentUnitType.CrossPromotionWidget:
					break;
				case InEditorContentUnitType.Announcement:
					break;
				case InEditorContentUnitType.Advertisement:
					break;
				case InEditorContentUnitType.Reward:
					buttonLabel = "Give Me!";
					break;
				}
				
				// draw dismiss button
				if (GUI.Button(dismissButtonRect, buttonLabel, integrationSkin.customStyles[STYLE_INEDITOR_DISMISS_BUTTON]))
					inEditorContentUnitType = InEditorContentUnitType.None;
			}
			#endif
		}
			
		#endif
		#endregion
		
		#if ENABLE_INTERNAL_TESTING
		public Hashtable GetDelegatesList()
		{
			Hashtable deletageHash = new Hashtable(16);
			
			deletageHash.Add("OnRequestCompleted", 				(OnRequestCompleted != null 			? OnRequestCompleted.GetInvocationList() 			: null));
			deletageHash.Add("OnBadgeUpdate", 					(OnBadgeUpdate != null 					? OnBadgeUpdate.GetInvocationList() 				: null));
			deletageHash.Add("OnRewardGiven", 					(OnRewardGiven != null 					? OnRewardGiven.GetInvocationList() 				: null));
			deletageHash.Add("OnWillDisplayContent", 			(OnWillDisplayContent != null 			? OnWillDisplayContent.GetInvocationList() 			: null));
			deletageHash.Add("OnDidDisplayContent", 			(OnDidDisplayContent != null 			? OnDidDisplayContent.GetInvocationList()			: null));
			deletageHash.Add("OnSuccessOpenRequest", 			(OnSuccessOpenRequest != null 			? OnSuccessOpenRequest.GetInvocationList() 			: null));
			deletageHash.Add("OnSuccessPreloadRequest", 		(OnSuccessPreloadRequest != null 		? OnSuccessPreloadRequest.GetInvocationList() 		: null));
			deletageHash.Add("OnDismissContent", 				(OnDismissContent != null 				? OnDismissContent.GetInvocationList() 				: null));
			deletageHash.Add("OnErrorOpenRequest", 				(OnErrorOpenRequest != null 			? OnErrorOpenRequest.GetInvocationList() 			: null));
			deletageHash.Add("OnErrorContentRequest", 			(OnErrorContentRequest != null 			? OnErrorContentRequest.GetInvocationList() 		: null));
			deletageHash.Add("OnErrorMetadataRequest", 			(OnErrorMetadataRequest != null 		? OnErrorMetadataRequest.GetInvocationList() 		: null));
			deletageHash.Add("OnPurchasePresented", 			(OnPurchasePresented != null 			? OnPurchasePresented.GetInvocationList() 			: null));
			deletageHash.Add("OnDismissCrossPromotionWidget", 	(OnDismissCrossPromotionWidget != null 	? OnDismissCrossPromotionWidget.GetInvocationList() : null));
			deletageHash.Add("OnErrorCrossPromotionWidget", 	(OnErrorCrossPromotionWidget != null 	? OnErrorCrossPromotionWidget.GetInvocationList() 	: null));
			deletageHash.Add("OnSuccessCancelRequest", 			(OnSuccessCancelRequest != null 		? OnSuccessCancelRequest.GetInvocationList() 		: null));
			deletageHash.Add("OnErrorCancelRequest", 			(OnErrorCancelRequest != null 			? OnErrorCancelRequest.GetInvocationList() 			: null));
			
			return deletageHash;
		}
		#endif
	}

#if !USE_NAMESPACE
namespace PlayHaven
{
#endif
	/** Triggered when a request has been completed.
	 * @param requrestId The hashcode of the completed request.
	 */
	public delegate void RequestCompletedHandler(int requestId);
	
	/** Triggered when the Badge has been updated.
	 * @param requrestId The hashcode of the badge request.
	 * @param badge The name of the current badge.
	 */
	public delegate void BadgeUpdateHandler(int requestId, string badge);
	
	/** Triggered when a reward content unit is triggered.
	 * @param requestId The hashcode of the reward.
	 * @param reward Type of reward.
	 */
	public delegate void RewardTriggerHandler(int requestId, Reward reward);
	
	/** Triggered when player decides to purchase a Playhaven IAP content unit.</summary>
	 * @param requestId The hashcode of the request.
	 * @param purchase Type of purchase.
	 */
	public delegate void PurchasePresentedTriggerHandler(int requestId, Purchase purchase);
	
	/** Triggered when a request has been completed successfully.
	 * @param requestId The hashcode of the request.
	 */
	public delegate void SuccessHandler(int requestId);
	
	/** Triggered when a content unit will be presented to the player.
	 * @param requestId The hashcode of the request.
	 */
	public delegate void WillDisplayContentHandler(int requestId);
	
	/** Triggered when a content unit has been displayed to the player.
	 * @param requestId The hashcode of the content request.
	 */
	public delegate void DidDisplayContentHandler(int requestId);
	
	/** Triggered when a content unit has been dismissed.
	 * @param requestId The hashcode of the dismissed content request.
	 * @param dismissType The Playhaven.DismissType of the dismissed request.
	 */
	public delegate void DismissHandler(int requestId, DismissType dismissType);
	
	//! Triggered when the Cross-Promotion Widget has been dismissed.
	public delegate void SimpleDismissHandler();
	
	/** Triggered when fetching a content unit has resulted in an error.
	 * @param requestId The hashcode of the content request.
	 * @param error The Error information for the failed request.
	 */
	public delegate void ErrorHandler(int requestId, Error error);
	
	/** Triggered when request for content originated from external scope was registered in manager
	 * @param requestId The hashcode of the registered request.
	 */
	public delegate void RequestRegisteredHandler(int requestId);
	
	//! Possible results of a user's purchase action.
	#if UNITY_ANDROID
	public enum PurchaseResolution
	{
		Bought,								//!< The item has been bought.
		Cancelled,							//!< The purchase was cancelled.
		Error,								//!< There was an error during the purchase process.
		Invalid,							//!< The requested item was invalid.
		Owned,								//!< The requested item was already owned by this user.
		Unset								//!< Result was not set.
	};
	#else
	public enum PurchaseResolution
	{
		Buy,								//!< The purchase was successfully completed.
		Cancel,								//!< The purchase was canceled before completion.
		Error								//!< The purchase could not be, or was not completed.
	};
	#endif
	
	//! Dismissal types handled by the Playhaven system.
	#if UNITY_ANDROID
	public enum DismissType
	{
		Unknown,							//!< Dismiss was of an unknown type.
		NoContent,							//!< There was no content to display.
			
		BackButton,							//!< Back button was pressed.
		Emergency,							//!< The user clicks the emergency close button.
		Launch,								//!< The user chooses to get something we've offered.
		NoThanks,							//!< The user clicks the HTML button to dismiss.
		OptIn,								//!< Opt-In data submitted.
		Purchase,							//!< Purchase clicked.
		Reward,								//!< Reward collected.
		SelfClose							//!< The view closed itself due to some error.
	};
	#else
	public enum DismissType
	{
		Unknown,															//!< Dismiss was of an unknown type.
		PHPublisherContentUnitTriggeredDismiss,								//!< Dismiss was triggered by the content unit.
		PHPublisherNativeCloseButtonTriggeredDismiss,						//!< Dismiss was triggered by the device's back button.
		PHPublisherApplicationBackgroundTriggeredDismiss,					//!< Dismiss was triggered by the OS putting the application to sleep.
		PHPublisherNoContentTriggeredDismiss,								//!< The content was null and was never displayed.
	};
	#endif
		
	//! The types of requests that are sent to the Playhaven server.
	public enum RequestType
	{
		Open,						//!< Sent when Playhaven is launched or resumed from suspended state
		Metadata,					//!< Request for metadata
		Content,					//!< Request for content
		Preload,					//!< Requset for preloading content
		CrossPromotionWidget		//!< Request for Cross-promotion Widget; the "More Games" window
	};
	
	/** %Error information.
	 * Contains error information returned by the Playhaven server.
	 */
	public class Error
	{
		public int code;							//!< The error code.
		public string description = string.Empty;	//!< A description of the error.
		
		/** Generates a string that represents the Error.
		 * @returns The error information in human-readable format.
		 */
		public override string ToString()
		{
			return "code: "+code+", description: "+description;
		}
	}
	
	/** %Reward information.
	 * Contains reward information returned by the Playhaven server.
	 */
	public class Reward
	{
		public string receipt;						//!< The receipt for this reward.
		public string name;							//!< The name of this reward.
		public int quantity;						//!< The quantity of virtual good items this reward should impart.
		
		/** Generates a string that represents the Reward.
		 * @returns The reward information in human-readable format.
		 */
		public override string ToString()
		{
			return "name: "+name+", quantity: "+quantity+", receipt: "+receipt;
		}
	}
	
	/** %Purchase information.
	 * Class containing purchase information returned by the PlayHaven server.
	 */
	public class Purchase
	{
		public string productIdentifier;			//!< The product identifier for the purchase.
		public int quantity;						//!< The quantity of virtual good items this purchase should impart.
		public string receipt;						//!< The receipt generated for the purchase.
			
		public string orderId;						//!< The order identifier.
		public double price;						//!< The price of the product.
		public string store;						//!< The name of the store the product is sold in.
		
		/** Generates a string that represents current Purchase.
		 * @returns The purchase information in human-readable format.
		 */  
		public override string ToString()
		{
			return "productIdentifier: "+productIdentifier+", quantity: "+quantity+", receipt: "+receipt;
		}
	}
}
