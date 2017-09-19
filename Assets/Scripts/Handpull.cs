using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UniRx;
using System;

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
        private float _pullRadius = 3f;
        private float _pushMultiplier = 2f;

        private const int _frameCount = 30;
        private Vector3 _currentFramePos;
        private Vector3 _lastFramePos;
        private Vector3[] _velocityFrames = new Vector3[_frameCount];
        private float[] _timeElapsedFrames = new float[_frameCount];
        private int _velocityIndex = 0;

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
            CaptureFrame();
        }

        private void CaptureFrame()
        {
            if (_currentFramePos != null)
                _lastFramePos = _currentFramePos;
            _currentFramePos = transform.position;
            if (_lastFramePos == null)
                return;

            Vector3 difference = _currentFramePos - _lastFramePos;
            _velocityFrames[_velocityIndex] = difference;
            _timeElapsedFrames[_velocityIndex] = Time.deltaTime;
            _velocityIndex++;
            _velocityIndex = _velocityIndex % _frameCount;
        }

        private Vector3 CalculateCurrentVelocity()
        {
            var averageVelocity = new Vector3();
            var averageTime = 0f;
            for (int i = 0; i < _velocityFrames.Length; i++)
            {
                averageVelocity = averageVelocity + _velocityFrames[i];
                averageTime += _timeElapsedFrames[i];
            }
            averageTime /= _frameCount;
            var velocityTimeMultiplier = 1/averageTime;
            averageVelocity /= _frameCount;
            return averageVelocity * velocityTimeMultiplier;
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
            _activated = true;
        }

        public void DeactivePulling()
        {
            _activated = false;
            _pullingObjects = false;
            var velocityToApply = CalculateCurrentVelocity();
            foreach(var obj in _pulledObjects)
            {
                if(GameManager.Instance.IsGravityOn)
                {
                    FadeInGravity(obj);
                }
                obj.RigidBody.velocity = velocityToApply * _pushMultiplier;
            }
            _pulledObjects.Clear();
        }

        private float _gravity = 9.81f;
        private void FadeInGravity(InteractableObjectControllerBase body)
        {
            if(body.SimulatedGravitySubscription != null)
            {
                body.SimulatedGravitySubscription.Dispose();
            }
            float bodyGravity = 0;
            Func<long, bool> gravityReached = (x) => bodyGravity < _gravity;
            var obs = Observable.EveryFixedUpdate().TakeWhile(gravityReached);
            body.SimulatedGravitySubscription = obs.Subscribe(_ =>
            {
                Debug.Log(bodyGravity);
                bodyGravity += _gravity*Time.deltaTime; // bodyGravity reaches gravity in 1 sec
                var gravityVelocity = Vector3.down*bodyGravity * Time.deltaTime;
                body.RigidBody.velocity = body.RigidBody.velocity + gravityVelocity;
            });
            // if we can just subscribe to when the observable 'obs' ends, I don't have to do this:
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                body.UseGravity = true;
            });
        }


    }
}