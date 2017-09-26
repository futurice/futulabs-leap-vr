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
		[SerializeField] private ParticleSystem _particles;
		[SerializeField] private Light _pointLight;

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
				_particles.Stop();
				_pointLight.DOKill();
				_pointLight.DOIntensity(0, 0.5f);
				_glowingBall.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
				{
					_used = true;
					Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(x =>
					{
						Kill();
					});
				});
			});
		}

		public void Kill()
		{
			Destroy(gameObject);
		}

		bool IsDestroyable(Collision c)
		{
			var t = c.gameObject.tag;
			if(t == "WallCube" || t == "InteractableObject" || t == "Destroyable")
			{
				return true;
			}
			return false;
		}

		void OnCollisionEnter(Collision collision)
		{
			if(IsDestroyable(collision) && !_used)
			{
				_used = true;
				var destroyableObject = collision.gameObject;
				var voxelizer = destroyableObject.AddComponent<VoxelizeGameobject>();
				voxelizer.Voxelize(collision.gameObject.tag=="Destroyable" ? 8 : 4);
				destroyableObject.tag = "Untagged";
				Kill();
			}
		}
	}
}