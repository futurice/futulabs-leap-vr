using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{
    public class Handpull : MonoBehaviour
    {
        private bool _activated = false;
        private bool _pullingObject = false;
        [SerializeField]
        private Material _physMaterial;

        [Tooltip("Where the physgun should start")]
        [SerializeField]
        private Transform _originStart;
        [Tooltip("Where the physgun should point")]
        [SerializeField]
        private Transform _originForward;

        private InteractableObjectControllerBase _pulledObject; //object currently being pulled
        private Rigidbody _pulledObjectRigidbody;

        [SerializeField]
        private LineRenderer pointer;
        private void FixedUpdate()
        {
            if (_activated)
            {
                if (!_pullingObject)
                    FindObject();
                else
                    PullObject();
            }
        }

        private void FindObject()
        {
            RaycastHit hit;
            Vector3 avgForward = Vector3.Lerp(_originStart.forward, _originForward.forward, 0.5f);
            Physics.Raycast(_originStart.position, avgForward, out hit);
            ChangePointer(_originStart.position, hit.point);
            //Debug.DrawRay(_originTransform.position, _originTransform.forward * 2);
            if (hit.collider.gameObject.tag.Equals("InteractableObject"))
            {
                _pullingObject = true;
                _pulledObject = hit.collider.GetComponent<InteractableObjectControllerBase>();
                _pulledObjectRigidbody = _pulledObject.GetComponent<Rigidbody>();
                _pulledObjectRigidbody.useGravity = false;
            }
        }

        private void ChangePointer(Vector3 start, Vector3 end)
        {
            pointer.SetPosition(0, start);
            pointer.SetPosition(1, Vector3.Lerp(start, end, 0.01f));
            pointer.SetPosition(2, Vector3.Lerp(start, end, 0.5f));
            pointer.SetPosition(3, Vector3.Lerp(start, end, 0.75f));
            pointer.SetPosition(4, end);
        }

        private void PullObject()
        {
            Vector3 velocity = (_originStart.position - _pulledObject.transform.position);
            ChangePointer(_originStart.position, _pulledObject.transform.position);
            _pulledObjectRigidbody.velocity = velocity;
        }

        public void ActivatePulling()
        {
            pointer.enabled = true;
            _activated = true;
        }

        public void DeactivePulling()
        {
            pointer.enabled = false;
            _activated = false;
            _pullingObject = false;
            if (_pulledObjectRigidbody != null)
                _pulledObjectRigidbody.useGravity = GameManager.Instance.IsGravityOn;
            _pulledObject = null;
        }


    }
}