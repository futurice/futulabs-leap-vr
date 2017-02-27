using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using System.Linq;

namespace Futulabs
{

    [System.Serializable]
    public enum ObjectManagerState
    {
        READY = 0,
        CREATING = 1
    }

    [System.Serializable]
    public enum InteractableObjectType
    {
        CUBE = 0,
        ICONOSPHERE = 1,
        STAR = 2
    }

    [System.Serializable]
    public class InteractableObject
    {
        public InteractableObjectType type;
        public GameObject prefab;
    }

    /// <summary>
    /// This class manages the creation of new objects.
    /// </summary>
    public class ObjectManager : Singleton<ObjectManager>
    {
        [Header("ObjectManager")]
        [Header("Interactions")]
        [SerializeField]
        Leap.Unity.Interaction.InteractionManager _interactionManager;

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
        [Tooltip(@"Types and prefabs of the objects that can be created.
            The objects must have a script attached that implements IInteractableObject")]
        private InteractableObject[] _interactableObjects;

        [Header("Object Creation")]
        [SerializeField]
        [Tooltip("Scale factor for the force applied to the object when it's materialized - to allow throwing")]
        private float _creationForceScaleFactor = 180;
        [SerializeField]
        [Tooltip("How many frames should be captured to calculate the force when materialized")]
        private int _creationForceWindowSize = 10;

        // The object that is currently being created
        private IInteractableObjectController _currentObject = null;

        // The objects created so far
        private List<IInteractableObjectController> _createdObjects = null;

        // Index of the current object type in the interactable objects array
        private InteractableObjectType _currentObjectType;
        private int _currentObjectTypeIndex = 0;

        private List<IInteractableObjectController> CreatedObjects
        {
            get
            {
                return ObjectContainer.GetComponentsInChildren<IInteractableObjectController>().ToList();
            }
        }

        public Transform ObjectContainer
        {
            get
            {
                return _objectContainer;
            }
        }

        public float CreationForceScaleFactor
        {
            get
            {
                return _creationForceScaleFactor;
            }
        }

        public int CreationForceWindowSize
        {
            get
            {
                return _creationForceWindowSize;
            }
        }

        public ObjectManagerState CurrentState
        {
            get;
            private set;
        }

        public InteractableObjectType CurrentCreatableObjectType
        {
            get
            {
                return _currentObjectType;
            }

            set
            {
                _currentObjectType = value;
                _currentObjectTypeIndex = Mathf.Clamp(System.Array.FindIndex(_interactableObjects, io => io.type == value), 0, _interactableObjects.Length - 1);
            }
        }

        private void Start()
        {

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
                _currentObject.ChangePitch(_leftHandPinchDetector.Position, _rightHandPinchDetector.Position);
            }
        }

        public void ToggleGravityForInteractableObjects(bool enabled)
        {
            int numObjects = CreatedObjects.Count;

            for (int i = 0; i < numObjects; ++i)
            {
                CreatedObjects[i].UseGravity = enabled;
            }
        }

        private void CreateObject()
        {
            CurrentState = ObjectManagerState.CREATING;

            // Create the object at midpoint between the two pinches
            Vector3 position = (_rightHandPinchDetector.Position - _leftHandPinchDetector.Position) * 0.5f;

            // Create the object and add to a list of objects
            GameObject newObject = Instantiate(_interactableObjects[_currentObjectTypeIndex].prefab, position, Quaternion.identity, _objectContainer) as GameObject;
            _currentObject = newObject.GetComponent<IInteractableObjectController>();
            CreatedObjects.Add(_currentObject);
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

            // Set the gravity status
            _currentObject.LeapInteractionBehaviour.useGravity = GameManager.Instance.IsGravityOn;

            // Reset the ObjectManager state to READY so we can continue creating
            CurrentState = ObjectManagerState.READY;
            _currentObject = null;
        }
    }

}