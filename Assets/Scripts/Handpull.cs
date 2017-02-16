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
        private Transform _originTransform;

        private InteractableObjectControllerBase _pulledObject; //object currently being pulled
        private Rigidbody _pulledObjectRigidbody;

        [SerializeField]
        private LineRenderer pointer;
        private float dt;
        private void FixedUpdate()
        {
            if (_activated)
            {
                dt += Time.deltaTime;
                _physMaterial.SetTextureOffset("Offset", new Vector2(dt, 0));
                if (!_pullingObject)
                    FindObject();
                else
                    PullObject();
            }
        }

        private void FindObject()
        {
            RaycastHit hit;
            Physics.Raycast(_originTransform.position, _originTransform.forward, out hit/*, LayerMask.GetMask("Default")*/);
            ChangePointer(_originTransform.position, hit.point);
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
            pointer.SetPosition(1, end);
        }

        private void PullObject()
        {
            Vector3 velocity = (_originTransform.position - _pulledObject.transform.position); //TODO: Magic
            ChangePointer(_originTransform.position, _pulledObject.transform.position);
            _pulledObjectRigidbody.velocity = velocity;
        }

        public void ActivatePulling()
        {
            dt = 0;
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