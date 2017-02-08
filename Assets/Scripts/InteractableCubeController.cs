using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

namespace Futulabs
{

[RequireComponent(typeof(LeapRTS))]
public class InteractableCubeController : MonoBehaviour, IInteractableObjectController
{
    [Header("Meshes")]
    [SerializeField]
    [Tooltip("Meshes that should be enabled when the object is being created i.e. outlines")]
    private MeshRenderer[] _outlineMeshes;
    [SerializeField]
    [Tooltip("Meshes that should be enabled when the object has materialized")]
    private MeshRenderer[] _materializedMeshes;

    [Header("Scaling")]
    [SerializeField]
    [Tooltip("Does the script implement it's own scaling or should it use the Leap RTS default uniform scaling")]
    private bool _overrideLeapRTSScaling = false;
    [SerializeField]
    [Tooltip("Minimum scale this object can get on any axis. Only affects if Leap RTS Scaling is overridden")]
    private float _minScale = 0.01f;
    [SerializeField]
    [Tooltip("Maximum scale this object can be morphed to on any axis. Only affects if Leap RTS Scaling is overriden")]
    private float _maxScale = 0.05f;

    private LeapRTS _leapRTSComponent;
    private Collider[] _colliders;
    private Rigidbody[] _rigidbodies;
    
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
    
    public bool OverrideLeapRTSScaling
    {
        get
        {
            return _overrideLeapRTSScaling;
        }
    }

    private Collider[] Colliders
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

    private Rigidbody[] Rigidbodies
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

    public void Create(PinchDetector leftPinchDetector, PinchDetector rightPinchDetector)
    {
       LeapRTSComponent.AllowScale = !OverrideLeapRTSScaling;
       LeapRTSComponent.enabled = true;
       LeapRTSComponent.PinchDetectorA = leftPinchDetector;
       LeapRTSComponent.PinchDetectorB = rightPinchDetector;

       // Turn off Collider and Rigidbody components to disable physics
       EnableCollidersAndRigidbodies(false);

       // Turn off materialized meshes
       EnableMaterializedMeshes(false);

       // Turn on outline meshes
       EnableOutlineMeshes(true);
    }

    public void Materialize()
    {
       // Turn off the Leap RTS controller - there can be only one active at a time
       LeapRTSComponent.enabled = false;

       // Turn on Collider and Rigidbody components to enable physics
       EnableCollidersAndRigidbodies(true);

       // Turn off the outline meshes
       EnableOutlineMeshes(false);

       // Turn on materialized meshes
       EnableMaterializedMeshes(true);
    }

    public void Morph(Vector3 leftPinchPosition, Vector3 rightPinchPosition)
    {
        // No need to do this - the Leap RTS uniform scaling works fine for cubes
    }

    private void EnableOutlineMeshes(bool enabled)
    {
        int numOutlineMeshes = _outlineMeshes.Length;

        for (int i = 0; i < numOutlineMeshes; ++i)
        {
            _outlineMeshes[i].enabled = enabled;
        }
    }
    
    private void EnableMaterializedMeshes(bool enabled)
    {
        int numMaterializedMeshes = _materializedMeshes.Length;

        for (int i = 0; i < numMaterializedMeshes; ++i)
        {
            _materializedMeshes[i].enabled = enabled;
        }
    }

    private void EnableCollidersAndRigidbodies (bool enabled)
    {
        Collider[] colliders = Colliders;
        int numColliders = colliders.Length;

        for (int i = 0; i < numColliders; ++i)
        {
            colliders[i].enabled = enabled;
        }

        Rigidbody[] rigidbodies = Rigidbodies;
        int numRigidbodies = rigidbodies.Length;

        for (int i = 0; i < numRigidbodies; ++i)
        {
            rigidbodies[i].isKinematic = !enabled;
            rigidbodies[i].detectCollisions = enabled;
        }
     }
}

}