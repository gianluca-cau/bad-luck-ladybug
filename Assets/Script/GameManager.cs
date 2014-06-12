using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	//just some constants 
	public const string HIGH_SCORE = "high score";
	public const string LONGEST_STREAK = "longest streak";
	public const string TOTAL_INSECTS_KILLED = "total insects killed";
	public const string TOTAL_FLOWERS_TAKEN = "total flowers taken";

	//singleton instance
	private static GameManager _instance;

	//all the game states
	public enum GameState{STARTING,LEVEL_SWITCHING,PLAYING,GAME_OVER,PAUSED};

	//amount of flowers needed to end a level
	public int []flowersPerLevel;

	//time between the start of each level
	public float levelSwitchTime;

	//the duration of the streak message on the screen
	public float durationStreakMsg;

	//number of lives
	private int lives = 3;

	//at start the flower taken are 0
	private static int flowersTaken = 0;

	//level number
	private static int level = 1;

	//how many flowers we took without letting the ladybug take any
	private static int streak = 0;

	//the actual game state
	private static GameState gameState = GameState.STARTING;


	private bool heartTaken = false;
	private bool ladybugMsgCalled = true;
	private bool restarted = false;

	//timers
	private float timerStreak;
	private float levelSwitchTimer;

	private static bool statsSaved = false;

	//stats about our match
	private static int longestStreakOfTheMatch;
	private static int insectsKilledInThisMatch;

	//references
	private GameObject _ladybug;
	private GameObject _instantiator;
	private GameObject _guiManager;

	void Awake()
	{
		_instance = this;
	}

	void Start()
	{
		Reset();
	}

	//singleton
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
	//we just lost a life
	public void LifeLost()
	{
		lives--;
		//if our streak is the longest of the match store this value
		if(streak > longestStreakOfTheMatch)
			longestStreakOfTheMatch = streak;

		//reset the actual streak
		streak = 0;
	}

	public void LivesGained(int numberOfHearts)
	{
		lives += numberOfHearts;
	}

	//we took a flower!
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
		return lives;
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

	//reset
	public void Reset()
	{
		//destroy all the game entities in the scene
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Flowers"))
		{
			Destroy(obj);
		}

		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Insect"))
		{
			Destroy (obj);
		}

		//put the ladybug at the start pos
		ladybug.transform.position = GameObject.FindGameObjectWithTag("Ladybug").GetComponent<Ladybug>().startPosition;

		//spawn the dupli flower
		instantiator.GetComponent<Instantiator>().SpawnAFlower("dupli");

		//reset the level number
		level = 1;

		//the match is starting!
		gameState = GameState.STARTING;

		//reset counters and stuff
		lives = 3;
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
	{
		if(!heartTaken)
		{
			if(streak == 25)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 25!+1 Life";
				guiManager.GetComponent<GUIManager>().ShowStreak();
				LivesGained(1);
				heartTaken = true;
			}
			if(streak == 50)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 50!\n\nGreat!+3 Lives";
				LivesGained(3);
				guiManager.GetComponent<GUIManager>().ShowStreak();
				heartTaken = true;
			}
			if(streak == 100)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 100!\n\nFantastic!+6 Lives";
				LivesGained(6);
				guiManager.GetComponent<GUIManager>().ShowStreak();
				heartTaken = true;
			}
			if(streak == 200)
			{
				guiManager.GetComponent<GUIManager>().streakMsg = "Streak of 200!\n\nUnbelievable!!+15 Lives";
				LivesGained(15);
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

			if(lives <= 0)
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
				Reset();
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
