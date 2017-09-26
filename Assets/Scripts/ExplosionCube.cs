using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UniRx;
using System;

namespace Futulabs
{
	public class ExplosionCube : MonoBehaviour 
	{
		[SerializeField] private MeshRenderer _renderer;
		private ObjectPool _explosionPool;
		private float _totalFadeTime = 4f;
		void Awake()
		{
			_explosionPool = GameObject.FindGameObjectWithTag("ExplosionPool").GetComponent<ObjectPool>();
		}

		public void FadeOut()
		{
			_totalFadeTime = UnityEngine.Random.Range(0.85f, 1.15f) * _totalFadeTime;
			_renderer.material.DOKill();
			_renderer.material.SetFloat("_EmissionGain", 0);
			_renderer.material.DOFloat(0.68f, "_EmissionGain", _totalFadeTime).SetEase(Ease.OutExpo);
			Observable.Timer(TimeSpan.FromSeconds(_totalFadeTime/2f)).TakeUntilDestroy(this).Subscribe(_ =>
			{
				transform.DOKill();
				transform.DOScale(Vector3.zero, _totalFadeTime/2f).SetEase(Ease.InExpo).OnComplete(() =>
				{
					_explosionPool.Return(gameObject);
				});
			});
		}

	}
}