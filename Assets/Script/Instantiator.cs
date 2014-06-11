using UnityEngine;
using System.Collections;

public class Instantiator : MonoBehaviour {

	public Transform[] insectsSpawnPoints;

	public GameObject dupliFlower;
	public GameObject normalFlower;
	public GameObject toxicFlower;

	public GameObject BombInsect;
	public GameObject PauseInsect;
	public GameObject HeartInsect;
	public GameObject BarrierInsect;

	public float spawnRadius;
	public float minSpawnDistanceFromPlayer;
	public float offsetTopScreen;
	public float insectSpawnMaxTime;
	public float flowerSpawnTime;
	public float spawnTime;
	public float toxicSpawnTime;
	public float spawnOffsetFromScreenBound;

	public bool LevelChanged = false;

	private GameObject player;
	private float flowerSpawnTimer;
	private float insectSpawnTimer;
	private float maxRandInsectType;
	private float toxicSpawnTimer;

	private float flowersPerSpawn = 1;
	private bool extraFlowersSpawned;


	// Use this for initialization
	void Start () 
	{
		player = GameObject.FindGameObjectWithTag("Ladybug");
		SpawnAFlower("dupli");
		maxRandInsectType = 4;
	}



	// Update is called once per frame
	void Update () 
	{ 
		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
		{
			insectSpawnTimer += Time.deltaTime;
			flowerSpawnTimer += Time.deltaTime;
			toxicSpawnTimer += Time.deltaTime;

			if(insectSpawnTimer > insectSpawnMaxTime - Random.Range(0,insectSpawnMaxTime/2 * 0.3f) - GameManager.instance.GetLevel() * 0.5f)
			{
				//if theres another pause insect we can only spawn insects != pause one
					int index;
				if(GameObject.Find("Pause insect(Clone)") != null)
					index = (int)Random.Range(1,maxRandInsectType);
				else
					index = (int)Random.Range(0,maxRandInsectType);

				SpawnAnInsect(index);
				insectSpawnTimer = 0;
			}

			if(flowerSpawnTimer > flowerSpawnTime)
			{
				for(int j = 0;j < flowersPerSpawn;j++)
					SpawnAFlower("normal");
				flowerSpawnTimer = 0;
			}

			if(toxicSpawnTimer > toxicSpawnTime - Random.Range(0,toxicSpawnTime * 0.5f))
			{
				SpawnAFlower("toxic");
				toxicSpawnTimer = 0;
			}
		}
	}

	/*------------------------------------------------------
	 * Insect Spots
	 * 0-1 top
	 * 2 left
	 * 3 bottom
	 * 4-5 right
	 * ----------------------------------------------------*/
	//0 -> Pause 1 -> Bomb
	void SpawnAnInsect(int insectType)
	{

		int index = (int)Random.Range(0,insectsSpawnPoints.Length);


		Vector2 spawnPos = insectsSpawnPoints[index].transform.position;
		GameObject insect = new GameObject();
		float angle = 0;

		if(index < 2)//top
			angle = 180;

		if(index == 2)//left
			angle = 270;

		if(index == 3)//bottom
			angle = 0;

		if(index > 3)//right
			angle = 90;

		switch(insectType)
		{
		case 0:
			insect = (GameObject)Instantiate(PauseInsect,spawnPos,Quaternion.Euler(0,0,angle));
			break;
		case 1:
			insect = (GameObject)Instantiate(BombInsect,spawnPos,Quaternion.Euler(0,0,angle));
			break;
		case 2:
			insect = (GameObject)Instantiate(HeartInsect,spawnPos,Quaternion.Euler(0,0,angle));
			break;
		case 3:
			insect = (GameObject)Instantiate(BarrierInsect,spawnPos,Quaternion.Euler(0,0,angle));
			break;
		}

		insect.GetComponent<Insect>().SetSpeed(GameManager.instance.GetLevel() * 0.005f);

	}

	public void SetFlowersPerSpawn(int flowersPerSpawn)
	{
		this.flowersPerSpawn = flowersPerSpawn;
	}

	public void SpawnAFlower(string type)
	{
		Vector2 spawnPoint;	
		GameObject flowerObj;

		do
		{
			spawnPoint = new Vector2(Random.Range((- Camera.main.orthographicSize * Camera.main.aspect) + spawnOffsetFromScreenBound,
			                                      (Camera.main.orthographicSize * Camera.main.aspect) - spawnOffsetFromScreenBound),
			                         Random.Range((- Camera.main.orthographicSize) + spawnOffsetFromScreenBound,Camera.main.orthographicSize - offsetTopScreen));
		}while(Vector2.Distance(player.transform.position,spawnPoint) < minSpawnDistanceFromPlayer);

		switch(type)
		{
		case "normal":
			flowerObj = (GameObject)Instantiate(normalFlower,spawnPoint,Quaternion.identity);
			break;
		case "dupli":
			flowerObj = (GameObject)Instantiate(dupliFlower,spawnPoint,Quaternion.identity);
			break;
		case "toxic":
			flowerObj = (GameObject)Instantiate(toxicFlower,spawnPoint,Quaternion.identity);
			break;
		}				
	}

}
