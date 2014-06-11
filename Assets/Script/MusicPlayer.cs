using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour {

	public AudioClip flowerPicked;
	public AudioClip timer;
	public AudioClip barrier;
	public AudioClip explosion;
	public AudioClip levelChange;
	public AudioClip gameOver;
	public AudioClip poisonedFlower;
	public AudioClip extraLife;
	public AudioClip barrierDestroyed;
	public AudioClip flowerTaken;

	private bool mute;
	private static MusicPlayer _instance;

	public static MusicPlayer instance
	{
		get
		{
			{
				_instance = GameObject.FindObjectOfType<MusicPlayer>();

				DontDestroyOnLoad(_instance.gameObject);
			}
			
			return _instance;
		}
	}

	void Awake()
	{
		if(_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(this);
		}
		else
		{
			if(this != _instance)
				Destroy(this.gameObject);
		}
	}

	public void PlayPickedFlowerSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(flowerPicked,gameObject.transform.position);
	}

	public void PlayTimerSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(timer,gameObject.transform.position);
	}

	public void PlayExplosionSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(explosion,gameObject.transform.position);
	}

	public void PlayBarrierSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(barrier,gameObject.transform.position);
	}

	public void PlayGameOverSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(gameOver,gameObject.transform.position);
	}

	public void PlayLevelChangeSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(levelChange,gameObject.transform.position);
	}

	public void PlayPoisonedFlowerSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(poisonedFlower,gameObject.transform.position);
	}

	public void PlayFlowerTakenSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(flowerTaken,gameObject.transform.position);
	}

	public void PlayExtraLifeSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(extraLife,gameObject.transform.position);
	}

	public void PlayBarrierDestroyedSound()
	{
		if(!mute)
			AudioSource.PlayClipAtPoint(barrierDestroyed,gameObject.transform.position);
	}

	public void TurnMusic()
	{
		if(GetComponent<AudioSource>().mute)
		{
			mute = false;
			GetComponent<AudioSource>().mute = false;
		}
		else
		{
			mute = true;
			GetComponent<AudioSource>().mute = true;
		}
		
	}
}
