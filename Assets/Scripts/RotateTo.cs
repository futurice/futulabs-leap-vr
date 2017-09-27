using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{
	public class RotateTo : MonoBehaviour 
	{
		[SerializeField] private Transform _target;
		// Update is called once per frame
		void Update () 
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, Time.deltaTime*4f);
			transform.position = _target.position;
		}
	}
}