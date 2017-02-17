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

        [SerializeField]
        private LayerMask _ignoreRaycastLayers;

        [SerializeField]
        private Transform _transformPalm;
        [SerializeField]
        private Transform _transformFinger;

        private InteractableObjectControllerBase _pulledObject; //object currently being pulled
        private Rigidbody _pulledObjectRigidbody;

        private Vector3 _oldForward;
        [Tooltip("For the direction forward, how much should it use the palm versus the finger. 1 is finger only")]
        [SerializeField]
        private float _lerpFactorFingerPalm = 0.5f;
        [Tooltip("Used for smoothing from frame to frame, how much should it use the old frame vs the new. 1 is 100% old")]
        [SerializeField]
        private float _lerpFactorOldNew = 0.8f;

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
            Vector3 avgForward = Vector3.Lerp(_transformPalm.forward, _transformFinger.forward, _lerpFactorFingerPalm);
            if (_oldForward != null)
                avgForward = Vector3.Lerp(avgForward, _oldForward, _lerpFactorOldNew);
            Physics.Raycast(_transformPalm.position, avgForward, out hit, Mathf.Infinity, _ignoreRaycastLayers);
            ChangePointer(_transformPalm.position, hit.point);
            if (hit.collider.gameObject.tag.Equals("InteractableObject"))
            {
                _pullingObject = true;
                _pulledObject = hit.collider.GetComponentInParent<InteractableObjectControllerBase>();
                _pulledObjectRigidbody = _pulledObject.Rigidbody;
                _pulledObjectRigidbody.useGravity = false;
            }
            _oldForward = avgForward;
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
            Vector3 velocity = (_transformPalm.position - _pulledObject.Rigidbody.transform.position);
            ChangePointer(_transformPalm.position, _pulledObject.Rigidbody.transform.position);
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