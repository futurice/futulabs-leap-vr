using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs 
{
	public class DummyToggler : MonoBehaviour 
	{
		[SerializeField]
		private KeyCode _toggleDummyKey = KeyCode.Return;
		[SerializeField]
		private GameObject _dummy;
		[SerializeField]
		private SkinnedMeshRenderer _leftHand; //Hands being enabled once dummy is disabled
		[SerializeField]
		private SkinnedMeshRenderer _rightHand;

		void ToggleDummy()
		{
			_dummy.SetActive(!_dummy.activeInHierarchy);
			bool isDummyActive = _dummy.activeInHierarchy;
			_leftHand.enabled = !isDummyActive;
			_rightHand.enabled = !isDummyActive;
			_leftHand.transform.parent.gameObject.SetActive(!isDummyActive);
			_rightHand.transform.parent.gameObject.SetActive(!isDummyActive);
		}
		
		// Update is called once per frame
		void Update () 
		{
			if(Input.GetKeyDown(_toggleDummyKey))
			{
				ToggleDummy();
			}	
		}
	}
}