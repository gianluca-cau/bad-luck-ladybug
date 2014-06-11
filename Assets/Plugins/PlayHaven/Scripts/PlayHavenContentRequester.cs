using UnityEngine;
using System.Collections;

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

namespace PlayHaven
{
	[AddComponentMenu("PlayHaven/Content Requester")]
	
	/** PlayHavenContentRequester
	 * 
	 * The PlayHavenContentRequester (Requester) allows the selection of a specific placement and the configuration of several options either programatically or via the inspecter.
	 * The Requester creates requests from the application to the Playhaven server via in-scene <i>placements</i>. The Requester allows the calling of specific placements in specific
	 * locations in your game. By creating a triggering script that works with a trigger collider, you can have a ContentRequest made to PlayHaven when the player
	 * enters a particular zone.
	 * 
	 * There are several options that can be set via the inspecter:
	 * <list type="bullets">
	 * <item> <b>Placement</b>: The name of the placement defined on the PlayHaven Dashboard that is to be used when Request() is called.
	 * <item> <b>When to Request</b>: Specifies when to make the request. It can be made automatically or manually. If set to 'Manual', then you must call Request() from your code.
	 * <item> <b>Delay Request</b>: If checked, this option will delay the request n number of seconds, defined by the 'Seconds' option following it, after Request() is called.
	 * <item> <b>Prefetch</b>: Allows for automatic prefecthing a Content Unit at specific times in the Requester's lifecycle.
	 * <item> <b>Connection for Prefetch</b>: Defines when prefetching is allowed to occur.
	 * <item> <b>Refetch When Used</b>: If checked, the placement will be prefetched when the OnDismissContent event is raised.
	 * <item> <b>Loading Overlay</b>: Darkens the background and prevents user interaction while the Content Unit is being loaded and displayed.
	 * <item> <b>Rewardable</b>: If checked, the Requester is automatically setup to receive reward messages and send the defined message type.
	 * <item> <b>Message Type</b>: Defines which kind of message to send when OnRewardGiven is received.
	 * <item> <b>Test Def %Reward</b>: Allows the definition of a 'test reward' and it's quantity for implementation testing.
	 * <item> <b>Limited Use</b>: Limits the number of times a Requester can be used before it will be Exhaused and no longer make Content Requests.
	 * <item> <b>Max Uses</b>: The maximum number of times the Requester will be used before it is Exhaused.
	 * <item> <b>Exhaust Action</b>: Allows definition of what to do when the Requester is Exhausted.
	 * </list>
	 */
	public class PlayHavenContentRequester : MonoBehaviour
	{
		//! Settings for when automatic requesting will be made.
		public enum WhenToRequest
		{
			Awake,					//!< Requests will be made on Awake
			Start,					//!< Requests will be made on Start
			OnEnable,				//!< Requests will be made every time the Component is enabled
			OnDisable,				//!< Requests will be made every time the Component is disabled
			Manual					//!< Requests will never be made automatically; you will have to call Request on your own.
		};
		
		//! Settings that will prevent content requests according to network connectivity.
		public enum InternetConnectivity
		{
			WiFiOnly,				//!< Requests will only be made if the user is on a Wifi network
			CarrierNetworkOnly,		//!< Requests will only be made if the user is on a carrier network
			WiFiAndCarrierNetwork,	//!< Requests will only be made if the user is on a Wifi or carrier network.
			Always = 100			//!< Requests will always be made, regardless of internet connectivity.
		};
		
		//! Settings for where messaged will be propogated when sent.
		public enum MessageType
		{
			None,					//!< No messages will be sent.
			Send,					//!< Messages will be sent to the same GameObject that this script is placed on.
			Broadcast,				//!< Messages will be sent to the same GameObject that this script is placed on and all of it's children.
			Upwards					//!< Messages will be sent to the parent of the GameObject that this script is placed on.
		};
		
		//! Settings for handeling of exhausted placements.
		public enum ExhaustedAction
		{
			None,					//!< Nothing will happen when the placement is exhausted.
			DestroySelf,			//!< The placement will destroy itself when exhausted.
			DestroyGameObject,		//!< The placement will destroy its own GameObject when exhausted.
			DestroyRoot				//!< The placement will destroy the root GameObject when exhausted.
		};
		
		#region Attributes		
		//! Sets when to notify PlayHaven of the request.
		public WhenToRequest whenToRequest = WhenToRequest.Manual;
		
		/** The placemenet that will be requested.
		 * @remarks This value must match a valid placement as entered on the Playhaven Dashboard.
		 */
		public string placement = string.Empty;
		
		//! Sets when prefecthing notifications to Playhaven will occur.
		public WhenToRequest prefetch = WhenToRequest.Manual;
		
		/** Sets when prefecthing is allowed occur.
		 * This flag will disallow prefetching to occur when specific network connections are detected.
		 * @remarks It is reccomended that prefetching only be performed while on Wifi networks.
		 */
		public InternetConnectivity connectionForPrefetch = InternetConnectivity.WiFiOnly;
		
		//! Controls whether refetching of the content unit will be performed again automatically once the content is consumed by the user.
		public bool refetchWhenUsed = false;
		
		/** Placement specific flag for displaying the pop-up immediatly upon initiating a request.
		 * @remarks This setting is overridden by the maskShowsOverlayImmediately setting in PlayHavenManager.
		 */
		public bool showsOverlayImmediately = false;
		
		//! Prepares the system for the possiblity of a reward being given, but does not mean that one neccessarily will.
		public bool rewardMayBeDelivered = false;
		
		//! Sets the MessageType for reward messages.
		public MessageType rewardMessageType = MessageType.Broadcast;
		
		/** For testing purposes only.
		 * Allows developers to test giving rewards for early sanity testing. Will request the value of defaultTestRewardName.
		 */
		public bool useDefaultTestReward = false;
		
		/** For in-editor testing purposes only.
		 * Allows developers to set the default reward item which can be used for sanity testing in the Playhaven Dashboard.
		 */
		public string defaultTestRewardName = string.Empty;
		
		/** For in-editor testing purposes only.
		 * Sets the quantity of default reward items given.
		 */
		public int defaultTestRewardQuantity = 1;
		
		//! The amount of time which to delay the request by after it is made.
		public float requestDelay = 0;
		
		//public bool pauseGameWhenDisplayed = true;
		
		/** Allows a placement to expire.
		 * Allows certain placements to expire after being triggered <c>n</c> times
		 * as defined by PlayHavenContentRequester.maxUses.
		 * @remarks This flag is set in the %PlayHavenContentRequestor Component inspector window.
		 * At the same time the "Max Uses" and "Exhaust Action" can be set.
		 */
		public bool limitedUse = false;
		
		/** Maximim number of time this placement will be used.
		 * Sets the maximum number of times that this placement will be fulfilled by requests to the Playhaven server.
		 * Once the placement has reached it's maximim usage count the behavior of the placement will be determined
		 * by the setting of PlayHavenContentRequester.exhaustAction.
		 * @remarks The limitedUse flag must be set to <c>true</c> for this value to work.
		 */
		public int maxUses;
		
		//! Sets the action to take when this placement has been exhausted.
		public ExhaustedAction exhaustAction = ExhaustedAction.None;
		#endregion
		
		#region Unity
		void Awake()
		{		
			refetch = refetchWhenUsed;
			
			if (whenToRequest == WhenToRequest.Awake)
			{
				if (requestDelay > 0)
					Invoke("Request", requestDelay);
				else
					Request();
			}
			else if (prefetch == WhenToRequest.Awake)
			{
				PreFetch();
			}
		}
		
		void OnEnable()
		{
			if (whenToRequest == WhenToRequest.OnEnable)
			{
				if (requestDelay > 0)
					Invoke("Request", requestDelay);
				else
					Request();
			}
			else if (prefetch == WhenToRequest.OnEnable)
			{
				PreFetch();
			}
		}
		
		void OnDisable()
		{
			if (whenToRequest == WhenToRequest.OnDisable)
			{
				// not technically possible to delay a request that is set to
				// be called in OnDisable, so no Invoke() is implemented here
				Request();
			}
			else if (prefetch == WhenToRequest.OnDisable)
			{
				PreFetch();
			}
		}
		
		void OnDestroy()
		{
			if (Manager)
			{
				Manager.OnRewardGiven -= HandlePlayHavenManagerOnRewardGiven;
				Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
				Manager.OnErrorContentRequest -= HandlePlayHavenManagerContentRequestOnError;
				//Manager.OnWillDisplayContent -= HandleManagerOnWillDisplayContent;
				//Manager.OnDidDisplayContent -= HandleManagerOnDidDisplayContent;
			}
		}
		
		void Start()
		{
			if (whenToRequest == PlayHavenContentRequester.WhenToRequest.Start)
			{
				if (requestDelay > 0)
					Invoke("Request", requestDelay);
				else
					Request();
			}
			else if (prefetch == WhenToRequest.Start)
			{
				PreFetch();
			}
		}
		#endregion
		
		#region Properties
		/** Property for the request identifier.
		 * @returns the contentRequestId when not 0, otherwise the prefetchRequestId.
		 */
		public int RequestId
		{
			get { return (contentRequestId != 0 ? contentRequestId : prefetchRequestId); }
			private set { contentRequestId = prefetchRequestId = value; }
		}

		/** Specifies if the placement has been exhausted.
		 * @returns <c>true</c> if the placement is exhaused, otherwise <c>false</c> if still valid.
		 */
		public bool IsExhausted
		{
			get
			{
				return exhausted;
			}
		}
		#endregion
		
		#region Actions
		//! Performs a request for the placement defined by the ContentRequester instance.
		public void Request()
		{
			Request(refetchWhenUsed);
		}
		
		/** Performs a request for the placement defined by the ContentRequester instance.
		 * @params refetch Passing a value of <c>true</c> will cause the content to be refetched once it is consumed.
		 */
		public void Request(bool refetch)
		{
			StartCoroutine(RequestCoroutine(refetch));
		}
		
		/** Prefetchs a Content Unit.
		 * Prefetches a Content Unit and makes ready for consumption prior to being triggered for display.
		 * This method is called when the 'Refetch When Used' checkbox is set in the inspector, or when
		 * refetchWhenUsed is true.
		 */
		public void PreFetch()
		{
			RequestId = 0;
			bool connectivityPermitted = true;
			switch (connectionForPrefetch)
			{
			case InternetConnectivity.WiFiOnly:
				#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				connectivityPermitted = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
				#else
				connectivityPermitted = iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaWiFiNetwork;
				#endif
				break;
			case InternetConnectivity.CarrierNetworkOnly:
				#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				connectivityPermitted = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork;
				#else
				connectivityPermitted = iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaCarrierDataNetwork;
				#endif
				break;
			case InternetConnectivity.WiFiAndCarrierNetwork:
				#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5
				connectivityPermitted = Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork || 
								        Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
				#else
				connectivityPermitted = iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaCarrierDataNetwork || 
								        iPhoneSettings.internetReachability == iPhoneNetworkReachability.ReachableViaWiFiNetwork;
				#endif
				break;
			}
			if (!connectivityPermitted) return;
			
			if (prefetchIsInProgress)
			{
				if (Debug.isDebugBuild)
					Debug.Log("prefetch request is in progress; not making another request");
				return;
			}
			
			if (Manager)
			{
				if (placement.Length > 0)
				{
					prefetchIsInProgress = true;
					Manager.OnSuccessPreloadRequest += HandleManagerOnSuccessPreloadRequest;
					Manager.OnDismissContent += HandlePlayHavenManagerOnDismissContent;
					if (Debug.isDebugBuild)
						Debug.Log("Making content preload request for placement: "+placement);
					prefetchRequestId = Manager.ContentPreloadRequest(placement);
				}
				else if (Debug.isDebugBuild)
					Debug.LogError("placement value not set in PlayHaventContentRequester");
			}
			//else if (Debug.isDebugBuild)
			//	Debug.LogError("PlayHaven manager is not available in the scene. Content requests cannot be initiated.");
		}

		/** Manually give the specified reward.
		 */
		public void GiveReward(Reward reward)
		{
			HandlePlayHavenManagerOnRewardGiven(-1, reward);
		}		
		#endregion
		
		#region Handlers
		void HandleManagerOnSuccessPreloadRequest(int requestId)
		{
			if (requestId == RequestId)
			{
				prefetchIsInProgress = false;
				if (Debug.isDebugBuild)
					Debug.Log("prefetch of placement successful: "+placement);
				
				Manager.OnSuccessPreloadRequest -= HandleManagerOnSuccessPreloadRequest;
				Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
			}
		}

		void HandlePlayHavenManagerContentRequestOnError(int hash, Error error)
		{
			if (Manager && contentRequestId == hash)
			{
				Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
				Manager.OnErrorContentRequest -= HandlePlayHavenManagerContentRequestOnError;
				Manager.OnSuccessPreloadRequest -= HandleManagerOnSuccessPreloadRequest;
				
				if (rewardMayBeDelivered)
					Manager.OnRewardGiven -= HandlePlayHavenManagerOnRewardGiven;
			}
		}
		
		void HandlePlayHavenManagerOnRewardGiven(int hashCode, Reward reward)
		{
			if (contentRequestId == hashCode || hashCode == -1)
			{
				switch (rewardMessageType)
				{
				case MessageType.Broadcast:
					BroadcastMessage("OnPlayHavenRewardGiven", reward);
					break;
				case MessageType.Send:
					SendMessage("OnPlayHavenRewardGiven", reward);
					break;
				case MessageType.Upwards:
					SendMessageUpwards("OnPlayHavenRewardGiven", reward);
					break;
				}
				
				Manager.OnRewardGiven -= HandlePlayHavenManagerOnRewardGiven;
			}
		}
		
		void HandlePlayHavenManagerOnDismissContent(int hashCode, PlayHaven.DismissType dismissType)
		{
			if (RequestId == hashCode)
			{
				requestIsInProgress = false;
				
#if UNITY_ANDROID
				if (prefetchIsInProgress && dismissType == DismissType.NoContent)
				{
					HandleManagerOnSuccessPreloadRequest(RequestId);
					return;
				}
#else
				if (prefetchIsInProgress && dismissType == DismissType.PHPublisherNoContentTriggeredDismiss)
				{
					HandleManagerOnSuccessPreloadRequest(RequestId);
					return;
				}
#endif
				
				if (Manager)
				{
					Manager.OnDismissContent -= HandlePlayHavenManagerOnDismissContent;
					Manager.OnErrorContentRequest -= HandlePlayHavenManagerContentRequestOnError;
					Manager.OnSuccessPreloadRequest -= HandleManagerOnSuccessPreloadRequest;
				}
				switch (rewardMessageType)
				{
				case MessageType.Broadcast:
					BroadcastMessage("OnPlayHavenContentDismissed", dismissType, SendMessageOptions.DontRequireReceiver);
					break;
				case MessageType.Send:
					SendMessage("OnPlayHavenContentDismissed", dismissType, SendMessageOptions.DontRequireReceiver);
					break;
				case MessageType.Upwards:
					SendMessageUpwards("OnPlayHavenContentDismissed", dismissType, SendMessageOptions.DontRequireReceiver);
					break;
				}
				
				if (!exhausted && limitedUse && uses >= maxUses)
				{
					Exhaust();
				}
#if UNITY_ANDROID
				else if (refetch && dismissType != DismissType.NoContent)
				{
					PreFetch();
				}
#else
				else if (refetch && dismissType != DismissType.PHPublisherNoContentTriggeredDismiss)
				{
					PreFetch();
				}
#endif
			}
		}
		#endregion
		
		#region Private
		private PlayHavenManager playHaven;
		private bool exhausted;
		private int uses;
		private int contentRequestId;
		private int prefetchRequestId;
		private bool requestIsInProgress;
		private bool prefetchIsInProgress;
		private bool refetch;

		private PlayHavenManager Manager
		{
			get
			{
				if (!playHaven)
					playHaven = PlayHavenManager.Instance;
				return playHaven;
			}
		}

		private void RequestPlayHavenContent()
		{
			if (requestDelay > 0)
				Invoke("Request", requestDelay);
			else
				Request();
		}

		private void Exhaust()
		{
			exhausted = true;
			switch (exhaustAction)
			{
			case ExhaustedAction.DestroySelf:
				Destroy(this);
				break;
			case ExhaustedAction.DestroyGameObject:
				Destroy(gameObject);
				break;
			case ExhaustedAction.DestroyRoot:
				Destroy(transform.root.gameObject);
				break;
			}
		}
		
		private IEnumerator RequestCoroutine(bool refetch)
		{
			if (whenToRequest == WhenToRequest.Manual && requestDelay > 0)
				yield return new WaitForSeconds(requestDelay);
			
			if (requestIsInProgress)
			{
				if (Debug.isDebugBuild)
					Debug.Log("request is in progress; not making another request");
				yield break;
			}
			
			if (exhausted)
			{
				if (Application.isEditor)
					Debug.LogWarning("content requester has been exhausted");
				yield break;
			}
			
			if (Manager.IsPlacementSuppressed(placement))
			{
				if (Debug.isDebugBuild)
					Debug.LogWarning("content requester is suppressed");
				yield break;
			}
			
			this.refetch = refetch;
			
			if (Manager)
			{
				if (placement.Length > 0)
				{
					Manager.OnDismissContent += HandlePlayHavenManagerOnDismissContent;
					Manager.OnErrorContentRequest += HandlePlayHavenManagerContentRequestOnError;
					
					/*
					if (pauseGameWhenDisplayed)
					{
						//Manager.OnWillDisplayContent += HandleManagerOnWillDisplayContent;
						Manager.OnDidDisplayContent += HandleManagerOnDidDisplayContent;
					}
					*/
					
					if (rewardMayBeDelivered)
					{
						Manager.OnRewardGiven -= HandlePlayHavenManagerOnRewardGiven;
						Manager.OnRewardGiven += HandlePlayHavenManagerOnRewardGiven;
					}
					
					#if UNITY_EDITOR
					contentRequestId = Manager.ContentRequest(placement, showsOverlayImmediately, this);
					if (useDefaultTestReward && defaultTestRewardName.Length > 0)
					{
						Reward reward = new Reward();
						reward.name = defaultTestRewardName;
						reward.quantity = defaultTestRewardQuantity;
						HandlePlayHavenManagerOnRewardGiven(contentRequestId, reward);
					}
					#else
					requestIsInProgress = true;
					contentRequestId = Manager.ContentRequest(placement, showsOverlayImmediately);
					#endif
				}
				else if (Debug.isDebugBuild)
					Debug.LogError("placement value not set in PlayHaventContentRequester");
			}
			//else if (Debug.isDebugBuild)
			//	Debug.LogWarning("PlayHaven manager is not available in the scene. Content requests cannot be initiated.");
			
			uses++;
			if (limitedUse && !rewardMayBeDelivered && uses >= maxUses)
			{
				Exhaust();
			}
		}
		#endregion
	}
}
