/**
 * Author:    Gianluca Cau
 * Created:   01.03.2014
 * 
 * Released under MIT license
 **/

using UnityEngine;
using System.Collections;

public class Barrier : MonoBehaviour {

	//keep a reference of the flower instantiator
	private GameObject instantiator;

	void Start()
	{
		instantiator = GameObject.Find("Instantiator");
	}


	void Update () 
	{
		//let the barrier rotate 
		transform.Rotate(Vector3.forward);
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		//if we hit any kind of flower...
		if(col.tag.Equals("Flowers"))
		{
			//if the barrier collides with a normal flower we lose the barrier and the flower is destroyed
			if(col.GetComponent<Flower>().myType == Flower.mType.NORMAL)
			{
				col.SendMessage("Explode");
				Destroy(gameObject);
			}
			//if we hit a flower of the dupli type we first destroy the flower then we tell the instantiator to instantiate another one of the same type
			if(col.GetComponent<Flower>().myType == Flower.mType.DUPLI)
			{
				col.SendMessage("Explode");
				instantiator.GetComponent<Instantiator>().SpawnAFlower("dupli");
				Destroy(gameObject);
			}
			//if we hit a toxic flower we destroy just flower and we let the barrier remain intact
			if(col.GetComponent<Flower>().myType == Flower.mType.TOXIC)
			{
				col.SendMessage("Explode");
			}
			//Play a sound when the barrier breaks
			MusicPlayer.instance.PlayBarrierDestroyedSound();
		}
	}
}
