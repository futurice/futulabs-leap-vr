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
		[SerializeField] private AudioSource _audio;

		private void Awake()
		{
			_user = GameObject.FindGameObjectWithTag("MainCamera").transform;	
		}

		void ActivateCube(RaycastHit hit)
		{
			_currentCube = hit.collider.transform.parent.GetComponent<InstructionCube>();
			_currentCube.ShowInstruction();
			_audio.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.INSTRUCTION_ACTIVATE));
		}

		void Update()
		{
			RaycastHit hit;
			if (Physics.Raycast(_user.position, _user.forward, out hit, Mathf.Infinity, _mask))
			{
				if(_currentCube != null && hit.collider.gameObject != _currentCube._instructionBox)
				{
					_currentCube.HideInstruction();
					ActivateCube(hit);
				}
				else if(_currentCube == null)
				{
					_currentCube = hit.collider.transform.parent.GetComponent<InstructionCube>();
					ActivateCube(hit);
				}
			}
		}
	}
}