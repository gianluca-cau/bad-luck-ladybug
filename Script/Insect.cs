using UnityEngine;
using System.Collections;

public abstract class Insect : MonoBehaviour {

	public float speed = 1;
	public static int number;
	public GameObject explosion;
	private float rotation;

	// Use this for initialization
	void Start () 
	{
		number++;
	}
	
	// Update is called once per frame
	protected void Update () 
	{
		if(GameManager.instance.GetGameState() == GameManager.GameState.PLAYING)
			Move ();
	}

	protected void Move()
	{
		transform.Translate(Vector2.up * speed);
	}

	protected void Explode()
	{
		GameManager.instance.InsectKilled();
		GameObject expl = (GameObject)Instantiate(explosion,transform.position,Quaternion.identity);
		GameObject.Destroy(expl,1);
		GameObject.Destroy(this.gameObject);

	}

	public void SetSpeed(float speed)
	{
		this.speed = speed;
	}
}
