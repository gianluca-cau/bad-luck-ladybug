using UnityEngine;
using System.Collections;

public class Ladybug : MonoBehaviour {

	public float startSpeed;

	//the direction
	public Vector2 direction;
	//the time to stop before running to another flower
	public float stopTime;
	//public GameObject exclamationMarkObj;
	public GameObject heartObj;
	public GameObject barrierObj;
	public GameObject sadFaceObj;
	public GameObject speedIncreasedObj;
	public GameObject moreFlowersObj;

	public float exclamationMarkOffset;
	public float tookAFlowerWaitTime;
	public bool CanMove = true;
	public Vector2 startPosition;

	public AudioClip flowerPicked;
	private GameObject flower;
	//private GameObject exclamationMark;
	private GameObject instantiator;
	private GameObject heart;
	private GameObject sadFace;
	private GameObject speedIncreased;
	private GameObject moreFlowers;
	private int tmpFlowerNumber = -1;
	private bool lookinForAFlower = true;
	private float actualSpeed;
	private float timer;

	private float checkFlowerExistsTime = 1;
	private float checkFlowerExistsTimer;
	private GameObject barrierInstance;
	//movement speed
	private float speed;
	private Animator animator;

	// Use this for initialization
	void Start ()
	{
		//initialize start position
		startPosition = Vector2.zero;
		//set actual speed same as start speed
		actualSpeed = startSpeed;
		//initialize emoticon sprites and deactivate them,they will be activated when needed
		//exclamationMark = (GameObject)Instantiate(exclamationMarkObj,transform.position + new Vector3(0,exclamationMarkOffset,0),Quaternion.identity);
		//exclamationMark.SetActive(false);

		heart = (GameObject)Instantiate(heartObj,transform.position + new Vector3(0,exclamationMarkOffset,0),Quaternion.identity);
		heart.SetActive(false);

		sadFace = (GameObject)Instantiate(sadFaceObj,transform.position + new Vector3(0,exclamationMarkOffset,0),Quaternion.identity);
		sadFace.SetActive(false);

		speedIncreased = (GameObject)Instantiate(speedIncreasedObj,transform.position + new Vector3(0,exclamationMarkOffset,0),Quaternion.identity);
		speedIncreased.SetActive(false);

		moreFlowers = (GameObject)Instantiate(moreFlowersObj,transform.position + new Vector3(0,exclamationMarkOffset,0),Quaternion.identity);
		moreFlowers.SetActive(false);

		//reference of the instantiator
		instantiator = GameObject.Find("Instantiator");
		//start pos
		transform.position = startPosition;
		speed = startSpeed;
		animator = GetComponent<Animator>();


	}
	
	// Update is called once per frame
	void Update () 
	{
		//react depending on the actual game state
		switch(GameManager.instance.GetGameState())
		{
			case GameManager.GameState.PLAYING:
				//speed = startSpeed + Controller.GetLevel() * (startSpeed * 0.1f);
				if(CanMove)
					Move();
				break;
		}

		if(actualSpeed == 0 || !CanMove || GameManager.instance.GetGameState() != GameManager.GameState.PLAYING)
		{
			animator.speed = 0;
		}
		else
			animator.speed = 0.9f;

	}
	

	public void Move()
	{
		//if we are lookin for another flower...
		if(lookinForAFlower)
		{
			//get the new flower
			flower = LookForAFlower();

			//if we found a flower...
			if(flower != null)
			{
				//stop for a little time...
			//	if(timer < stopTime)
			//	{
				//	actualSpeed = 0;
					//exclamationMark.transform.position = transform.position + new Vector3(0,exclamationMarkOffset,0);
					//exclamationMark.SetActive(true);
				//	timer += Time.deltaTime;
				//}
			//	else
				{
					//exclamationMark.SetActive(false);
					actualSpeed = speed;
					tmpFlowerNumber = flower.GetComponent<Flower>().mId;
					flower.GetComponent<Flower>().IsTargeted = true;
					lookinForAFlower = false;
					float angle = Mathf.Atan2((this.transform.position - flower.transform.position).y,(this.transform.position - flower.transform.position).x) * Mathf.Rad2Deg;
					transform.rotation = Quaternion.Euler(new Vector3(0,0,angle + 90));
					timer = 0;
				}
			}
		}
		if(checkFlowerExistsTimer > checkFlowerExistsTime)
		{
			FlowerExist(tmpFlowerNumber);
			checkFlowerExistsTimer = 0;
		}
		this.transform.Translate(actualSpeed * Vector2.up);
		checkFlowerExistsTimer += Time.deltaTime;
	}

	 GameObject LookForAFlower()
	{
		float distance = 10000;
		GameObject targetFlower = null;

			foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Flowers"))
			{
				if(Vector2.Distance(this.transform.position,obj.transform.position) < distance)
				{
					distance = Vector2.Distance(this.transform.position,obj.transform.position);
					if(obj.GetComponent<Flower>().mId != tmpFlowerNumber && obj.GetComponent<Flower>().IsActive && obj != null)
						targetFlower = obj;
				}
			}
		return targetFlower;     
	}

	public void LookForAnotherFlower()
	{
		lookinForAFlower = true;
		flower = null;
	}

	public void FlowerExist(int flowerNumber)
	{
		//Debug.Log("Checking if flower exists...");
		bool flowerFound = false;
		foreach(GameObject flower in GameObject.FindGameObjectsWithTag("Flowers"))
		{
			if(flower.GetComponent<Flower>().mId == flowerNumber)
				flowerFound = true;
		}
		if(!flowerFound)
			LookForAnotherFlower();
	}

	IEnumerator OnTriggerEnter2D(Collider2D c)
	{
		if(c.tag == "Flowers")
		{
			MusicPlayer.instance.PlayFlowerTakenSound();
			GameObject flower = c.gameObject;
			if(flower.GetComponent<Flower>().IsTargeted)
			{

				Flower.mType t = flower.GetComponent<Flower>().GetType();
				flower.SendMessage("Explode");

				if(t == Flower.mType.NORMAL || t == Flower.mType.DUPLI)
				{
					GameManager.instance.HeartLost();
					heart.transform.position = transform.position + new Vector3(0,exclamationMarkOffset,0);
					heart.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
					StartCoroutine(Effects.MoveAndFade(heart,Vector2.up,0.01f,0.02f));
				}
				if(t == Flower.mType.TOXIC)
				{
					sadFace.transform.position = transform.position + new Vector3(0,exclamationMarkOffset,0);
					sadFace.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);

					StartCoroutine(Effects.MoveAndFade(sadFace,Vector2.up,0.01f,0.02f));
				}
				actualSpeed = 0;
				Debug.Log(name);
				yield return new WaitForSeconds(tookAFlowerWaitTime);

				if(t == Flower.mType.DUPLI)
					instantiator.GetComponent<Instantiator>().SpawnAFlower("dupli");
				actualSpeed = speed;
				LookForAnotherFlower();
			}
		}
	}

	IEnumerator Paused(float pauseTime)
	{
		CanMove = false;
		GetComponent<SpriteRenderer>().color = Color.gray;
		GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().color = Color.gray;
		GameObject guiController = GameObject.Find("GUI controller");

		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
		{
			guiController.GetComponent<GUIManager>().timerText = "3";
			MusicPlayer.instance.PlayTimerSound();
			yield return new WaitForSeconds(pauseTime);
			guiController.GetComponent<GUIManager>().timerText = "2";
			MusicPlayer.instance.PlayTimerSound();
			yield return new WaitForSeconds(pauseTime);
			guiController.GetComponent<GUIManager>().timerText = "1";
			MusicPlayer.instance.PlayTimerSound();
			yield return new WaitForSeconds(pauseTime);
			guiController.GetComponent<GUIManager>().timerText = "";
			MusicPlayer.instance.PlayTimerSound();
			GetComponent<SpriteRenderer>().color = Color.white;
		}
		GameObject.FindGameObjectWithTag("Background").GetComponent<SpriteRenderer>().color = Color.white;
		CanMove = true;
	}

	public void SetSpeed(float speed)
	{
		this.speed = speed;
	}

	public void SpeedIncreasedMsg()
	{
		speedIncreased.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
		speedIncreased.transform.position = transform.position + new Vector3(0,exclamationMarkOffset,0);
		StartCoroutine(Effects.MoveAndFade(speedIncreased,Vector2.up,0.01f,0.02f));
	}

	public void MoreFlowersMsg()
	{
		moreFlowers.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
		moreFlowers.transform.position = transform.position + new Vector3(0,exclamationMarkOffset,0);
		StartCoroutine(Effects.MoveAndFade(moreFlowers,Vector2.up,0.01f,0.02f));
	}

	public void CreateBarrier()
	{
		barrierInstance = (GameObject) Instantiate(barrierObj,transform.position,Quaternion.identity);
		barrierInstance.transform.parent = transform;
	}
}
