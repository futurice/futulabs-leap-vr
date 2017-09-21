using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

namespace Futulabs
{
	public class InstructionsManager : MonoBehaviour 
	{
		[SerializeField] private List<InstructionCube> _instructionCubes = new List<InstructionCube>();
		private int _currentCubeIndex = 0;
		private float _timeToShow = 5f;
		private bool _startTimer = true;
		private int _lastCubeIndex = -1;
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
			// timer based:
			/* 
			if(_startTimer)
			{
				_startTimer = false;
				if(_lastCubeIndex != -1) _instructionCubes[_lastCubeIndex].HideInstruction();
				_instructionCubes[_currentCubeIndex].ShowInstruction();
				_lastCubeIndex = _currentCubeIndex;
				_currentCubeIndex = Mathf.RoundToInt(Mathf.Repeat(_currentCubeIndex+1,_instructionCubes.Count));
				Debug.Log(_currentCubeIndex);
				Observable.Timer(TimeSpan.FromSeconds(_timeToShow)).Subscribe(_ =>
				{
					_startTimer = true;
				});
			}*/
		}
	}
}