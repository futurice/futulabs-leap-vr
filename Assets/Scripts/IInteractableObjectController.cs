using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace Futulabs
{

interface IInteractableObjectController
{
    /// <summary>
    /// Returns the required LeapRTS component that controls the object's
    /// transform wrt. to hand gestures.
    /// </summary>
    LeapRTS LeapRTSComponent
    {
        get;
    }
    
    /// <summary>
    /// A Leap library interaction behaviour that will be enabled after creation.
    /// </summary>
    InteractionBehaviour LeapInteractionBehaviour
    {
        get;
    }

    /// <summary>
    /// Should the LeapRTSComponent scaling be overridden? If the scaling is overriden
    /// the scaling can be done in the Morph function.
    /// </summary>
    bool OverrideLeapRTSScaling
    {
        get;
    }

    /// <summary>
    /// Creates the object with Rigidbody and Collider components disabled.
    /// The object must have an active Leap RTS component that controls the
    /// scaling and transform.
    /// </summary>
    void Create(InteractionManager interactionManager, PinchDetector leftPinchDetector, PinchDetector rightPinchDetector);

    /// <summary>
    /// This function materializes the object i.e. enables the Collider and
    /// Rigidbody components and disables/removes the Leap RTS component.
    /// </summary>
    void Materialize();

    /// <summary>
    /// Morphs the object according to the difference in the pinch positions.
    /// The morphing changes the scale of the object, possibly non-uniformly.
    /// </summary>
    /// <param name="leftPinchPosition"></param>
    /// <param name="rightPinchPosition"></param>
    void Morph(Vector3 leftPinchPosition, Vector3 rightPinchPosition);
}

}
