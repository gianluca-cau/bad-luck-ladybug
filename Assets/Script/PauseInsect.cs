using UnityEngine;
using System.Collections;

public class PauseInsect : Insect {
	
	private GameObject ladybug;

	// Use  for initialization
	void Start () 
	{
		ladybug = GameObject.FindGameObjectWithTag("Ladybug");
	}
	
	// Update is called once per frame
	void Update () 
	{
		base.Update();

		if(Input.GetKey(KeyCode.D))
		{
			if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
				ladybug.GetComponent<Ladybug>().SendMessage("Paused",1f);
			base.Explode();
		}
	}

	void OnMouseDown()
	{
		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
			ladybug.GetComponent<Ladybug>().SendMessage("Paused",1f);
		base.Explode();
	}
	
}
