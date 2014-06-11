package com.playhaven.unity3d;

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

import com.playhaven.android.*;
import com.playhaven.android.compat.UnityCompat;
import com.playhaven.android.view.*;
import com.playhaven.android.req.*;
import com.playhaven.android.data.*;
import com.playhaven.android.push.GCMRegistrationRequest;
import com.unity3d.player.UnityPlayer;
import android.app.Activity;
import android.util.Log;
import android.graphics.Bitmap;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.net.Uri;
import org.json.*;
import java.util.HashMap;
import java.lang.Runnable;

public class PlayHavenFacade
{	
    public final static String UNITY_SDK_VERSION = "android-unity-2.0.1";
    public final static String TAG = PlayHavenFacade.class.getSimpleName();
    	
	/**
	  * Constructor.
	  * @param currentActivity The current activity.
      * @param token The application token, obtained from the PlayHaven web dashboard for your game.
      * @param secret The application secret, obtained from the PlayHaven web dashboard for your game.
	  */
	public PlayHavenFacade(final Activity currentActivity, String token, String secret, String googleCloudMessagingProjectNumber)
	{		
    	PlayHaven.setVendorCompat(currentActivity, new UnityCompat(UNITY_SDK_VERSION));
		setCurrentActivity(currentActivity);
		setKeys(token, secret, googleCloudMessagingProjectNumber);
	}
	
	/**
	  * Set the current activity.  This is needed so that asynchronous code can be
	  * executed on the UI thread.
	  * @param currentActivity The current activity.
	  */
	public void setCurrentActivity(final Activity currentActivity)
	{
		this.currentActivity = currentActivity;
	}

	/**
	  * Get information from the current intent, if any
	  */
	public String getCurrentIntentData()
	{
		if(currentActivity == null) return null;
		
		String resp = null;
		
		Intent intent = UnityPlayer.currentActivity.getIntent(); // NOT currentActivity.getIntent();
		if(intent == null) return null;
		JSONArray array = new JSONArray();

		Bundle adData = intent.getBundleExtra(PlayHavenView.BUNDLE_DATA);
		if(adData != null)
		{
			for(String key : adData.keySet())
			{
				if(PlayHavenView.BUNDLE_DATA_REWARD.equals(key))
				{
					for(Reward reward : adData.<Reward>getParcelableArrayList(PlayHavenView.BUNDLE_DATA_REWARD))
					{
						HashMap<String, Object> hash = new HashMap<String, Object>(4);
						hash.put("type", "reward");
						hash.put("name", reward.getTag());
						hash.put("quantity", reward.getQuantity().intValue());
						hash.put("receipt", reward.getReceipt().toString());
						JSONObject json = new JSONObject(hash);
						array.put(json);
					}	
				}else if(PlayHavenView.BUNDLE_DATA_PURCHASE.equals(key)){
					for(Purchase purchase : adData.<Purchase>getParcelableArrayList(PlayHavenView.BUNDLE_DATA_PURCHASE))
					{
						HashMap<String, Object> hash = new HashMap<String, Object>(4);
						hash.put("type", "purchase");
						hash.put("productIdentifier", purchase.getSKU());
						hash.put("quantity", purchase.getQuantity());
						hash.put("receipt", purchase.getReceipt());
						hash.put("orderId", purchase.getOrderId());
						hash.put("price", purchase.getPrice());
						hash.put("store", purchase.getStore());
						JSONObject json = new JSONObject(hash);
						array.put(json);
					}	
				}else if(PlayHavenView.BUNDLE_DATA_OPTIN.equals(key)){
					for(DataCollectionField field : adData.<DataCollectionField>getParcelableArrayList(PlayHavenView.BUNDLE_DATA_OPTIN))
					{
						HashMap<String, Object> hash = new HashMap<String, Object>(4);
						hash.put("type", "optin");
						hash.put(field.getName(), field.getValue());
						JSONObject json = new JSONObject(hash);
						array.put(json);					
					}
				}
			}
			// Prevent rewards (etc) from being awarded twice
			intent.removeExtra(PlayHavenView.BUNDLE_DATA);
		}

		Uri uri = intent.getData();
		if(uri != null)
		{
			HashMap<String,Object> hash = new HashMap<String,Object>();
			hash.put("type", "uri");
			hash.put("uri", uri.toString());
			JSONObject json = new JSONObject(hash);
			array.put(json);
			intent.setData(Uri.EMPTY);
		}

		UnityPlayer.currentActivity.setIntent(intent);
		return array.toString();
	}
	
	
    /**
     * Set the token and secret keys with the Android SDK.
     * @param token The application token, obtained from the PlayHaven web dashboard for your game.
     * @param secret The application secret, obtained from the PlayHaven web dashboard for your game.
     */
    public void setKeys(String token, String secret, String googleCloudMessagingProjectNumber)
    {
 		Log.d(TAG, "setKeys");
        try 
		{
			if (googleCloudMessagingProjectNumber == null || googleCloudMessagingProjectNumber.length() == 0)
				PlayHaven.configure(this.currentActivity, token, secret);
			else
				PlayHaven.configure(this.currentActivity, token, secret, googleCloudMessagingProjectNumber);
        } 
		catch (PlayHavenException e)
		{
        	Log.e(TAG, "Could not configure v2 of PlayHaven Android SDK");
        	e.printStackTrace();
        }
    }

    /**
     * Set the opt-out status.
     * @param option true if opted-out, false otherwise (the default) 
     */
    public void setOptOut(boolean option)
    {
		// Log.d(TAG, "setOptOut");
		try{
			PlayHaven.setOptOut(this.currentActivity, option);
		}catch(PlayHavenException e){
			Log.e(TAG, "Could not set opt out status");
			e.printStackTrace();
		}
    }
	
    /**
     * Get the opt-out status.
     * @return true if opted out 
     */
    public boolean getOptOut()
    {
		// Log.d(TAG, "getOptOut");
		try{
			return PlayHaven.getOptOut(this.currentActivity);
		}catch(PlayHavenException e){
			Log.e(TAG, "Could not get opt out status");
			e.printStackTrace();
		}
		
		// Default
		return false;
    }
	


	/**
	 * Send the Open() request to PlayHaven, notifying the system that the game
	 * has launched.
     * @param hash A hash value that uniquely identifies the request.
	 */
	public void openRequest(int id)
	{
 		Log.d(TAG, "openRequest");
        new OpenRequestRunner().run(currentActivity, id);
	}
	
	/**
	 * Send a metadata request to PlayHaven.
     * @param hash A hash value that uniquely identifies the request.
     * @param placement The placement to associate the metadata to.
	 */
	public void metaDataRequest(int hash, String placement)
	{
 		Log.d(TAG, String.format("metaDataRequest: %s", placement));
		new MetadataRequestRunner().run(currentActivity, hash, placement);
	}
	
	/**
	 * Send a content request to PlayHaven.
     * @param hash A hash value that uniquely identifies the request.
     * @param placement The placement to associate the metadata to.
	 */
	public void contentRequest(int id, String placement)
	{
 		Log.d(TAG, String.format("contentRequest: %s", placement));	
		new ContentRequestRunner().run(currentActivity, id, placement);
	}

	/**
	 * Send a content preload request to PlayHaven.
     * @param hash A hash value that uniquely identifies the request.
     * @param placement The placement to associate the metadata to.
	 */
	public void preloadRequest(int hash, String placement)
	{
 		Log.d(TAG, String.format("preloadRequest: %s", placement));
		new PreloadRequestRunner().run(currentActivity, hash, placement);
	}

	/**
	 * Submit an IAP tracking request.
	 */
	public void iapTrackingRequest(String productId, String orderId, int quantity, double price, String store, int resolution)
	{
 		Log.d(TAG, String.format("iapTrackingRequest: %s (quantity=%d)", productId, quantity));
		com.playhaven.android.data.Purchase purchase = new com.playhaven.android.data.Purchase();
        purchase.setSKU(productId);
        purchase.setPrice(price);
        purchase.setQuantity(quantity);
		com.playhaven.android.data.Purchase.Result result = com.playhaven.android.data.Purchase.Result.values()[resolution];
        purchase.setResult(result);
		if (store != null)
		{
        	purchase.setStore(store);
		}
		if (orderId != null)
		{
        	purchase.setPayload(TAG + ":" + orderId);
        	purchase.setOrderId(orderId);
		}
		new PurchaseTrackingRequestRunner().run(currentActivity, purchase);
	}

	/**
 	  * Register for push notifications.
 	  */
	public void registerForPushNotifications()
	{
		new GCMRegistrationRequestRunner().run(currentActivity);
	}

	/**
 	  * Deregister from push notifications.
 	  */
	public void deregisterFromPushNofications()
	{
		new GCMDeregistrationRequestRunner().run(currentActivity);		
	}
    
	private Activity currentActivity;
	private HashMap<Integer, Placement> preloadedPlacements = new HashMap<Integer, Placement>(16);

	private class RequestRunner implements Runnable
	{
		private PlayHavenRequest request;
		private Context context;
		
		public void run(final Activity currentActivity, final PlayHavenRequest request)
		{
			this.request = request;
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
            request.send(context);
		}        
	}

	private class OpenRequestRunner implements Runnable, RequestListener
	{
		private Context context;
        private int id;
		
		public void run(final Activity currentActivity, int id)
		{
            this.id = id;
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
            OpenRequest open = new OpenRequest();
            open.setResponseHandler(this);
            open.send(context);
		}

        @Override
        public void handleResponse(String json)
        {
            Log.d(TAG, "completed open request");
			HashMap<String, Object> data = new HashMap<String, Object>(2);
			data.put("name", "success");
			data.put("hash", id);
			JSONObject responseData = new JSONObject(data);
            
			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
        }
        
        @Override
        public void handleResponse(PlayHavenException e)
        {
            Log.e(TAG, "failed to perform open request", e);
        }
	}

	private class MetadataRequestRunner implements Runnable, RequestListener
	{
		private Context context;
        private int id;
		private String placement;
		
		public void run(final Activity currentActivity, int id, String placement)
		{
            this.id = id;
			this.placement = placement;
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
            MetadataRequest meta = new MetadataRequest(placement);
            meta.setResponseHandler(this);
            meta.send(context);
		}

        @Override
        public void handleResponse(String json)
        {
            Log.d(TAG, String.format("completed metadata request: %s", json));

			HashMap<String, Object> payload = new HashMap<String, Object>(2);
			payload.put("type", "badge");
			payload.put("value", 1); // TEMP VALUE!
            JSONObject payloadData = new JSONObject(payload);

			HashMap<String, Object> notification = new HashMap<String, Object>(1);
			notification.put("notification", payloadData);
            JSONObject noteData = new JSONObject(notification);

			HashMap<String, Object> data = new HashMap<String, Object>(4);
			data.put("name", "success");
			data.put("hash", id);
			data.put("data", noteData);
			JSONObject responseData = new JSONObject(data);
            
			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
        }
        
        @Override
        public void handleResponse(PlayHavenException e)
        {
            Log.e(TAG, "failed to perform metadata request", e);
        }
	}
    
	private class ContentRequestRunner implements Runnable, PlayHavenListener, PlacementListener
	{
        private int id;
		private String placementTag;
		private Context context;
		private int stage = STAGE_PRELOAD;
		private Placement placement;
		
		private final static int STAGE_PRELOAD = 0;
		private final static int STAGE_SHOW = 1;
		
		public void run(final Activity currentActivity, int id, final String placementTag)
		{
            this.id = id;
			this.placementTag = placementTag;
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
			Placement placement = null;
            
			switch (stage)
			{
				case STAGE_PRELOAD:
					// first check to see if the placement was pre-loaded and use it instead of fetching a new one
					Integer preloadedPlacementId = null;
					for (Integer id : preloadedPlacements.keySet())
					{
						Placement preloadedPlacement = preloadedPlacements.get(id);
						if (preloadedPlacement.getPlacementTag().equals(placementTag))
						{
							placement = preloadedPlacement;
							preloadedPlacementId = id;
							break;
						}
					}
					if (preloadedPlacementId != null)
					{
		                Log.d(TAG, String.format("%s was pre-loaded; showing it immediately", placementTag));
						preloadedPlacements.remove(preloadedPlacementId);
						notifyWillDisplay(id);
	            		Windowed dialog = new Windowed(context, placement, this);
	            		dialog.show();
						notifyDidDisplay(id);
					}
					else
					{
	            		placement = new Placement(placementTag);
	            		placement.setListener(this);
						placement.preload(context);						
					}
					break;
				case STAGE_SHOW:
					placement = this.placement;				
					if (placement != null)
					{
	            		Windowed dialog = new Windowed(context, placement, this);
	            		dialog.show();
					}
					else
		                Log.e(TAG, String.format("%s failed to display: placement object was null", placementTag));
					break;
			}
		}

        @Override
        public void viewFailed(PlayHavenView view, PlayHavenException exception)
        {
            if(NoContentException.class.isInstance(exception))
            {
                Log.d(TAG, String.format("%s had no content", view.getPlacementTag()));

                HashMap<String, Object> typeHash = new HashMap<String, Object>(1);
                typeHash.put("type", "NoContent");
                JSONObject typeData = new JSONObject(typeHash);
                
                HashMap<String, Object> map = new HashMap<String, Object>(4);
                map.put("data", typeData);
                map.put("name", "dismiss");
                map.put("hash", id);
                JSONObject responseData = new JSONObject(map);
                UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
            }
            else
            {
                Log.e(TAG, String.format("%s failed to display", view.getPlacementTag()));
				notifyError(id, exception.getMessage());
            }
        }
        
        @Override
        public void viewDismissed(PlayHavenView view, PlayHavenView.DismissType dismissType, Bundle data)
        {
            Log.d(TAG, String.format("Request for %s was dismissed: %s", view.getPlacementTag(), dismissType));

            JSONObject responseData;
            HashMap<String, Object> map = new HashMap<String, Object>(4);
	    if (data != null && data.keySet() != null)
	    {
            Log.d(TAG, String.format("Checking bundle data for %s", view.getPlacementTag()));
            for(String key : data.keySet())
	    {
		if(PlayHavenView.BUNDLE_DATA_REWARD.equals(key))
		{
            		Log.d(TAG, String.format("Processing reward bundle data for %s", view.getPlacementTag()));
			for(Reward reward : data.<Reward>getParcelableArrayList(PlayHavenView.BUNDLE_DATA_REWARD))
			{
				HashMap<String, Object> rewardHash = new HashMap<String, Object>(4);
				rewardHash.put("name", reward.getTag());
				rewardHash.put("quantity", reward.getQuantity());
				rewardHash.put("receipt", reward.getReceipt());
				JSONObject rewardData = new JSONObject(rewardHash);
                    
				map.clear();
				map.put("data", rewardData);
				map.put("name", "reward");
				map.put("hash", id);
				responseData = new JSONObject(map);
                    
				UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
			}
		}else if(PlayHavenView.BUNDLE_DATA_PURCHASE.equals(key)){
            		Log.d(TAG, String.format("Processing purchase bundle data for %s", view.getPlacementTag()));
			for(Purchase purchase : data.<Purchase>getParcelableArrayList(PlayHavenView.BUNDLE_DATA_PURCHASE))
			{
				HashMap<String, Object> purchaseHash = new HashMap<String, Object>(4);
				purchaseHash.put("productIdentifier", purchase.getSKU());
				purchaseHash.put("name", purchase.getTitle());
				purchaseHash.put("price", purchase.getPrice());
				purchaseHash.put("store", purchase.getStore());
				purchaseHash.put("quantity", purchase.getQuantity());
				purchaseHash.put("receipt", purchase.getReceipt());
				JSONObject purchaseData = new JSONObject(purchaseHash);

				map.clear();
				map.put("data", purchaseData);
				map.put("name", "purchasePresentation");
				map.put("hash", id);
				responseData = new JSONObject(map);

				UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
			}
		}else if(PlayHavenView.BUNDLE_DATA_OPTIN.equals(key)){
            		Log.d(TAG, String.format("Processing opt-in bundle data for %s : NOT IMPLEMENTED", view.getPlacementTag()));
			for(DataCollectionField field : data.<DataCollectionField>getParcelableArrayList(PlayHavenView.BUNDLE_DATA_OPTIN))
			{
				// TBD
			}
		}
	    }
	    }

            HashMap<String, Object> typeHash = new HashMap<String, Object>(1);
            typeHash.put("type", dismissType.toString());
            JSONObject typeData = new JSONObject(typeHash);
            
            map.clear();
            map.put("data", typeData);
            map.put("name", "dismiss");
            map.put("hash", id);
            
            responseData = new JSONObject(map);
            UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
        }
        
        @Override
        public void contentDismissed(Placement placement, PlayHavenView.DismissType dismissType, Bundle data)
        {
            Log.d(TAG, String.format("%s was dismissed: %s", placement.getPlacementTag(), dismissType));            
        }
        
        @Override
        public void contentFailed(Placement placement, PlayHavenException e)
        {
            Log.d(TAG, String.format("%s failed to display: %s", placement.getPlacementTag(), e.getMessage()));            
			notifyError(id, e.getMessage());
        }
        
        @Override
        public void contentLoaded(Placement placement)
        {
            Log.d(TAG, String.format("%s was loaded", placement.getPlacementTag()));
			this.placement = placement;
            stage = STAGE_SHOW;

			notifyWillDisplay(id);
			currentActivity.runOnUiThread(this);
			notifyDidDisplay(id);
        }

		private void notifyWillDisplay(int id)
		{
			HashMap<String, Object> data;
			JSONObject responseData;
	
			// willdisplay
			data = new HashMap<String, Object>(4);
			data.put("data", "");
			data.put("name", "willdisplay");
			data.put("hash", id);
			responseData = new JSONObject(data);
			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());							
		}
		
		private void notifyDidDisplay(int id)
		{
			HashMap<String, Object> data;
			JSONObject responseData;
	
			// diddisplay
			data = new HashMap<String, Object>(4);
			data.put("data", "");
			data.put("name", "diddisplay");
			data.put("hash", id);
			responseData = new JSONObject(data);
			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());					
		}

		private void notifyError(int id, String message)
		{
			HashMap<String, Object> error = new HashMap<String, Object>(2);
			error.put("code", 0);
			error.put("description", message);
			JSONObject errorData = new JSONObject(error);

			HashMap<String, Object> data = new HashMap<String, Object>(4);
			data.put("data", errorData);
			data.put("name", "error");
			data.put("hash", id);
			JSONObject responseData = new JSONObject(data);

			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());			
		}
	}
	
	private class PurchaseTrackingRequestRunner implements Runnable
	{
		private com.playhaven.android.data.Purchase purchase;
		private Context context;
		
		public void run(final Activity currentActivity, com.playhaven.android.data.Purchase purchase)
		{
			this.purchase = purchase;
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}		

		public void run()
		{
			(new PurchaseTrackingRequest(purchase)).send(context);
		}
	}
	
	private class PreloadRequestRunner implements Runnable, PlacementListener
	{
        private int id;
		private String placementTag;
		private Context context;
		
		public void run(final Activity currentActivity, int id, final String placementTag)
		{
            this.id = id;
			this.placementTag = placementTag;
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
    		Placement placement = new Placement(placementTag);
    		placement.setListener(this);
			placement.preload(context);
		}
        
        @Override
        public void contentDismissed(Placement placement, PlayHavenView.DismissType dismissType, Bundle data)
        {
            Log.d(TAG, String.format("%s was dismissed: %s", placement.getPlacementTag(), dismissType));            
        }
        
        @Override
        public void contentFailed(Placement placement, PlayHavenException e)
        {
            Log.d(TAG, String.format("%s failed to display: %s", placement.getPlacementTag(), e.getMessage()));            
			notifyError(id, e.getMessage());
        }
        
        @Override
        public void contentLoaded(Placement placement)
        {
            Log.d(TAG, String.format("%s was pre-loaded", placement.getPlacementTag()));
			preloadedPlacements.put(id, placement);

			HashMap<String, Object> data = new HashMap<String, Object>(1);
			data.put("name", "gotcontent");
			data.put("hash", id);
			JSONObject responseData = new JSONObject(data);
            
			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());
        }

		private void notifyError(int id, String message)
		{
			HashMap<String, Object> error = new HashMap<String, Object>(2);
			error.put("code", 0);
			error.put("description", message);
			JSONObject errorData = new JSONObject(error);

			HashMap<String, Object> data = new HashMap<String, Object>(4);
			data.put("data", errorData);
			data.put("name", "error");
			data.put("hash", id);
			JSONObject responseData = new JSONObject(data);

			UnityPlayer.UnitySendMessage("PlayHavenManager", "HandleNativeEvent", responseData.toString());			
		}
	} 

	private class GCMRegistrationRequestRunner implements Runnable
	{
		private Context context;
		
		public void run(final Activity currentActivity)
		{
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
			(new GCMRegistrationRequest()).register(context);
		}
	}

	private class GCMDeregistrationRequestRunner implements Runnable
	{
		private Context context;
		
		public void run(final Activity currentActivity)
		{
			context = (Context) currentActivity;
			currentActivity.runOnUiThread(this);
		}
		
		public void run()
		{
			(new GCMRegistrationRequest()).deregister(context);
		}
	}
}
