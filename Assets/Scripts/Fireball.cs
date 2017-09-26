using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Futulabs
{
	public class Fireball : MonoBehaviour 
	{
		[SerializeField] private Rigidbody _rigidBody;
		[SerializeField] private Collider _collider;
		[SerializeField] private Vector3 _targetScale;
		[SerializeField] private Transform _glowingBall;
		
		void Awake()
		{
			_glowingBall.localScale = Vector3.zero;
			_glowingBall.DOScale(_targetScale, 0.3f);
		}

		public void Throw(Vector3 direction, float velocityMag)
		{
			transform.SetParent(null);
			var velocity = direction * velocityMag;
			_rigidBody.isKinematic = false;
			_rigidBody.velocity = velocity;
			_collider.enabled = true;
		}

		public void Kill()
		{
			Destroy(gameObject);
		}

		void OnCollisionEnter(Collision collision)
		{
			if(collision.gameObject.tag == "Wall")
			{
				collision.gameObject.AddComponent<CubeExplosion>();
				collision.gameObject.tag = "Untagged";
				Kill();
			}
		}
	}
}