﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
using System;
using Leap.Unity.Interaction;

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
		[SerializeField] private ParticleSystem _particleFlames;
		[SerializeField] private Light _pointLight;
		[SerializeField] private AudioSource _impactAudio;
		[SerializeField] private AudioSource _flyAudio;
		[SerializeField] private InteractionBehaviour _interactionBehaviour;
		[SerializeField] private ParticleSystem _impactParticlesPrefab;
		[SerializeField] private Vector3 _initialSize;

		private float _initialParticleMinSize;
		private float _initialParticleMaxSize;

		private bool _used = false;
		
		void Awake()
		{
			_glowingBall.localScale = Vector3.zero;
			_glowingBall.DOScale(_targetScale, 0.15f);
			_initialParticleMinSize = _particleFlames.main.startSize.constantMin;
			_initialParticleMaxSize = _particleFlames.main.startSize.constantMax;
		}

		void Update()
		{
			ResetParticleSize();
		}

		public void Throw(Vector3 direction, float velocityMag)
		{
			_interactionBehaviour.isKinematic = true;
			transform.SetParent(null);
			var velocity = direction * velocityMag;
			_interactionBehaviour.isKinematic = false;
			_rigidBody.velocity = velocity;
			_collider.enabled = true;
			Observable.Timer(TimeSpan.FromSeconds(0.05f)).TakeUntilDestroy(gameObject).Subscribe(_ =>
			{
				_flyAudio.Play();
			});
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

		private void ResetParticleSize()
		{
			var size = transform.lossyScale;
			var sizeRatio = size.magnitude / _initialSize.magnitude;
			var main = _particleFlames.main;
			main.startSize = new ParticleSystem.MinMaxCurve(_initialParticleMinSize*sizeRatio, _initialParticleMaxSize*sizeRatio);
			Debug.Log(_particleFlames.main.startSize.constantMin + " - " + _particleFlames.main.startSize.constantMax);
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

		void PlayImpact()
		{
			_impactAudio.transform.SetParent(null);
			var audioLength = _impactAudio.clip.length;
			_impactAudio.Play();
			Observable.Timer(TimeSpan.FromSeconds(audioLength)).Subscribe(_ =>
			{
				Destroy(_impactAudio.gameObject);
			});
		}

		void OnCollisionEnter(Collision collision)
		{
			var particles = Instantiate(_impactParticlesPrefab);
			particles.transform.position = transform.position;
			PlayImpact();
			if(IsDestroyable(collision) && !_used)
			{
				var destroyableObject = collision.gameObject;
				var voxelizer = destroyableObject.AddComponent<VoxelizeGameobject>();
				voxelizer.Voxelize(collision.gameObject.tag=="Destroyable" ? 8 : 4);
				destroyableObject.tag = "Untagged";

			}
			particles.transform.localScale = Vector3.one/3f;
			_used = true;
			Kill();
		}
	}
}