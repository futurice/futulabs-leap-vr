using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{
	public class Fireball : MonoBehaviour 
	{
		[SerializeField] private Rigidbody _rigidBody;
		[SerializeField] private Collider _collider;
		public void Throw(Vector3 direction, float velocityMag)
		{
			Debug.Log(direction);
			transform.SetParent(null);
			var velocity = direction * velocityMag;
			_rigidBody.isKinematic = false;
			_rigidBody.velocity = velocity;
			_collider.enabled = true;
		}

		public void Kill()
		{

		}

		void OnCollisionEnter(Collision collision)
		{
			if(collision.gameObject.tag == "Wall")
			{
				collision.gameObject.AddComponent<CubeExplosion>();
				collision.gameObject.tag = "Untagged";
			}
		}
	}
}