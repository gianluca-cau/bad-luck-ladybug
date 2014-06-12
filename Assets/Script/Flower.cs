using UnityEngine;
using System.Collections;

public class Flower : MonoBehaviour {

	//increase this everytime we create an instance
	public static int id;

	//assign the respective id to each instance
	public int mId;

	//rotation speed 
	public float rotationSpeed;

	//is the flower targeted by the ladybug?
	public bool IsTargeted{get;set;}

	//is the flower active?
	public bool IsActive{get;set;}
	//public int InstantSpawn = -1;

	//particles spawned when we hit a flower
	public GameObject explosionParticles;

	//the comic on the first flower (touch me)
	public GameObject comic;

	//types of flower,each one with different behavior
	public enum mType{DUPLI,NORMAL,TOXIC};

	//the first flower will spawn always at the same spot
	public Transform firstSpawnPosition;

	//the type of the flower
	public mType myType;

	//is the flower initialized?
	private bool initialized = false;

	//references
	private GameObject instantiator;
	private GameObject ladyBug;
	private GameObject comicInstance;
	private GameObject guiController;

	void Start()
	{
		IsActive = false;
		//increase this everytime we create another instance
		id++;

		//assign it to the instance
		mId = id;

		//get references
		instantiator = GameObject.Find("Instantiator");
		ladyBug = GameObject.FindGameObjectWithTag("Ladybug");
		guiController = GameObject.Find("GUI controller");

		//if this is the first flower we spawn
		if(mId == 1)
		{
			transform.position = firstSpawnPosition.position;

			//create the comic near the flower
			comicInstance = (GameObject)Instantiate(comic,transform.position + new Vector3(- GetComponent<CircleCollider2D>().radius * 1.4f,GetComponent<CircleCollider2D>().radius * 3.3f,0),Quaternion.identity);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		Initialize();

		//if the flower is targeted by the ladybug...
		if(IsTargeted)
			//...paint it red!
			GetComponent<SpriteRenderer>().color = Color.red;

		transform.Rotate(Vector3.forward * rotationSpeed);
	}

	void Initialize()
	{
		if(!initialized)
		{
			gameObject.tag = "Flowers";
			IsActive = true;
			initialized = true;
		}
	}

	void OnMouseDown()
	{
		//if we click on this flower and we are in the playing phase...
		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
		{
			//...if this flower is active
			if(IsActive)
			{
				MusicPlayer.instance.PlayPickedFlowerSound();

				//if the flower is dupli type tell the intantiator to spawn another one
				if(myType == mType.DUPLI)
					instantiator.SendMessage("SpawnAFlower","dupli");

				//if the flower is toxic...
				if(myType == mType.TOXIC)
				{
					//show the warning message
					guiController.GetComponent<GUIManager>().warningMsg = "Lives -1";
					guiController.GetComponent<GUIManager>().showWarningMsg();

					//one life is gone!
					GameManager.instance.LifeLost();
					MusicPlayer.instance.PlayPoisonedFlowerSound();
				}
				//if this flower is targeted by the ladybug...
				if(IsTargeted)
				{
					//...tell her to look for another one cuz this is already gone!
					ladyBug.SendMessage("LookForAnotherFlower");
				}
				//let this flower explode!
				Explode();
			}
		}
	}

	//return the type of this flower
	public mType GetType()
	{
		return myType;
	}

	public void Explode()
	{
		//we notice the game manager that we took a flower
		GameManager.instance.FlowerTaken();

		//explosion of "fancy" particles when we get the flower
		GameObject expl = (GameObject)Instantiate(explosionParticles,transform.position,Quaternion.identity);

		//destroy the particle game obj after 1 sec
		GameObject.Destroy(expl,1);

		//if this is the first flower also destroy the comic
		if(mId == 1)
			Destroy(comicInstance);

		//destroy this game obj
		GameObject.Destroy(this.gameObject);
	}
	
}
