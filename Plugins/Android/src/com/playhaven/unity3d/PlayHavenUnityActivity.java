package com.playhaven.unity3d;

import com.unity3d.player.UnityPlayerActivity;
import android.content.*;

public class PlayHavenUnityActivity extends UnityPlayerActivity
{
	@Override
	protected void onNewIntent(Intent intent)
	{
		super.onNewIntent(intent);
		setIntent(intent);
	}
}