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
        protected Quaternion _lastRotation;

        protected float maxDeltaPosition = 0.1f;
        protected float maxDeltaAngle = 10f;

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
            // TODO: FIXME - I am sorry. This had to be done to make the video awesome.
            // This is counting on the fact that the IK will recover within one frame
            // it will start stuttering if not
            if (Vector3.Magnitude(_target.position - _lastPosition) > maxDeltaPosition ||
                Quaternion.Angle(_lastRotation, _target.rotation) > maxDeltaAngle)
            {
                _lastPosition = _target.position;
                _lastRotation = _target.rotation;
                return;
            }

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
            _lastRotation = _target.rotation;
        }

    }
}