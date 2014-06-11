#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using PlayHaven;

// COPYRIGHT(c) 2013, Medium Entertainment, Inc., a Delaware corporation, which operates a service
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

public class TutorialController : MonoBehaviour
{
	public enum TutorialPhase { Intermission, Start, Advertisement, RewardOne, RewardTwo, MoreGames, Done }
	
	#region Attributes
	public PlayHavenContentRequester advertisement;
	public PlayHavenContentRequester reward1;
	public PlayHavenContentRequester reward2;
	public PlayHavenContentRequester crossPromotion;
	#endregion
	
	#region Events
	public delegate void TutorialPhaseChange(bool isShowing);
	public static event TutorialPhaseChange OnPhaseChange;
	#endregion
		
	#region Unity
	void Awake()
	{
		instance = this;
	}
	
	void Start()
	{
		float width = Screen.width;
		float height = Screen.height;
		
		guiPanel = new Rect(width/8, height/8, width - (width/4), height - (height/4));
		
		if (OnPhaseChange != null) OnPhaseChange(IsShowingTutorial);	
		
		foundProblemWhileStarting = CheckForProblems();
	}
	
	void OnDisable()
	{
		if (OnPhaseChange != null) OnPhaseChange(false);
	}
	
	void OnGUI()
	{
		if (!IsShowingTutorial) return;
		GUI.depth = -20;
	
		switch(tutorialPhase)
		{
		case TutorialPhase.Start:
			GUI.Window((int)TutorialPhase.Start, guiPanel, TutorialStart, "Welcome to the PlayHaven Tutorial Level!");
			break;
		case TutorialPhase.Advertisement:
			GUI.Window((int)TutorialPhase.Advertisement, guiPanel, TutorialAdvertisement, "Advertisement Example");
			break;
		case TutorialPhase.RewardOne:
			GUI.Window((int)TutorialPhase.RewardOne, guiPanel, TutorialRewardOne, "Reward One Example");
			break;
		case TutorialPhase.RewardTwo:
			GUI.Window((int)TutorialPhase.RewardTwo, guiPanel, TutorialRewardTwo, "Reward Two Example");
			break;
		case TutorialPhase.MoreGames:
			GUI.Window((int)TutorialPhase.MoreGames, guiPanel, TutorialMoreGames, "More Games Example");
			break;
		}
	}

	#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		// Display error messages while in edit mode
		if (!Application.isPlaying && CheckForProblems())
		{
			const float VERTICAL_POSITION = 20;
			style.normal.textColor = Color.red;
			Handles.Label(SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(new Vector3(10, VERTICAL_POSITION * 2, 10)), problemWhileStartingMessage, style);
			Handles.Label(SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(new Vector3(10, VERTICAL_POSITION, 10)), "To get working fast, please read the quick-start guide in the 'Plugins/PlayHaven/Sample' folder!", style);
		}
	}
	#endif
	
	#endregion
	
	#region Actions
	public static void AdvanceTutorial(TutorialPhase phase)
	{	
		TutorialController tutorialController = TutorialController.instance;
		if (tutorialController != null)
			TutorialController.instance.PhaseChange(phase); 
	}	
	#endregion
	
	#region Private
	private TutorialPhase tutorialPhase = TutorialPhase.Start;
	private Rect guiPanel;
	private GUIStyle style = new GUIStyle();
	
	private static TutorialController instance;
	private string problemWhileStartingMessage;
	private static bool dontShowOnPlay;
	
	private Vector2 tutorialScrollPosition = Vector2.zero;
	private bool foundProblemWhileStarting;

	private bool IsShowingTutorial
	{ 
		get 
		{ 
			return tutorialPhase != TutorialPhase.Intermission && tutorialPhase != TutorialPhase.Done;
		}
	}
	
	private void PhaseChange(TutorialPhase phase)
	{
		if (enabled)
		{
			tutorialScrollPosition = Vector2.zero;
			tutorialPhase = phase;
			
			if (OnPhaseChange != null) OnPhaseChange(IsShowingTutorial);
		}
	}
	
	private void TutorialStart(int i)
	{
		if (foundProblemWhileStarting)
		{
			GUI.color = Color.red;
			GUILayout.Label(problemWhileStartingMessage);
			GUI.color = Color.white;
			GUILayout.Label("Please follow the quick-start guide in the Plugins/PlayHaven folder.");
		}
		else
		{
			tutorialScrollPosition = GUILayout.BeginScrollView(tutorialScrollPosition, style);
			{
				GUILayout.Label("This tutorial will guide you through what's going on here, and how you can implement" +
					" PlayHaven content in your own games. You can disable this tutorial at anytime by selecting the Tutorial" +
					" GameObject in the Hierachy and turning off either the GameObject or the TutorialController Component." +
					" It is also possible to disable while not playing to do away with it entirely, but if this is your first time" +
					" checking out PlayHaven then we recommend giving it a go; it's quite short.");
				
				GUILayout.Label("In this very simple scene we will demonstrate the PlayHaven plugin for Unity. There is one Advertisement," +
					" two Rewards, and One Cross Promotion Widget. You trigger them by passing through the Trigger Colliders. You can easily" +
					" find these Colliders in the inspector; they're all contained under the 'Example Placements' GameObjects.");
				
				GUILayout.Label("While these content units will work while on the device it is important to note that, while you" +
					" are playing in the editor, only a generic popup will appear.  When you use PlayHaven on a device, however, you" +
					" will see rich content being displayed whenever a content unit appears. Give it a try!");
				
				GUILayout.Label("The Horizontal Axis (default A/D and Left/Right Keys) moves left and right and the Jump key (default is" +
					" the Space Key) jumps. We've utilized the Input Manager for this, so if you have modified the Horizontal Axis and/or the" +
					" Jump Axis then you will be using your project specific settings for these Axes.");
				
				GUILayout.Label("Finally, there is documentation for the API available in the Plugins/PlayHaven folder. You should copy the zip file" +
					" somewhere other than your project before unzipping it. We've tried to keep this demo as small as possible so that it won't take" +
					" too long to download. If you feel that it's missing something then drop us a line at support@playhaven.com");
			}
			GUILayout.EndScrollView();
			if (GUILayout.Button("Got it!", GUILayout.Height(100)))
				PhaseChange(TutorialPhase.Intermission);
		}
	}
	
	private void TutorialAdvertisement(int i)
	{
		tutorialScrollPosition = GUILayout.BeginScrollView(tutorialScrollPosition, style);
		{
			GUILayout.Label("This placement demonstrates an interstitial advertisement.  Advertisements earn you money each time a player" +
				" sees/follows one. They are the easiest way to monitize your game!");	// sees or follows?
		}
		GUILayout.EndScrollView();
		if (GUILayout.Button("Got it!", GUILayout.Height(100)))
			PhaseChange(TutorialPhase.Intermission);
	}
	
	private void TutorialRewardOne(int i)
	{
		tutorialScrollPosition = GUILayout.BeginScrollView(tutorialScrollPosition, style);
		{
			GUILayout.Label("This placement demonstrates a reward. Rewards are easy ways to encourage your players to continue playing." +
				" You can provide any reward item you want, and because the data that changes is hosted on the PlayHaven server you can" +
				" change it anytime without having to build, test, release! The PlayHaven server will provide the player with whatever" +
				" you set.");
			
			GUILayout.Label("This example uses a static trigger to reward the player something when they jump through our little 'hoop'." +
				" You don't have to make it difficult for your players to obtain rewards, and could even reward them for making it to a certain" +
				" level, or for just starting the game!");
		}
		GUILayout.EndScrollView();
		if (GUILayout.Button("Got it!", GUILayout.Height(100)))
			PhaseChange(TutorialPhase.Intermission);
	}
	
	private void TutorialRewardTwo(int i)
	{
		tutorialScrollPosition = GUILayout.BeginScrollView(tutorialScrollPosition, style);
		{
			GUILayout.Label("This placement demonstrates another reward, except this time we've wrapped it up as a compound trigger." +
				" Triggering placements are very flexible with practically no limit to how they're used.");
			
			GUILayout.Label("It is worth noting that some developers prefer to use a single PlayHavenContentRequester for all of their requests " +
				" by setting the placement value to a valid placement before calling. If you decide to do this then you will be responsible" +
				" for counting uses and prefetching. This is because the PlayHavenContentRequester object would count it's own uses and" +
				" Prefetch whatever the initial placement value is.");
			
			GUILayout.Label("While it's possible to use a single PlayHavenContentRequester, we reccomend using a few different ones. You might" +
				" consider using a unique placement everywhere you're going to be doing prefetching and limiting uses, but have a shared item" +
				" that you will use for manual requests. At the end of the day, however, the system is flexible enough for you to use it as" +
				" you want, and not have to conform to any particular standard.");
		}
		GUILayout.EndScrollView();
		if (GUILayout.Button("Got it!", GUILayout.Height(100)))
		PhaseChange(TutorialPhase.Intermission);
	}
	
	private void TutorialMoreGames(int i)
	{
		tutorialScrollPosition = GUILayout.BeginScrollView(tutorialScrollPosition, style);
		{
			GUILayout.Label("This placement demonstrates the Cross Promotion Widget. The Cross Promotion Widget allows your players to find" +
				" other awesome games to install and play. You can promote your other titles here as well. You could, of course, use it anywhere" +
				" in your game that you feel like!");
			
			GUILayout.Label("");
		}
		GUILayout.EndScrollView();
		if (GUILayout.Button("Got it!", GUILayout.Height(100)))
			PhaseChange(TutorialPhase.Intermission);
	}
	
	// Check for things show-stoppers and generate an error message if we discover one.
	private bool CheckForProblems()
	{
		if (PlayHavenManager.Instance)
		{
			// Test to see if Token and Secret have been filled in. 
			if (string.IsNullOrEmpty(PlayHavenManager.Instance.token) || string.IsNullOrEmpty(PlayHavenManager.Instance.secret))
			{
				problemWhileStartingMessage = "We have detected that your token or secret has not been filled in!";
				
				#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					Selection.activeObject = PlayHavenManager.Instance.gameObject;
				
					// Due to a bug in Unity4 with PingObject, the following line has been disabled.
					//EditorGUIUtility.PingObject(Selection.activeObject);
				}
				#endif
			}
			
			// Check our ContentRequesters to see if the placement names have been set
			else if (string.IsNullOrEmpty(advertisement.placement) || string.IsNullOrEmpty(reward1.placement) || string.IsNullOrEmpty(reward2.placement) || string.IsNullOrEmpty(crossPromotion.placement))
			{
				problemWhileStartingMessage = "We have detected that not all of your placements have been assigned on the PlayHavenContentRequester Componenets!";
				
				#if UNITY_EDITOR
				if (Application.isPlaying)
					Selection.objects = new GameObject[] { advertisement.gameObject, reward1.gameObject, reward2.gameObject, crossPromotion.gameObject };
				#endif
			}
			
			// If there are no errors, clear the error message
			else
				problemWhileStartingMessage = string.Empty;
		}
		
		return !string.IsNullOrEmpty(problemWhileStartingMessage);
	}
	#endregion
}