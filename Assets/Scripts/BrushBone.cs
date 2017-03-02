using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{ 
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
    public class BrushBone : MonoBehaviour
    {
        [SerializeField]
        protected Transform _target;

		protected Rigidbody _boneRigidbody;  // Rigidbody of the bone
        protected Vector3 _lastPosition; // Last position of the target transform

		protected Rigidbody BoneRigidbody
		{
			get
			{
				if (_boneRigidbody == null)
				{
					_boneRigidbody = GetComponent<Rigidbody>();
				}

				return _boneRigidbody;
			}
		}

        protected void Start()
        {
			BoneRigidbody.useGravity = false;
			BoneRigidbody.isKinematic = false;
			BoneRigidbody.constraints = RigidbodyConstraints.FreezeAll;
			BoneRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			BoneRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

			BoneRigidbody.drag = 0.0f;
			BoneRigidbody.angularDrag = 0.0f;
        }

        protected void FixedUpdate()
        {
            // Match the position and rotation of the target
			BoneRigidbody.MovePosition(_target.position);
			BoneRigidbody.MoveRotation(_target.rotation);

            transform.position = _target.position;
            transform.rotation = _target.rotation;

            // Match the velocity
            float velocity = Vector3.Magnitude(_target.position - _lastPosition) / Time.fixedDeltaTime;
			BoneRigidbody.velocity = (_target.position - _lastPosition).normalized * velocity;

			// TODO: Match angular velocity

            _lastPosition = _target.position;
        }

    }
}