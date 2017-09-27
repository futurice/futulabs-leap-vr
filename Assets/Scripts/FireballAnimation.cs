using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
namespace Futulabs
{
	public class FireballAnimation : MonoBehaviour 
	{
		[SerializeField] private Animator _animator;
		[SerializeField] private ParticleSystem _particleSystem;

		void Update()
		{
			var speed = _animator.GetFloat("speed");
			if(speed < 1 && _particleSystem.gameObject.activeInHierarchy)
			{
				_particleSystem.gameObject.SetActive(false);
			}
			else if(speed == 1 && !_particleSystem.gameObject.activeInHierarchy)
			{
				_particleSystem.gameObject.SetActive(false);
			}
		}
	}
}