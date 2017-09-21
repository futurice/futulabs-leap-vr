using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Futulabs
{
	public class InstructionsManager : MonoBehaviour 
	{
		private Transform _user;
		private InstructionCube _currentCube;

		private void Awake()
		{
			_user = GameObject.FindGameObjectWithTag("MainCamera").transform;	
		}

		void Update()
		{
			RaycastHit hit;
			if (Physics.Raycast(_user.position, _user.forward, out hit))
			{
				if(hit.collider.gameObject.tag == "InstructionCube")
				{
					if(_currentCube != null && hit.collider.gameObject != _currentCube.gameObject)
					{
						_currentCube.HideInstruction();
						_currentCube = hit.collider.gameObject.GetComponent<InstructionCube>();
						_currentCube.ShowInstruction();
					}
					else if(_currentCube == null)
					{
						_currentCube = hit.collider.gameObject.GetComponent<InstructionCube>();
						_currentCube.ShowInstruction();
					}
				}
			}
		}
	}
}