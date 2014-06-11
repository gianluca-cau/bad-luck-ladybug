using UnityEngine;
using System.Collections;

public class ManagePlugin : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		DontDestroyOnLoad(gameObject);
	}

#if UNITY_ANDROID
	void OnApplicationPause(bool pause)
	{
		if(!pause)
		{
			PlayHavenManager.Instance.OpenNotification();
		}
	}
#endif 

	// Update is called once per frame
	void Update () {
	
	}
}
