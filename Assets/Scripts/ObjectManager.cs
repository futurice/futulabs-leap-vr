using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace Futulabs
{

/// <summary>
/// This class manages the creation of new objects.
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    public enum ObjectManagerState
    {
        READY       = 0,
        CREATING    = 1
    }

    [Header("Interactions")]
    [SerializeField]
    InteractionManager _interactionManager;
    [Header("Pinch")]
    [SerializeField]
    private PinchDetector _leftHandPinchDetector;
    [SerializeField]
    private PinchDetector _rightHandPinchDetector;
    [SerializeField]
    [Tooltip("Maximum distance between pinches, where the object creation activates.")]
    private float _maxCreationActivationDistance = 0.01f;

    [Header("Objects")]
    [SerializeField]
    private Transform _objectContainer;
    [SerializeField]
    [Tooltip(@"Prefabs of the objects that can be created.
            The objects must have a script attached that implements IInteractableObject")]
    private GameObject[] _interactableObjectPrefabs;

    // The object that is currently being created
    private IInteractableObjectController _currentObject = null;

    // The objects created so far
    private List<IInteractableObjectController> _createdObjects = null;

    public ObjectManagerState CurrentState
    {
        get;
        private set;
    }

    private void Awake()
    {
        _createdObjects = new List<IInteractableObjectController>();
        
        // Register handlers that materialize the object when the pinch is released
        _leftHandPinchDetector.OnDeactivate.AddListener(() =>
        {
            MaterializeObject();
        });

        _rightHandPinchDetector.OnDeactivate.AddListener(() =>
        {
            MaterializeObject();
        });

        CurrentState = ObjectManagerState.READY;
    }

	private void Update()
    {
        // If the object manager is ready i.e. not busy creating an object
        if (CurrentState == ObjectManagerState.READY)
        {
            // If we are pinching with both hands
            if (_leftHandPinchDetector.IsPinching && _rightHandPinchDetector.IsPinching)
            {
                // If the pinch distance is less than the maximum creation activation distance
                if (Vector3.Distance(_leftHandPinchDetector.Position, _rightHandPinchDetector.Position) < _maxCreationActivationDistance)
                {
                    CreateObject();
                }
            }
        }
        else if (CurrentState == ObjectManagerState.CREATING)
        {
            if (_currentObject.OverrideLeapRTSScaling)
            { 
                _currentObject.Morph(_leftHandPinchDetector.Position, _rightHandPinchDetector.Position);
            }
        }
    }

    public void ToggleGravityForInteractableObjects(bool enabled)
    {
        int numObjects = _createdObjects.Count;

        for (int i = 0; i < numObjects; ++i)
        {
            _createdObjects[i].UseGravity = enabled;
        }
    }

    private void CreateObject()
    {
        CurrentState = ObjectManagerState.CREATING;
            
        // Create the object at midpoint between the two pinches
        Vector3 position = (_rightHandPinchDetector.Position - _leftHandPinchDetector.Position) * 0.5f;

        // TODO: Select object according to menu selection
        int objectIndex = Random.Range(0, _interactableObjectPrefabs.Length);

        // Create the object and add to a list of objects
        GameObject newObject = Instantiate(_interactableObjectPrefabs[objectIndex], position, Quaternion.identity, _objectContainer) as GameObject;
        _currentObject = newObject.GetComponent<IInteractableObjectController>();
        _createdObjects.Add(_currentObject);
        _currentObject.Create(_interactionManager, _leftHandPinchDetector, _rightHandPinchDetector);
    }

    private void MaterializeObject()
    {
        // If we don't have an object to materialize - return
        if (_currentObject == null)
        {
            return;
        }

        _currentObject.Materialize();

        // Reset the ObjectManager state to READY so we can continue creating
        CurrentState = ObjectManagerState.READY;
        _currentObject = null;
    }
}

}