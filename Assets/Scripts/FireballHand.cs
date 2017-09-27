using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;

namespace Futulabs
{
	public class FireballHand : MonoBehaviour 
	{
		[SerializeField] private Fireball _fireBallPrefab;
		[SerializeField] private Transform _throwDirection;
		[SerializeField] private LayerMask _wallMask;
		[SerializeField] private Transform _crossHair;
		private const float _handOpenTime = 1f;
		private IDisposable _handOpeningSubscription;

		private Fireball _fireBallInstance;
		private HandVelocity _rightArm;
		private float _fireVelocityLimit = 0.01f;

		[Header("Sounds")]
		[SerializeField] private AudioSource _throwAudio;

		private bool _canThrow = false;

		void Awake()
		{
			_crossHair.gameObject.SetActive(false);
		}

		void Start()
		{
			_rightArm = GameObject.FindGameObjectWithTag("RightHand").GetComponent<HandVelocity>();
			_rightArm._lastVelocity.TakeUntilDestroy(this).Subscribe(velocity =>
			{
				if(ShouldThrow(velocity) && _fireBallInstance != null && _canThrow)
				{			
					_crossHair.gameObject.SetActive(false);
					_throwAudio.Play();
					var direction =  _throwDirection.forward;
					_fireBallInstance.Throw(direction, 4);
					_fireBallInstance = null;
				}
			});
		}


		void Update()
		{
			RaycastHit hit;
			if(Physics.Raycast(transform.position, _throwDirection.forward, out hit, Mathf.Infinity, _wallMask))
			{
				_crossHair.position = hit.point;
			}
		}

		private bool IsPointWithinCone(Vector3 coneTipPosition, Vector3 coneCenterLine, Vector3 point, float FOVRadians)
		{
			Vector3 differenceVector = point - coneTipPosition;
			differenceVector.Normalize();
		
			return Vector3.Dot(coneCenterLine, differenceVector) >= Mathf.Cos(FOVRadians);
		}

		private bool ShouldThrow(Vector3 handVelocity)
		{
			var velocityDirection = handVelocity.normalized;
			var velocityPosition = transform.position + velocityDirection;
			bool sameDirection = IsPointWithinCone(transform.position, transform.up*-1, velocityPosition, Mathf.PI/4f);
			bool overLimit = handVelocity.magnitude > _fireVelocityLimit;
			return sameDirection && overLimit;
		}

		public void HandOpen()
		{
			DisposeSubscription(_handOpeningSubscription);

			_handOpeningSubscription = Observable.Timer(TimeSpan.FromSeconds(_handOpenTime)).TakeUntilDestroy(this).Subscribe(_ =>
			{
				_canThrow = false;
				StartFireBall();
				Observable.Timer(TimeSpan.FromSeconds(0.75)).TakeUntilDestroy(this).Subscribe(x =>
				{
					_canThrow = true;
				});
			});
		}

		public void HandClose()
		{
			DisposeSubscription(_handOpeningSubscription);
		}

		private void StartFireBall()
		{
			_crossHair.gameObject.SetActive(true);
			if(_fireBallInstance == null)
			{
				_fireBallInstance = Instantiate(_fireBallPrefab) as Fireball;
				_fireBallInstance.transform.SetParent(transform);
				_fireBallInstance.transform.position = transform.position + transform.up * -0.1f;
			}
		}

		private void DisposeSubscription(IDisposable disp)
		{
			if(disp != null)
			{
				disp.Dispose();
			}
		}
	}
}