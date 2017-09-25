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
		private const float _handOpenTime = 1f;
		private IDisposable _handOpeningSubscription;

		private Fireball _fireBallInstance;
		private HandVelocity _rightArm;
		private float _fireVelocityLimit = 0.02f;
		

		void Start()
		{
			_rightArm = GameObject.FindGameObjectWithTag("RightHand").GetComponent<HandVelocity>();
			_rightArm._lastVelocity.TakeUntilDestroy(this).Subscribe(velocity =>
			{
				if(ShouldFire(velocity) && _fireBallInstance != null)
				{
					//Debug.Break();
					_fireBallInstance.Throw(_rightArm._averageVelocity.Value, 1000);
					_fireBallInstance = null;
				}
			});
		}

		private bool IsPointWithinCone(Vector3 coneTipPosition, Vector3 coneCenterLine, Vector3 point, float FOVRadians)
		{
			Vector3 differenceVector = point - coneTipPosition;
			differenceVector.Normalize();
		
			return Vector3.Dot(coneCenterLine, differenceVector) >= Mathf.Cos(FOVRadians);
		}

		private bool ShouldFire(Vector3 handVelocity)
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
				StartFireBall();
			});
		}

		public void HandClose()
		{
			DisposeSubscription(_handOpeningSubscription);
		}

		private void StartFireBall()
		{
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