
/**
 * Author:    Gianluca Cau
 * Created:   01.03.2014
 * 
 * Released under MIT license
 **/

using UnityEngine;
using System.Collections;

public class BarrierInsect : Insect {

	//keep a reference of the ladybug
	private GameObject ladybug;

	// Use this for initialization
	void Start ()
	{
		ladybug = GameObject.FindGameObjectWithTag("Ladybug");

	}
	
	// Update is called once per frame
	void Update () 
	{
		//update the superclass
		base.Update();
		/*
		if(Input.GetKey(KeyCode.D))
		{
			ladybug.SendMessage("CreateBarrier");
			MusicPlayer.instance.PlayBarrierSound();
			base.Explode();
		}*/
	}

	//if we tap or click on this object we create a barrier around the player
	void OnMouseDown()
	{
		ladybug.SendMessage("CreateBarrier");
		MusicPlayer.instance.PlayBarrierSound();
		base.Explode();
	}
}
