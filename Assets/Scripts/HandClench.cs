using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Futulabs
{
	
	public class HandClench : MonoBehaviour 
	{
		[SerializeField] private List<Transform> _fingers;
		[SerializeField] private Transform _palmPoint;
		
		public readonly ReactiveProperty<Vector3> AverageFingerToPalmDistance = new ReactiveProperty<Vector3>();

		void Update()
		{
			var totalDistance = Vector3.zero;
			foreach(var f in _fingers)
			{
				totalDistance += f.position - _palmPoint.position;
			}
			totalDistance /= _fingers.Count;
			AverageFingerToPalmDistance.Value = totalDistance;
		}
	}
}