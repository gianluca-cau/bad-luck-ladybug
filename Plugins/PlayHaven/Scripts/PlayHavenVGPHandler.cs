//#define USE_GENERICS
using UnityEngine;
using System.Collections;
#if USE_GENERICS
using System.Collections.Generic;
#endif

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
	[AddComponentMenu("PlayHaven/VGP Handler")]
	
	//! Handles notifying Playhaven of Virtual Goods Promotions that the user has purchased.
	public class PlayHavenVGPHandler : MonoBehaviour 
	{
		#region Events
		/** Triggered on VGP purchase.
		 * Triggered by the user's intent to purchase a Virtual Good which was presented by the Playhaven system.
		 * @param requestId The ID of the content request.
		 * @param purchase Information describing the item to be purchased.
		 */
		public delegate void PurchaseEventHandler(int requestId, Purchase purchase);
		
		/** Triggered when purchase is presented.
		 * Triggered when a request has resulted in the user deciding to purchase an item presented within a Virtual Goods Promotion pop-up.
		 */
		public event PurchaseEventHandler OnPurchasePresented;
		#endregion
		
		#region Unity
		void Awake()
		{
			playHaven = PlayHavenManager.Instance;
		}
		
		void OnEnable()
		{
			playHaven.OnPurchasePresented += PlayHavenOnPurchasePresented;
		}
		
		void OnDisable()
		{
			playHaven.OnPurchasePresented -= PlayHavenOnPurchasePresented;
		}
		#endregion
		
		#region Properties
		/** Gets the instance.
		 * Singleton pattern instance accessor; use this to get the instance of PlayHavenVGPHandler in the current scene.
		 * @returns The instance of the PlayHavenVGPHandler in the current scene.
		 */
		public static PlayHavenVGPHandler Instance
		{
			get
			{
				if (!instance)
					instance = GameObject.FindObjectOfType(typeof(PlayHavenVGPHandler)) as PlayHavenVGPHandler;
				return instance;
			}
		}
		#endregion
		
		#region Actions
		/* Resolves a purchase with Playhaven. 
		 * Sends a Product Purchase Tracking Request based on the provided requestId to Playhaven.
		 * @param requestId Request identifier.
		 * @param resolution The PlayHavenManager.PurchaseResolution type.
		 * @param track If <c>true</c> it will submit a tracking request to PlayHaven for user-segmentation purposes.
		 */
		public void ResolvePurchase(int requestId, PurchaseResolution resolution, bool track)
		{
			ResolvePurchase(requestId, resolution, null, track);
		}

		/* Resolves a purchase with Playhaven. 
		 * Sends a Product Purchase Tracking Request based on the provided requestId to Playhaven.
		 * @param requestId Request identifier.
		 * @param resolution The PlayHavenManager.PurchaseResolution type.
		 * @param track If <c>true</c> it will submit a tracking request to PlayHaven for user-segmentation purposes.
		 */
		public void ResolvePurchase(int requestId, PurchaseResolution resolution, byte[] receiptData, bool track)
		{
			#if !UNITY_ANDROID
			if (purchases.ContainsKey(requestId))
			{
				#if USE_GENERICS
				Purchase purchase = purchases[requestId];
				#else
				Purchase purchase = (Purchase)purchases[requestId];
				#endif
				purchases.Remove(requestId);
				#if !UNITY_ANDROID
				playHaven.ProductPurchaseResolutionRequest(resolution);
				#endif
				if (track)
					playHaven.ProductPurchaseTrackingRequest(purchase, resolution, receiptData);
			}
			else if (Debug.isDebugBuild)
			{
				Debug.LogWarning("PlayHaven VGP handler does not have a record of a purchase with the provided request identifier: "+requestId);
			}
			#endif
		}

		/* Resolves a purchase with Playhaven.
		 * Sends a Product Purchase Tracking Request based on the provided Purchase to Playhavent for user-segmentation purposes.
		 * @param purchase The instance of Purchase that will be sent to the PlayHaven server for tracking.
		 * @param resolution The PlayHavenManager.PurchaseResolution type.
		 * @param track If <c>true</c> it will submit a tracking request to PlayHaven for user-segmentation purposes.
		 */
		public void ResolvePurchase(Purchase purchase, PurchaseResolution resolution, bool track)
		{
			ResolvePurchase(purchase, resolution, null, track);
		}
		
		/* Resolves a purchase with Playhaven.
		 * Sends a Product Purchase Tracking Request based on the provided Purchase to Playhavent for user-segmentation purposes.
		 * @param purchase The instance of Purchase that will be sent to the PlayHaven server for tracking.
		 * @param resolution The PlayHavenManager.PurchaseResolution type.
		 * @param track If <c>true</c> it will submit a tracking request to PlayHaven for user-segmentation purposes.
		 */
		public void ResolvePurchase(Purchase purchase, PurchaseResolution resolution, byte[] receiptData, bool track)
		{
			if (!purchases.ContainsValue(purchase))
			{
				if (Debug.isDebugBuild)
					Debug.LogWarning("PlayHaven VGP handler does not have a record of a purchase with the provided purchase object; will track only if requested.");
				if (track)
					playHaven.ProductPurchaseTrackingRequest(purchase, resolution, receiptData);
			}
			else
			{
				int requestId = -1;
				foreach (int rid in purchases.Keys)
				{
					if (purchases[rid] == purchase)
					{
						requestId = rid;
						break;
					}
				}
				if (requestId > -1)
				{
					purchases.Remove(requestId);
					#if !UNITY_ANDROID
					playHaven.ProductPurchaseResolutionRequest(resolution);
					#endif
					if (track)
						playHaven.ProductPurchaseTrackingRequest(purchase, resolution, receiptData);
				}
				else
					Debug.LogError("Unable to determine request identifier for provided purchase object.");
			}
		}
		#endregion
		
		#region Handlers
		void PlayHavenOnPurchasePresented(int requestId, Purchase purchase)
		{
			if (OnPurchasePresented != null)
			{
				purchases.Add(requestId, purchase);
				OnPurchasePresented(requestId, purchase);
			}
		}
		#endregion
		
		#region Private
		private static PlayHavenVGPHandler instance;
		private PlayHavenManager playHaven;
		#if USE_GENERICS
		private Dictionary<int, PlayHaven.Purchase> purchases = new Dictionary<int, PlayHaven.Purchase>(4);
		#else
		private Hashtable purchases = new Hashtable(4);
		#endif
		#endregion		
	}
}