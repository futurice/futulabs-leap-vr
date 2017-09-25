using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
namespace Futulabs
{
	public class HandVelocity : MonoBehaviour 
	{
		public readonly IReactiveProperty<Vector3> _averageVelocity = new ReactiveProperty<Vector3>(); // over 10 frames
		public readonly IReactiveProperty<Vector3> _lastVelocity = new ReactiveProperty<Vector3>(); // over 1 frame
		[SerializeField] private Transform _target;

		private Vector3[] _averageVelocities = new Vector3[_avgFrameAmount];
		private const int _avgFrameAmount = 10;
		private int _avgFrameIndex = 0;
		private Vector3 _lastPosition;
		private Vector3 _averageVelocityTemp;

		void Start()
		{
			_lastPosition = _target.position;
		}

		void Update()
		{
			var frameVelocity = _target.position - _lastPosition; // check up on this
			_lastVelocity.Value = frameVelocity;
			_averageVelocities[_avgFrameIndex] = frameVelocity;
			_avgFrameIndex++;
			_avgFrameIndex = (int)Mathf.Repeat(_avgFrameIndex, _avgFrameAmount);

			_averageVelocityTemp = Vector3.zero;
			foreach(Vector3 v in _averageVelocities)
			{
				_averageVelocityTemp += v;
			}
			_averageVelocityTemp /= _avgFrameAmount;
			_averageVelocity.Value = _averageVelocityTemp;

			_lastPosition = _target.position;

			//Debug.DrawLine(transform.position, transform.position + _averageVelocity.Value * 100);
		}

	}
}