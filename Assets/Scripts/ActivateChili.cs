using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{


	public class ActivateChili : MonoBehaviour 
	{
		[SerializeField] private Rigidbody _chiliCorn;
		void OnDestroy()
		{
			_chiliCorn.isKinematic = false;
		}
	}
}