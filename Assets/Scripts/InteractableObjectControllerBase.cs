using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace Futulabs
{
    [RequireComponent(typeof(Rigidbody))]               // Required by InteractionBehaviour
    [RequireComponent(typeof(LeapRTS))]                 // Required to do transform manipulations with Leap Motion
    [RequireComponent(typeof(InteractionBehaviour))]    // Required to for physics interactions with the Leap Motions hands
    public class InteractableObjectControllerBase : MonoBehaviour, IInteractableObjectController
    {
        [Header("Meshes")]
        [SerializeField]
        [Tooltip("Meshes that should be enabled when the object is being created i.e. outlines")]
        protected MeshRenderer[] _outlineMeshes;
        [SerializeField]
        [Tooltip("Meshes that should be enabled when the object has materialized")]
        protected MeshRenderer[] _materializedMeshes;

        [Header("Scaling")]
        [SerializeField]
        [Tooltip("Does the script implement its own scaling or should it use the Leap RTS default uniform scaling")]
        protected bool _overrideLeapRTSScaling = false;
        [SerializeField]
        [Tooltip("Minimum scale this object can get on any axis. Only affects if Leap RTS Scaling is overridden")]
        protected float _minScale = 0.01f;
        [SerializeField]
        [Tooltip("Maximum scale this object can be morphed to on any axis. Only affects if Leap RTS Scaling is overriden")]
        protected float _maxScale = 0.05f;

        protected LeapRTS _leapRTSComponent;
        protected InteractionBehaviour _leapInteractionBehaviour;
        protected Collider[] _colliders;
        protected Rigidbody[] _rigidbodies;

        public LeapRTS LeapRTSComponent
        {
            get
            {
                if (_leapRTSComponent == null)
                {
                    _leapRTSComponent = GetComponent<LeapRTS>();
                }

                return _leapRTSComponent;
            }
        }

        public InteractionBehaviour LeapInteractionBehaviour
        {
            get
            {
                if (_leapInteractionBehaviour == null)
                {
                    _leapInteractionBehaviour = GetComponent<InteractionBehaviour>();
                }

                return _leapInteractionBehaviour;
            }
        }

        public bool OverrideLeapRTSScaling
        {
            get
            {
                return _overrideLeapRTSScaling;
            }
        }

        public bool UseGravity
        {
            get
            {
                return LeapInteractionBehaviour.useGravity;
            }

            set
            {
                LeapInteractionBehaviour.useGravity = value;
            }
        }

        protected Collider[] Colliders
        {
            get
            {
                if (_colliders == null)
                {
                    _colliders = GetComponentsInChildren<Collider>();
                }

                return _colliders;
            }
        }

        protected Rigidbody[] Rigidbodies
        {
            get
            {
                if (_rigidbodies == null)
                {
                    _rigidbodies = GetComponentsInChildren<Rigidbody>();
                }

                return _rigidbodies;
            }
        }

        virtual public void Create(InteractionManager interactionManager, PinchDetector leftPinchDetector, PinchDetector rightPinchDetector)
        {
            LeapRTSComponent.AllowScale = !OverrideLeapRTSScaling;
            LeapRTSComponent.enabled = true;
            LeapRTSComponent.PinchDetectorA = leftPinchDetector;
            LeapRTSComponent.PinchDetectorB = rightPinchDetector;

            LeapInteractionBehaviour.enabled = false;
            LeapInteractionBehaviour.Manager = interactionManager;

            // Turn off Collider and Rigidbody components to disable physics
            EnableCollidersAndRigidbodies(false);

            // Turn off materialized meshes
            EnableMaterializedMeshes(false);

            // Turn on outline meshes
            EnableOutlineMeshes(true);
        }

        virtual public void Materialize()
        {
            // Turn off the Leap RTS controller - there can be only one active at a time
            LeapRTSComponent.enabled = false;
            LeapInteractionBehaviour.enabled = true;

            // Turn on Collider and Rigidbody components to enable physics
            EnableCollidersAndRigidbodies(true);

            // Turn off the outline meshes
            EnableOutlineMeshes(false);

            // Turn on materialized meshes
            EnableMaterializedMeshes(true);
        }

        virtual public void Morph(Vector3 leftPinchPosition, Vector3 rightPinchPosition)
        {
            // No need to do this - the Leap RTS uniform scaling works fine for cubes
        }

        virtual protected void EnableOutlineMeshes(bool enabled)
        {
            int numOutlineMeshes = _outlineMeshes.Length;

            for (int i = 0; i < numOutlineMeshes; ++i)
            {
                _outlineMeshes[i].enabled = enabled;
            }
        }

        virtual protected void EnableMaterializedMeshes(bool enabled)
        {
            int numMaterializedMeshes = _materializedMeshes.Length;

            for (int i = 0; i < numMaterializedMeshes; ++i)
            {
                _materializedMeshes[i].enabled = enabled;
            }
        }

        virtual protected void EnableCollidersAndRigidbodies(bool enabled)
        {
            Collider[] colliders = Colliders;
            int numColliders = colliders.Length;

            for (int i = 0; i < numColliders; ++i)
            {
                colliders[i].enabled = enabled;
            }

            LeapInteractionBehaviour.isKinematic = !enabled;
        }
    }

}