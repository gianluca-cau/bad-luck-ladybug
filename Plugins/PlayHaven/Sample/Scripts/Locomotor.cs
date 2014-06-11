using UnityEngine;
using System.Collections;

// COPYRIGHT(c) 2013, Medium Entertainment, Inc., a Delaware corporation, which operates a service
// called PlayHaven., All Rights Reserved
//  
// NOTICE:  All information contained herein is, and remains the property of Medium Entertainment, Inc.
// and its suppliers, if any.  The intellectual and technical concepts contained herein are 
// proprietary to Medium Entertainment, Inc. and its suppliers and may be covered by U.S. and Foreign
// Patents, patents in process, and are protected by trade secret or copyright law. Dissemination of this 
// information or reproduction of this material is strictly forbidden unless prior written permission 
// is obtained from Medium Entertainment, Inc.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// 
// Contact: support@playhaven.com

public class Locomotor : MonoBehaviour
{
	#region Attributes
	public Transform spawnPoint;
	#endregion
	
	#region Unity
	void Awake()
	{
		TutorialController.OnPhaseChange += HandleTutorialControllerOnPhaseChange;
		rigidbody.freezeRotation = isGrounded = true;
		Respawn();
		UpdateCamera();
	}
	
	void OnDestroy()
	{
		TutorialController.OnPhaseChange -= HandleTutorialControllerOnPhaseChange;
	}

	void Update()
	{
		#if UNITY_EDITOR
		// KEYBOARD CONTROL
		horizontal = Input.GetAxis("Horizontal") * PLAYER_SPEED;
		if (isGrounded && Input.GetButtonDown("Jump"))
			Jump();
		#endif
		// TOUCHSCREEN CONTROL
		if (Input.touchCount > 0)
		{
			touch = Input.GetTouch(0);
			if (touchDownX == 0)
				touchDownX = touch.position.x;
			horizontal = Mathf.Sign(touch.position.x - touchDownX) * PLAYER_SPEED;
			if (isGrounded && Input.touchCount == 2)
				Jump();
		}
		else
			touchDownX = 0;
			
		UpdateCamera();
	}
	
	void FixedUpdate()
	{
		rigidbody.MovePosition(rigidbody.position + (Vector3.forward * horizontal));
		horizontal = 0;
	}

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			if (contact.otherCollider.name == "Platform")
			{
				isGrounded = true;
				return;
			}
			
			if (contact.otherCollider.name == "RespawnCollider")
			{
				Respawn();
				return;
			}
		}
	}
	#endregion
	
	#region Handlers
	void HandleTutorialControllerOnPhaseChange (bool isShowing)
	{
		enabled = !isShowing;
		rigidbody.isKinematic = isShowing;
	}
	#endregion

	#region Private
	private bool isGrounded;
	private Touch touch;
	private float touchDownX = 0;
	private float horizontal = 0;
	
	private const float CAMERA_DISTANCE_FROM_PLAYER = 8;
	private const float CAMERA_Y_POSITION = 5;
	private const float PLAYER_SPEED = 0.2f;

	private void Jump()
	{
		rigidbody.AddForce(Vector3.up * 8, ForceMode.Impulse);
		isGrounded = false;
	}
	
	private void UpdateCamera()
	{
		Camera.main.transform.position = new Vector3(transform.position.x + CAMERA_DISTANCE_FROM_PLAYER, CAMERA_Y_POSITION, transform.position.z);
		Camera.main.transform.LookAt(transform);
	}
	
	private void Respawn()
	{
		transform.position = spawnPoint.position;
	}
	#endregion	
}
