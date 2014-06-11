using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour {

	public GUISkin mySkin;
	public float streakTime;
	public float warningMsgTime;
	public Texture2D flowerTexture;
	public Texture2D heartTexture;


	public string timerText;
	public string level;
	public string streakMsg = "";
	public string warningMsg;
	public string levelSwitchPlayerMsg;
	public string newHighScoreMsg = "New high score!";
	
	public Rect buttonsBeginArea;
	public Rect gameOverRect;
	public Rect timerTextRect;
	public Rect scoreRect;
	public Rect newScoreRect;
	public Rect streakRect;
	public Rect levelSwitchPlayerMsgRect;

	public GUIStyle startMessageStyle;
	public GUIStyle startMessageStyle2;
	public GUIStyle levelSwitchStyle;
	public GUIStyle newScoreStyle;
	public GUIStyle timerStyle;
	public GUIStyle levelSwitchPlayerMsgStyle;
	public bool drawHighScore;

	public Color pauseColor;
	private float nativeWidth = 800;
	private float nativeHeight = 480;
	private bool fadeCalled;

	private Rect levelSwitchRect;
	private float streakTimer;
	private float warningMsgTimer;
	private int matchsPlayed,
	matchsToTriggerAD = 2;
	void Start()
	{
		levelSwitchRect = new Rect(0,nativeHeight * 0.5f - levelSwitchStyle.fontSize * 0.5f,nativeWidth,60);
		timerStyle = levelSwitchStyle;
	}

	void OnGUI()
	{
		GUI.skin = mySkin;

		//GUI scale indipendent from resolution
		float rx = Screen.width / nativeWidth;
		float ry = Screen.height / nativeHeight;
		GUI.matrix =  Matrix4x4.TRS (new Vector3(0, 0, 0), Quaternion.identity, new Vector3 (rx, ry, 1));

		switch(GameManager.instance.GetGameState())
		{
		case GameManager.GameState.STARTING:
			drawHighScore = false;
			Effects.DrawOutline(new Rect(0,nativeHeight * 0.3f,nativeWidth,50),"Take the flowers!",startMessageStyle,Color.black,startMessageStyle.normal.textColor,5);
			if(GUI.Button(new Rect(325,240,150,60),"Start"))
			{
				GameManager.instance.StartGame();
			}
			//Effects.DrawOutline(new Rect(0,nativeHeight * 0.3f + 60,nativeWidth,30),"Tap to start",startMessageStyle2,Color.black,startMessageStyle2.normal.textColor,5);
			break;

		case GameManager.GameState.PLAYING:

			Effects.DrawOutline(timerTextRect,timerText,timerStyle,Color.black,Color.white,5);

			if(streakTimer < streakTime)
			{
				Effects.DrawOutline(streakRect,streakMsg,startMessageStyle2,Color.black,Color.cyan,5);
				streakTimer += Time.deltaTime;
			}

			if(warningMsgTimer < warningMsgTime)
			{
				Effects.DrawOutline(levelSwitchRect,warningMsg,levelSwitchStyle,Color.black,Color.red,4);
				warningMsgTimer += Time.deltaTime;
			}
			break;

		case GameManager.GameState.LEVEL_SWITCHING:
			warningMsg = "";
			level = "Level " + GameManager.instance.GetLevel();
			Effects.DrawOutline(levelSwitchRect,level,levelSwitchStyle,Color.black,Color.yellow,5);
			Effects.DrawOutline(levelSwitchPlayerMsgRect,levelSwitchPlayerMsg,startMessageStyle2,Color.black,Color.red,4);
			break;

		case GameManager.GameState.GAME_OVER:
			Effects.DrawOutline(gameOverRect,"Game Over",levelSwitchStyle,Color.black,Color.red,10);
			Effects.DrawOutline(scoreRect,"Your score is : " + GameManager.instance.GetFlowersTaken(),startMessageStyle2,Color.black,Color.yellow,3);

			if(drawHighScore)
				Effects.DrawOutline(newScoreRect,newHighScoreMsg,startMessageStyle2,Color.black,Color.red,4);

			GUILayout.BeginArea(buttonsBeginArea);

			if(GUILayout.Button("Play again"))
			{
				matchsPlayed++;
				if(matchsPlayed >= matchsToTriggerAD)
				{
					PlayHavenManager.Instance.ContentRequest("game_over");
					matchsPlayed = 0;
				}
				GameManager.instance.Restart();
			}

			if(GUILayout.Button("Menu"))
			{
				Application.LoadLevel("main_menu");
			}
			GUILayout.EndArea();
			break;
		case GameManager.GameState.PAUSED:
			Effects.DrawOutline(new Rect(0,0,800,180),"Paused!",startMessageStyle,Color.black,pauseColor,4);
			Effects.DrawOutline(new Rect(0,0,800,300),"Press resume to restart",startMessageStyle2,Color.black,Color.cyan,4);
			//GUI.Label(new Rect(0,0,800,480),"Paused\nPress resume to restart",startMessageStyle2);

			if(GUI.Button(new Rect(300,220,200,100),"Resume"))
			{
				GameManager.instance.ResumeGame();
			}

			Effects.DrawOutline(new Rect(0,100,800,560),"Press the back button to return to the main menu",startMessageStyle2,Color.black,Color.red,4);

			break;
		default:
			break;
		}

		GUI.BeginGroup(new Rect(10,10,nativeWidth,nativeHeight));
		GUI.DrawTexture(new Rect(0,0,50,50),flowerTexture);
		Effects.DrawOutline(new Rect(60,0,1000,40),GameManager.instance.GetFlowersTaken().ToString(),mySkin.label,Color.black,Color.white,3);
		Effects.DrawOutline(new Rect(150,0,1000,40),"Level " + GameManager.instance.GetLevel(),mySkin.label,Color.black,Color.magenta,3);
				GUI.BeginGroup(new Rect(300,5,500,40));
		Effects.DrawOutline(new Rect(0,0,100,40),"Lives ",mySkin.label,Color.black,Color.white,3);


		if(GameManager.instance.GetHeartsRemaining() < 9)
				for(int i = 0;i < GameManager.instance.GetHeartsRemaining();i++)
					GUI.DrawTexture(new Rect(80 + 45*i,0,40,40),heartTexture);
		else
		{
			GUI.DrawTexture(new Rect(80,0,40,40),heartTexture);
			Effects.DrawOutline(new Rect(130,0,100,40),GameManager.instance.GetHeartsRemaining().ToString(),mySkin.label,Color.black,Color.white,3);
		}
				GUI.EndGroup();
		GUI.EndGroup();
	}
	
	public void ShowStreak()
	{
		streakTimer = 0;
	}

	public void showWarningMsg()
	{
		warningMsgTimer = 0;
	}
}
