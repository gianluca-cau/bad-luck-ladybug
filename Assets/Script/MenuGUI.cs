using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;

public class MenuGUI : MonoBehaviour {

	public GUISkin mySkin;
	public GUIStyle playButtonStyle;
	public GUIStyle audioButtonStyle;

	public Rect beginAreaRect;
	public Rect bestScoreWindow;
	public Rect bestScoreContent;
	public Rect howToPlayRect;
	public Rect playButtonRect;
	public Rect audioButtonRect;

	public Texture2D audioBtnOnTex,
	audioBtnOffTex;

	public Vector2 itemsTextureSize;
	public float itemsOffsetX,
	itemsOffsetY;

	public Item[] items;

	private float nativeWidth = 800;
	private float nativeHeight = 480;
	private string clicked = "";
	private Texture2D audioBtnTex;

	private BannerView bannerView;

	void Start()
	{
		audioBtnTex = audioBtnOnTex;
		// Create a 320x50 banner at the top of the screen.
		bannerView = new BannerView(
			"", AdSize.Banner, AdPosition.Bottom);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);
	}

	void OnGUI()
	{
		GUI.skin = mySkin;
		//GUI scale indipendent from resolution
		float rx = Screen.width / nativeWidth;
		float ry = Screen.height / nativeHeight;
		GUI.matrix =  Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, new Vector3 (rx, ry, 1));



		switch(clicked)
		{
		case "":
			if(GUI.Button(audioButtonRect,audioBtnTex,audioButtonStyle))
			{
				audioBtnTex = (audioBtnTex == audioBtnOnTex) ? audioBtnOffTex : audioBtnOnTex;
				MusicPlayer.instance.TurnMusic();
			}
			GUILayout.BeginArea(beginAreaRect);
			GUILayout.BeginHorizontal();
			
			if(GUILayout.Button("Stats"))
			{
				clicked = "high score";
			}
			if(GUILayout.Button("Start"))
			{
				clicked = "how to play";
				//Application.LoadLevel("main_scene");
			}
			if(GUILayout.Button("Quit"))
			{
				Application.Quit();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			break;
		case "high score":
			GUI.Window(0,bestScoreWindow,HighScoreAndStats,"");
			break;
		case "how to play":
			GUI.Window(1,howToPlayRect,HowToPlay,"");
			break;
		}

	}

	void HighScoreAndStats(int id)
	{
		Effects.DrawOutline(new Rect(40,40,500,40),"Best score: " + PlayerPrefs.GetInt(GameManager.HIGH_SCORE),GUI.skin.label,Color.black,Color.white,5);
		Effects.DrawOutline(new Rect(40,100,500,40),"Longest streak: " + PlayerPrefs.GetInt(GameManager.LONGEST_STREAK),GUI.skin.label,Color.black,Color.white,5);
		Effects.DrawOutline(new Rect(40,160,500,40),"Total flowers taken: " + PlayerPrefs.GetInt(GameManager.TOTAL_FLOWERS_TAKEN),GUI.skin.label,Color.black,Color.white,5);
		Effects.DrawOutline(new Rect(40,220,500,40),"Total insects killed: " + PlayerPrefs.GetInt(GameManager.TOTAL_INSECTS_KILLED),GUI.skin.label,Color.black,Color.white,5);

		if(GUI.Button(new Rect(240,320,800,85),"Back"))
		{
			clicked = "";
		}
	}

	void HowToPlay(int id)
	{
		for(int i = 0; i < items.Length;i++)
		{
			GUI.DrawTexture(new Rect(40,25 + itemsOffsetY *i,itemsTextureSize.x,itemsTextureSize.y),items[i].sprite.texture);
			Effects.DrawOutline(new Rect(itemsTextureSize.x + itemsOffsetX,25 + itemsOffsetY*i,800,480),items[i].description,GUI.skin.label,Color.black,Color.white,3);
		}

		if(GUI.Button(playButtonRect,"Play",playButtonStyle))
		{
			bannerView.Destroy();
			Application.LoadLevel("main_scene");
		}
	}
}

[System.Serializable]
public class Item
{
	public string name;
	public Sprite sprite;
	public string description;
}
