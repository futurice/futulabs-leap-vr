using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

namespace Futulabs
{
	public class Fireball : MonoBehaviour 
	{
		[SerializeField] private Rigidbody _rigidBody;
		[SerializeField] private Collider _collider;
		[SerializeField] private Vector3 _targetScale;
		[SerializeField] private Transform _glowingBall;
		[SerializeField] private GameObject _initialGlow;
		[SerializeField] //bitch

		private bool _used = false;
		
		void Awake()
		{
			_glowingBall.localScale = Vector3.zero;
			_glowingBall.DOScale(_targetScale, 0.15f);
		}

		public void Throw(Vector3 direction, float velocityMag)
		{
			transform.SetParent(null);
			var velocity = direction * velocityMag;
			_rigidBody.isKinematic = false;
			_rigidBody.velocity = velocity;
			_collider.enabled = true;
			Observable.Timer(TimeSpan.FromSeconds(3f)).TakeUntilDestroy(gameObject).Subscribe(_ =>
			{
				_glowingBall.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
				{
					Destroy(gameObject);
				});
			});
		}

		public void Kill()
		{
			Destroy(gameObject);
		}

		bool IsDestroyable(Collision c)
		{
			if(c.gameObject.tag == "WallCube")
			{
				return true;
			}
			if(c.gameObject.tag == "InteractableObject")
			{
				var cube = c.transform.parent.GetComponent<InteractableCubeController>();
				if(cube != null)
				{
					return true;
				}
			}
			return false;
		}

		void OnCollisionEnter(Collision collision)
		{
			if(IsDestroyable(collision) && !_used)
			{
				_used = true;
				var destroyableObject = collision.gameObject;
				destroyableObject.AddComponent<CubeExplosion>();
				destroyableObject.tag = "Untagged";
				Kill();
			}
		}
	}
}