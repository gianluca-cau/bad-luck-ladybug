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

public class AdvanceTutorial : MonoBehaviour
{
	#region Attributes
	public TutorialController.TutorialPhase phaseToTrigger;
	#endregion
	
	#region Unity
	void OnTriggerEnter(Collider other)
	{
		if (!hasBeenTriggered && other.tag == "Player")
		{
			TutorialController.AdvanceTutorial(phaseToTrigger);
			hasBeenTriggered = true;
		}
	}
	#endregion

	#region Private
	private bool hasBeenTriggered;
	#endregion
}
