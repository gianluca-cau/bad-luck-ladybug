using UnityEngine;
using System.Collections;

public class HeartInsect : Insect {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		base.Update();
	}

	void OnMouseDown()
	{
		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
		{
			GameManager.instance.LivesGained(1);
			MusicPlayer.instance.PlayExtraLifeSound();
			base.Explode();
		}
	}
}
