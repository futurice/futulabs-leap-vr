using UnityEngine;
using Leap.Unity.Interaction;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Leap.Unity.Interaction
{

    /**
    * The InteractiveBrushBone class is a physics element of an InteractiveBrushHand object.
    * It contains Unity physics components that are controlled by the Interaction Engine.
    * @since 4.1.3
    */
    public class InteractionBrushBone : MonoBehaviour
    {

        // Used by InteractionBrushHand:
        /** The active InteractionManager. */
        public InteractionManager manager;
        /** This InteractiveBrushBone's RigidBody. */
        public Rigidbody capsuleBody;
        /** This InteractiveBrushBone's Collider. */
        public Collider collider;
        /** This InteractiveBrushBone's target position. */
        public Vector3 lastTarget;

        [SerializeField]
        private bool _autoStart = false;

        // Once the brush becomes dislocated, it then remains dislocated until it
        // stops triggering and then the _dislocatedCounter expires.
        private const int DISLOCATED_BRUSH_COOLDOWN = 3;
        private int _dislocatedCounter = DISLOCATED_BRUSH_COOLDOWN;

        private void Start()
        {
            if(_autoStart)
            {
                startTriggering();
            }
        }

        /** Changes the collider to react to collisions as a trigger. */
        public void startTriggering()
        {
            collider.isTrigger = true;
            _dislocatedCounter = 0;
        }

        private void Update()
        {
            if (_autoStart)
            {
                updateTriggering();
            }
        }

        /** Determines whether the brush bone should react to collisions as a trigger. */
        public bool updateTriggering()
        {
            if (_dislocatedCounter < DISLOCATED_BRUSH_COOLDOWN)
            {
                if (++_dislocatedCounter == DISLOCATED_BRUSH_COOLDOWN)
                {
                    collider.isTrigger = false;
                    return false;
                }
                return true;
            }
            return false;
        }

        private void tryNotify(Collider other)
        {
            IInteractionBehaviour ib = other.GetComponentInParent<IInteractionBehaviour>();
            if (ib)
            {
                manager.EnsureActive(ib);
                _dislocatedCounter = 0;
                ib.NotifyBrushDislocated();
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            tryNotify(other);
        }

        protected void OnTriggerStay(Collider other)
        {
            tryNotify(other);
        }

#if UNITY_EDITOR
        private string ThisLabel()
        {
            return string.Format("{0} {1} {2} {3}",
                gameObject.name,
                "<layer",
                LayerMask.LayerToName(gameObject.layer),
                ">"
            );
        }

        private string ThatLabel(Collision collision)
        {
            GameObject otherObj = collision.collider.gameObject;
            return string.Format("{0} {1} {2} {3}",
                otherObj.name,
                "<layer",
                LayerMask.LayerToName(gameObject.layer),
                ">"
            );
        }

        private void OnCollisionEnter(Collision collision)
        {
            GameObject otherObj = collision.collider.gameObject;
            if (otherObj.GetComponentInParent<InteractionBehaviourBase>() == null)
            {
                Debug.LogError("For interaction to work properly please prevent collision between an InteractionBrushHand and non-interaction objects. " + ThisLabel() + ", " + ThatLabel(collision));
            }
        }
#endif // UNITY_EDITOR
    }
}
