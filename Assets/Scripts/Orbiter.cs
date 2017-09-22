using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{
	public class Orbiter : MonoBehaviour 
	{
		public HashSet<Rigidbody> _orbitingBodies = new HashSet<Rigidbody>();
		[SerializeField]
		private Rigidbody _thisRigid;
		private bool _orbiting = false;

		private const float _freezeDistance = 0.5f;
		private const float _freezeVelocityAmplifier = 3f;

		public void StartOrbit(List<Rigidbody> bodies)
		{
			_orbiting = true;
			foreach(var b in bodies)
			{
				_orbitingBodies.Add(b);
			}
		}

		public void StopOrbit()
		{
			_orbiting = false;
			_orbitingBodies.Clear();
		}
		
		// Update is called once per frame
		void FixedUpdate () 
		{
			if(_orbiting)
			{
				SimulateOrbit();
			}
		}

		private void SimulateOrbit()
		{
			foreach(var body in _orbitingBodies)
			{
				if(body != null)
				{
					var direction = transform.position - body.position;
					float distance = direction.magnitude;
					bool freeze = distance < _freezeDistance;
					if(!freeze)
					{
						var forceMag = ( _thisRigid.mass * body.mass) * Mathf.Pow(distance, 2f);
						var force = direction.normalized * forceMag;
						body.AddForce(force);
					}
					else
					{
						var velocityMag = Mathf.Pow(distance, 2f) *_freezeVelocityAmplifier;
						var velocity = direction.normalized * velocityMag;
						body.velocity = velocity;
					}
				}
			}
		}
	}
}