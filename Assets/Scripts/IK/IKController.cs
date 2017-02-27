using UnityEngine;
using System.Collections.Generic;
using Leap.Unity;

namespace Futulabs
{
    public enum DataSource
    {
        KINECT = 0,
        HTC_VIVE = 1,
        LEAP_MOTION = 2,
        OCULUS_RIFT = 3,
        OTHER = 4
    };

    [System.Serializable]
    public class IKTarget
    {
        public DataSource dataSource;
        public Transform ikTarget;

        public bool IsDataSourceTrackingBone(HumanBodyBones bone)
        {
            switch (dataSource)
            {
                case DataSource.LEAP_MOTION:
                    return IsLeapMotionTrackingBone(bone);
                case DataSource.KINECT:
                    return IsKinectTrackingBone(bone);
                default:
                    return true;
            }
        }

        private bool IsLeapMotionTrackingBone(HumanBodyBones bone)
        {
            Leap.Controller lc = IKDataSourceManager.Instance.LeapController;
            Leap.Frame frame = lc.Frame();
            List<Leap.Hand> hands = frame.Hands;

            bool leftHandTracked = false;
            bool rightHandTracked = false;

            int numHands = hands.Count;

            for (int i = 0; i < numHands; ++i)
            {
                if (hands[i].IsLeft)
                {
                    leftHandTracked = true;
                }
                else if (hands[i].IsRight)
                {
                    rightHandTracked = true;
                }
            }

            switch (bone)
            {
                // Left hand bones
                case HumanBodyBones.LeftHand:
                case HumanBodyBones.LeftIndexDistal:
                case HumanBodyBones.LeftIndexIntermediate:
                case HumanBodyBones.LeftIndexProximal:
                case HumanBodyBones.LeftLittleDistal:
                case HumanBodyBones.LeftLittleIntermediate:
                case HumanBodyBones.LeftLittleProximal:
                case HumanBodyBones.LeftLowerArm:
                case HumanBodyBones.LeftMiddleDistal:
                case HumanBodyBones.LeftMiddleIntermediate:
                case HumanBodyBones.LeftMiddleProximal:
                case HumanBodyBones.LeftRingDistal:
                case HumanBodyBones.LeftRingIntermediate:
                case HumanBodyBones.LeftRingProximal:
                case HumanBodyBones.LeftThumbDistal:
                case HumanBodyBones.LeftThumbIntermediate:
                case HumanBodyBones.LeftThumbProximal:
                    return leftHandTracked;
                
                // Right hand bones
                case HumanBodyBones.RightHand:
                case HumanBodyBones.RightIndexDistal:
                case HumanBodyBones.RightIndexIntermediate:
                case HumanBodyBones.RightIndexProximal:
                case HumanBodyBones.RightLittleDistal:
                case HumanBodyBones.RightLittleIntermediate:
                case HumanBodyBones.RightLittleProximal:
                case HumanBodyBones.RightLowerArm:
                case HumanBodyBones.RightMiddleDistal:
                case HumanBodyBones.RightMiddleIntermediate:
                case HumanBodyBones.RightMiddleProximal:
                case HumanBodyBones.RightRingDistal:
                case HumanBodyBones.RightRingIntermediate:
                case HumanBodyBones.RightRingProximal:
                case HumanBodyBones.RightThumbDistal:
                case HumanBodyBones.RightThumbIntermediate:
                case HumanBodyBones.RightThumbProximal:
                    return rightHandTracked;
                
                // Leap only tracks hands
                default:
                    Debug.LogWarningFormat("IKTarget IsLeapMotionTrackingBone: Invalid bone for Leap Motion: {0} - returning false", bone);
                    return false;
            }
        }

        private bool IsKinectTrackingBone(HumanBodyBones bone)
        {
            // TODO: Specialize for individual joints
            KinectManager km = IKDataSourceManager.Instance.KinectManager;
            return km.IsUserTracked(km.GetPrimaryUserID());
        }
    }

    [System.Serializable]
    public class IKGoal
    {
        public HumanBodyBones bone;
        public AvatarIKGoal ikGoal;
        [Tooltip("IK targets in their order of precedence")]
        public List<IKTarget> ikTargets;
        public float positionWeight = 1.0f;
        public float rotationWeight = 1.0f;
    }

    [System.Serializable]
    public class IKHint
    {
        public HumanBodyBones bone;
        public AvatarIKHint ikHint;
        public List<IKTarget> ikTargets;
        public float positionWeight = 1.0f;
    }

    [RequireComponent(typeof(Animator))]
    public class IKController : MonoBehaviour
    {
        [Header("Root motion")]
        public Transform bodyPositionTransform = null;
        public Transform bodyRotationTransform = null;

        [Header("Inverse kinematics")]
        public bool ikActive = false;

        public Transform lookAtIKTarget = null;
        public float lookAtWeight = 1.0f;

        public List<IKGoal> ikGoals;
        public List<IKHint> ikHints;

        protected Animator _animator;
        protected Vector3 _bodyPositionVector;
        protected Quaternion _bodyRotationVector;
        protected bool _calibrated = false;

        private void Start()
        {
            _bodyPositionVector = Vector3.zero;

            _animator = GetComponent<Animator>();

            int numIKGoals = ikGoals.Count;
        }

        private void OnAnimatorIK()
        {
            if (_animator)
            {
                // If the IK is active, set the position and rotation directly to the goal. 
                if (ikActive)
                {
                    // Set the look target position, if one has been assigned
                    if (lookAtIKTarget != null)
                    {
                        _animator.SetLookAtWeight(1);
                        _animator.SetLookAtPosition(lookAtIKTarget.position);
                    }

                    // Set IK Goals
                    if (ikGoals != null)
                    {
                        int numIKGoals = ikGoals.Count;

                        for (int i = 0; i < numIKGoals; ++i)
                        {
                            UpdateIKGoal(ikGoals[i]);
                        }
                    }

                    // Set IK Hints
                    if (ikHints != null)
                    {
                        int numIKHints = ikHints.Count;

                        for (int i = 0; i < numIKHints; ++i)
                        {
                            UpdateIKHint(ikHints[i]);
                        }
                    }
                    
                    if (bodyPositionTransform != null)
                    { 
                        _bodyPositionVector.Set(
                            bodyPositionTransform.position.x,
                            _animator.bodyPosition.y,
                            bodyPositionTransform.position.z);

                        _animator.bodyPosition = _bodyPositionVector;
                    }

                    if (bodyRotationTransform != null)
                    { 
                        _animator.bodyRotation = bodyRotationTransform.rotation;
                    }
                }
                // If the IK is not active, reset the positions and rotations back to original
                else
                {
                    ResetIK();
                }
            }
        }

        private void UpdateIKGoal(IKGoal goal)
        {
            int numIKTargets = goal.ikTargets == null ? 0 : goal.ikTargets.Count;

            for (int i = 0; i < numIKTargets; ++i)
            {
                if (goal.ikTargets[i].IsDataSourceTrackingBone(goal.bone))
                { 
                    _animator.SetIKPositionWeight(goal.ikGoal, goal.positionWeight);
                    _animator.SetIKRotationWeight(goal.ikGoal, goal.rotationWeight);
                    _animator.SetIKPosition(goal.ikGoal, goal.ikTargets[i].ikTarget.position);
                    _animator.SetIKRotation(goal.ikGoal, goal.ikTargets[i].ikTarget.rotation);
                    return;
                }
            }
        }

        private void UpdateIKHint(IKHint hint)
        {
            int numIKHints = hint.ikTargets == null ? 0 : hint.ikTargets.Count;

            for (int i = 0; i < numIKHints; ++i)
            {
                if (hint.ikTargets[i].IsDataSourceTrackingBone(hint.bone))
                {
                    _animator.SetIKHintPositionWeight(hint.ikHint, hint.positionWeight);
                    _animator.SetIKHintPosition(hint.ikHint, hint.ikTargets[i].ikTarget.position);
                    return;
                }
            }
        }

        private void ResetIKGoals()
        {
            if (ikGoals != null)
            {
                int numIKGoals = ikGoals.Count;

                for (int i = 0; i < numIKGoals; ++i)
                {
                    _animator.SetIKPositionWeight(ikGoals[i].ikGoal, 0);
                    _animator.SetIKRotationWeight(ikGoals[i].ikGoal, 0);
                }
            }
        }

        private void ResetIKHints()
        {
            if (ikHints != null)
            {
                int numIKHints = ikHints.Count;

                for (int i = 0; i < numIKHints; ++i)
                {
                    _animator.SetIKHintPositionWeight(ikHints[i].ikHint, 0);
                }
            }
        }

        private void ResetIK()
        {
            if (!_animator)
            {
                return;
            }

            // Reset look at target
            if (lookAtIKTarget != null)
            {
                _animator.SetLookAtWeight(0);
            }

            ResetIKHints();
            ResetIKGoals();
        }
    }

}