using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;
namespace Futulabs
{
	public class FireballImpact : MonoBehaviour 
	{
		[SerializeField] private ParticleSystem _particles;
		[SerializeField] private Light _pointLight;
		void Start () 
		{
			_particles.Play();
			_pointLight.DOIntensity(0, 1f).SetEase(Ease.InExpo);
			Observable.Timer(System.TimeSpan.FromSeconds(3)).TakeUntilDestroy(this).Subscribe(_ =>
			{
				Destroy(gameObject);
			});
		}
	}
}