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
		[SerializeField] private LayerMask _mask;

		private void Awake()
		{
			_user = GameObject.FindGameObjectWithTag("MainCamera").transform;	
		}

		void Update()
		{
			RaycastHit hit;
			if (Physics.Raycast(_user.position, _user.forward, out hit, Mathf.Infinity, _mask))
			{
				if(_currentCube != null && hit.collider.gameObject != _currentCube.gameObject)
				{
					_currentCube.HideInstruction();
					_currentCube = hit.collider.transform.parent.GetComponent<InstructionCube>();
					_currentCube.ShowInstruction();
				}
				else if(_currentCube == null)
				{
					_currentCube = hit.collider.transform.parent.GetComponent<InstructionCube>();
					_currentCube.ShowInstruction();
				}
			}
		}
	}
}