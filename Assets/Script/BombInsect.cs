/**
 * Author:    Gianluca Cau
 * Created:   01.03.2014
 * 
 * Released under MIT license
 **/

using UnityEngine;
using System.Collections;

public class BombInsect : Insect {

	//keep a reference of the instantiator
	private GameObject instantiator;

	// Use this for initialization
	void Start () 
	{
		instantiator = GameObject.Find("Instantiator");
	}
	
	// Update is called once per frame
	void Update () 
	{
		//update the superclass
		base.Update();
	}

	//if we are in the playing phase and we click on this object let it explode!
	void OnMouseDown()
	{
		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
			Explosion();
	}

	void Explosion()
	{
		//get the amount of the flowers in the scene when the explosion occurs...
		int flowersExploded = GameObject.FindGameObjectsWithTag("Flowers").Length;
		MusicPlayer.instance.PlayExplosionSound();

		//...then iterate trough all of them...
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Flowers"))
		{
			//...and destroy each one
			obj.GetComponent<Flower>().Explode();
		}
		//then spawn a dupli flower since there is only one of them at any moment and the old one has just being destroyed
		instantiator.GetComponent<Instantiator>().SpawnAFlower("dupli");

		//let this insect disappear
		base.Explode();

	}
}
