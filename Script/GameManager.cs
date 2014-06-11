using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public const string HIGH_SCORE = "high score";
	public const string LONGEST_STREAK = "longest streak";
	public const string TOTAL_INSECTS_KILLED = "total insects killed";
	public const string TOTAL_FLOWERS_TAKEN = "total flowers taken";

	public enum GameState{STARTING,LEVEL_SWITCHING,PLAYING,GAME_OVER,PAUSED};
	public int []flowersPerLevel;
	public float levelSwitchTime;

	public float durationStreakMsg;

	private static GameManager _instance;
	private int hearts = 3;
	private static int flowersTaken = 0;
	private static int level = 1;
	private static int streak = 0;

	private static GameState gameState = GameState.STARTING;
	private float levelSwitchTimer;
	private bool heartTaken = false;
	private bool ladybugMsgCalled = true;
	private bool restarted = false;
	private float timerStreak;
	private static bool statsSaved = false;
	private static int longestStreakOfTheMatch;
	private static int insectsKilledInThisMatch;
	private GameObject _ladybug;
	private GameObject _instantiator;
	private GameObject _guiManager;

	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
		Restart();
	}

	public static GameManager instance
	{
		get
		{
			if(_instance == null)
				_instance = GameObject.FindObjectOfType<GameManager>();
			return _instance;
		}
	}

	public GameObject ladybug
	{
		get
		{
			if(_ladybug == null)
				_ladybug = GameObject.FindGameObjectWithTag("Ladybug");
			return _ladybug;
		}
	}

	public GameObject guiManager
	{
		get
		{
			if(_guiManager == null)
				_guiManager = GameObject.Find("GUI controller");
			return _guiManager;
		}
	}

	public GameObject instantiator
	{
		get
		{
			if(_instantiator == null)
				_instantiator = GameObject.Find("Instantiator");
			return _instantiator;
		}
	}

	public void HeartLost()
	{
		hearts--;
		if(streak > longestStreakOfTheMatch)
			longestStreakOfTheMatch = streak;
		streak = 0;
	}

	public void HeartsGained(int numberOfHearts)
	{
		hearts += numberOfHearts;
	}
	
	public void FlowerTaken()
	{
		flowersTaken++;
		streak++;
	}

	public int GetFlowersTaken()
	{
		return flowersTaken;
	}

	public  int GetHeartsRemaining()
	{
		return hearts;
	}

	public void NextLevel()
	{
		level++;
	}

	public int GetLevel()
	{
		return level;
	}

	public GameState GetGameState()
	{
		return gameState;
	}

	public void InsectKilled()
	{
		insectsKilledInThisMatch++;
	}

	public void Restart()
	{
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Flowers"))
		{
			Destroy(obj);
		}

		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Insect"))
		{
			Destroy (obj);
		}

		ladybug.transform.position = GameObject.FindGameObjectWithTag("Ladybug").GetComponent<Ladybug>().startPosition;
		instantiator.GetComponent<Instantiator>().SpawnAFlower("dupli");
		level = 1;
		gameState = GameState.STARTING;
		hearts = 3;
		longestStreakOfTheMatch = 0;
		insectsKilledInThisMatch = 0;
		flowersTaken = 0;
		streak = 0;
		statsSaved = false;
		instantiator.SendMessage("SetFlowersPerSpawn",1);
		ladybug.SendMessage("SetSpeed",0.04f);
	}

	public void StartGame()
	{
		if(level == 1)
		{
			gameState = GameState.LEVEL_SWITCHING;
		}
	}

	public void ResumeGame()
	{
		if(gameState == GameState.PAUSED)
			gameState = GameState.PLAYING;
	}

	void Update()
	{/*
		if(Input.GetKey(KeyCode.A))
		{
			GameObject[] objs =  GameObject.FindWithTag("Flowers");
			for(int i = 0;i < objs.Length;i++)
			{
				if(objs[i].GetComponent<Flower>().IsTargeted)
					objs[i].GetComponent<Flower>().SendMessage("
			}
		}*/
		if(!heartTaken)
		{
			if(streak == 25)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 25!+1 Life";
				guiManager.GetComponent<GUIManager>().ShowStreak();
				HeartsGained(1);
				heartTaken = true;
			}
			if(streak == 50)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 50!\n\nGreat!+3 Lives";
				HeartsGained(3);
				guiManager.GetComponent<GUIManager>().ShowStreak();
				heartTaken = true;
			}
			if(streak == 100)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 100!\n\nFantastic!+6 Lives";
				HeartsGained(6);
				guiManager.GetComponent<GUIManager>().ShowStreak();
				heartTaken = true;
			}
			if(streak == 200)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 200!\n\nUnbelievable!!+15 Lives";
				HeartsGained(15);
				guiManager.GetComponent<GUIManager>().ShowStreak();
				heartTaken = true;
			}
		}

		if(streak% 25 != 0 && streak != 0)
			heartTaken = false;

		switch(gameState)
		{
		case GameState.STARTING:
		
			break;

		case GameState.PLAYING:

			restarted = false;

			if(Input.GetKeyDown(KeyCode.Escape))
			{
				gameState = GameState.PAUSED;
			}

			if(hearts <= 0)
			{
				gameState = GameState.GAME_OVER;
				MusicPlayer.instance.PlayGameOverSound();
			}

			if(!ladybugMsgCalled)
			{
				switch(level)
				{
				case 2:
					ladybug.SendMessage("SetSpeed",0.05f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					//GameObject.Find("GUI controller").GetComponent<GUIController>().levelSwitchPlayerMsg = "Speed increased!!";
					break;
					
				case 3:
					ladybug.SendMessage("SetSpeed",0.053f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;
					
				case 4:
					instantiator.SendMessage("SetFlowersPerSpawn",2);
					ladybug.SendMessage("MoreFlowersMsg");
					break;

				case 5:
					ladybug.SendMessage("SetSpeed",0.055f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 6:
					instantiator.SendMessage("SetFlowersPerSpawn",3);
					ladybug.SendMessage("MoreFlowersMsg");
					break;

				case 7:
					ladybug.SendMessage("SetSpeed",0.057f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;
				case 8:
					ladybug.SendMessage("SetSpeed",0.059f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;
				case 9:
					instantiator.SendMessage("SetFlowersPerSpawn",4);
					ladybug.SendMessage("MoreFlowersMsg");
					break;

				case 10:
					ladybug.SendMessage("SetSpeed",0.061f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 11:
					instantiator.SendMessage("SetFlowersPerSpawn",5);
					ladybug.SendMessage("MoreFlowersMsg");
					break;

				case 12:
					ladybug.SendMessage("SetSpeed",0.062f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 13:
					ladybug.SendMessage("SetSpeed",0.063f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 14:
					ladybug.SendMessage("SetSpeed",0.064f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 15:
					ladybug.SendMessage("SetSpeed",0.065f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 16:
					ladybug.SendMessage("SetSpeed",0.066f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 17:
					ladybug.SendMessage("SetSpeed",0.067f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 18:
					ladybug.SendMessage("SetSpeed",0.068f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 19:
					ladybug.SendMessage("SetSpeed",0.069f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				case 20:
					ladybug.SendMessage("SetSpeed",0.07f);
					ladybug.SendMessage("SpeedIncreasedMsg");
					break;

				}
				ladybugMsgCalled = true;
			}
			if(flowersTaken >= flowersPerLevel[level])
			{
				level++;
				MusicPlayer.instance.PlayLevelChangeSound();
				instantiator.GetComponent<Instantiator>().LevelChanged = true;
				foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Flowers"))
				{
					obj.SendMessage("Explode");
				}
				instantiator.GetComponent<Instantiator>().SpawnAFlower("dupli");

				gameState = GameState.LEVEL_SWITCHING;
			}
			break;

		case GameState.PAUSED:
			if(Input.GetKeyDown(KeyCode.Escape))
			{
				Application.LoadLevel("main_menu");
				Restart();
			}
			break;
		case GameState.LEVEL_SWITCHING:

			ladybugMsgCalled = false;


			if(levelSwitchTimer > levelSwitchTime)
			{
				gameState = GameState.PLAYING;
				levelSwitchTimer = 0;
			}

			levelSwitchTimer += Time.deltaTime;
			break;

		case GameState.GAME_OVER:
			if(!statsSaved)
			{
				if(flowersTaken > PlayerPrefs.GetInt(HIGH_SCORE))
				{
					PlayerPrefs.SetInt(HIGH_SCORE,flowersTaken);
					guiManager.GetComponent<GUIManager>().drawHighScore = true;
				}

				if(PlayerPrefs.GetInt(LONGEST_STREAK) < longestStreakOfTheMatch)
					PlayerPrefs.SetInt(LONGEST_STREAK,longestStreakOfTheMatch);

				PlayerPrefs.SetInt(TOTAL_INSECTS_KILLED,PlayerPrefs.GetInt(TOTAL_INSECTS_KILLED) + insectsKilledInThisMatch);
				PlayerPrefs.SetInt(TOTAL_FLOWERS_TAKEN,PlayerPrefs.GetInt(TOTAL_FLOWERS_TAKEN) + flowersTaken);
				PlayerPrefs.Save();
				statsSaved = true;
			}
			break;
		}
	}
}
