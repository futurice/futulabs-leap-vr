using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{ 
    public class PissingMeOff : MonoBehaviour
    {
        [SerializeField]
        protected Rigidbody _rigidbody;
        [SerializeField]
        protected Transform _target;

        protected Vector3 _lastPosition;

        protected void Start()
        {
            _rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }

        protected void FixedUpdate()
        {
            // Match the position and rotation of the target
            _rigidbody.MovePosition(_target.position);
            _rigidbody.MoveRotation(_target.rotation);

            transform.position = _target.position;
            transform.rotation = _target.rotation;

            // Match the velocity
            float velocity = Vector3.Magnitude(_target.position - _lastPosition) / Time.fixedDeltaTime;
            _rigidbody.velocity = (_target.position - _lastPosition).normalized * velocity;

            _lastPosition = _target.position;
        }

    }
}