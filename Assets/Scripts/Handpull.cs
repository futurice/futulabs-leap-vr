using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Futulabs
{
    public class Handpull : MonoBehaviour
    {
        [SerializeField]
        private Material _physMaterial;

        [SerializeField]
        private LayerMask _ignoreRaycastLayers;

        [SerializeField]
        private Transform _transformPalm;
        
        [SerializeField]
        private Orbiter _orbitScript;

        [SerializeField]
        private LineRenderer _debugLineRenderer;

        private HashSet<InteractableObjectControllerBase> _pulledObjects = new HashSet<InteractableObjectControllerBase>();
        private Vector3 _oldForward;
        private bool _activated = false;
        private bool _pullingObjects = false;
        private float _pullRadius = 2.5f;
        

        private void FixedUpdate()
        {
            _oldForward = _transformPalm.up * -1f;
            if (_activated)
            {
                var bodies = FindObjects();
                _orbitScript.StartOrbit(bodies);
            }
            else
            {
                _orbitScript.StopOrbit();
            }
        }

        private List<Rigidbody> FindObjects()
        {
            var hits = Physics.OverlapSphere(_transformPalm.position + (_transformPalm.up*-1)*_pullRadius, _pullRadius, _ignoreRaycastLayers, QueryTriggerInteraction.UseGlobal);
            foreach(var hit in hits)
            {
                if (hit.gameObject.tag.Equals("InteractableObject"))
                {
                    _pullingObjects = true;
                    var pulledObject = hit.GetComponentInParent<InteractableObjectControllerBase>();
                    _pulledObjects.Add(pulledObject);
                    pulledObject.RigidBody.useGravity = false;
                }
            }
            return _pulledObjects.Select(x => x.RigidBody).ToList();
        }

        void Update()
        {
            if(_oldForward != null)
            {
                _debugLineRenderer.SetPosition(0, _transformPalm.position);
                var endPos = _transformPalm.position + _oldForward*5;
                _debugLineRenderer.SetPosition(1, endPos);
            }
        }

        public void ActivatePulling()
        {
            Debug.Log("Activating");
            _activated = true;
        }

        public void DeactivePulling()
        {
            Debug.Log("Deactivating");
            _activated = false;
            _pullingObjects = false;
            foreach(var obj in _pulledObjects)
            {
                obj.RigidBody.useGravity = GameManager.Instance.IsGravityOn;
            }
            _pulledObjects.Clear();
        }


    }
}