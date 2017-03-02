using UnityEngine;
using System.Collections.Generic;
using Leap.Unity;
using RootMotion.FinalIK;

namespace Futulabs
{
	public enum DataSource
	{
		KINECT = 0,
		HTC_VIVE = 1,
		LEAP_MOTION = 2,
		OCULUS_RIFT = 3,
		OCULUS_TOUCH = 4,
		OTHER = 5
	};

	public enum VRIKGoal
	{
		SPINE = 0,
		LEFT_ARM = 1,
		RIGHT_ARM = 2,
		LEFT_LEG = 3,
		RIGHT_LEG = 4
	}

	[System.Serializable]
	public class IKTarget
	{
		public DataSource dataSource;
		public Transform ikTargetTransform;
		public float positionWeight = 1.0f;
		public float rotationWeight = 1.0f;
		public float trackingLostThresholdTime = 0.5f; // Threshold time to report that tracking is lost
		[HideInInspector]
		public float lastTrackingInformationReceivedTime = -1.0f;

		public bool IsDataSourceTrackingBone(HumanBodyBones bone)
		{
			bool isTracking = false;

			switch (dataSource)
			{
				case DataSource.LEAP_MOTION:
				{
					isTracking = IsLeapMotionTrackingBone(bone);
					break;
				}
				case DataSource.OCULUS_TOUCH:
				{
					isTracking = IsOculusTouchTrackingBone(bone);
					break;
				}
				default:
				{
					isTracking = true;
					break;
				}
			}

			// If we are tracking log the tracking information received time
			if (isTracking)
			{
				lastTrackingInformationReceivedTime = Time.time;
			}

			// If we are not tracking but we are within the tracking lost threshold time return true
			if (!isTracking && Time.time - lastTrackingInformationReceivedTime > trackingLostThresholdTime)
			{
				return true;
			}

			return isTracking;
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

		private bool IsOculusTouchTrackingBone(HumanBodyBones bone)
		{
			// We assume that the left controller is used to track bones on the left side
			// and the right controller is used to track the bones on the right side
			switch (bone)
			{
				// Left bones
				case HumanBodyBones.LeftEye:
				case HumanBodyBones.LeftShoulder:
				case HumanBodyBones.LeftUpperLeg:
				case HumanBodyBones.LeftLowerLeg:
				case HumanBodyBones.LeftFoot:
				case HumanBodyBones.LeftToes:
				case HumanBodyBones.LeftUpperArm:
				case HumanBodyBones.LeftLowerArm:
				case HumanBodyBones.LeftHand:
				case HumanBodyBones.LeftIndexDistal:
				case HumanBodyBones.LeftIndexIntermediate:
				case HumanBodyBones.LeftIndexProximal:
				case HumanBodyBones.LeftLittleDistal:
				case HumanBodyBones.LeftLittleIntermediate:
				case HumanBodyBones.LeftLittleProximal:
				case HumanBodyBones.LeftMiddleDistal:
				case HumanBodyBones.LeftMiddleIntermediate:
				case HumanBodyBones.LeftMiddleProximal:
				case HumanBodyBones.LeftRingDistal:
				case HumanBodyBones.LeftRingIntermediate:
				case HumanBodyBones.LeftRingProximal:
				case HumanBodyBones.LeftThumbDistal:
				case HumanBodyBones.LeftThumbIntermediate:
				case HumanBodyBones.LeftThumbProximal:
					return OVRInput.GetControllerPositionTracked(OVRInput.Controller.LTouch) && OVRInput.GetControllerOrientationTracked(OVRInput.Controller.LTouch);

				// Right bones
				case HumanBodyBones.RightEye:
				case HumanBodyBones.RightShoulder:

				case HumanBodyBones.RightUpperLeg:
				case HumanBodyBones.RightLowerLeg:
				case HumanBodyBones.RightFoot:
				case HumanBodyBones.RightToes:

				case HumanBodyBones.RightUpperArm:
				case HumanBodyBones.RightLowerArm:
				case HumanBodyBones.RightHand:
				case HumanBodyBones.RightIndexDistal:
				case HumanBodyBones.RightIndexIntermediate:
				case HumanBodyBones.RightIndexProximal:
				case HumanBodyBones.RightLittleDistal:
				case HumanBodyBones.RightLittleIntermediate:
				case HumanBodyBones.RightLittleProximal:
				case HumanBodyBones.RightMiddleDistal:
				case HumanBodyBones.RightMiddleIntermediate:
				case HumanBodyBones.RightMiddleProximal:
				case HumanBodyBones.RightRingDistal:
				case HumanBodyBones.RightRingIntermediate:
				case HumanBodyBones.RightRingProximal:
				case HumanBodyBones.RightThumbDistal:
				case HumanBodyBones.RightThumbIntermediate:
				case HumanBodyBones.RightThumbProximal:
					return OVRInput.GetControllerPositionTracked(OVRInput.Controller.RTouch) && OVRInput.GetControllerOrientationTracked(OVRInput.Controller.RTouch);

				default:
					Debug.LogWarningFormat("IKTarget IsOculusTouchTrackingBone: Invalid bone for Oculus Touch: {0} - returning false", bone);
					return false;
			}
		}
	}

	[System.Serializable]
	public class IKGoal
	{
		public HumanBodyBones bone;
		public VRIKGoal VRIKGoal;
		[Tooltip("IK targets in their order of precedence")]
		public List<IKTarget> ikTargets;

		public IKTarget GetBestIKTarget()
		{
			int numIKTargets = ikTargets == null ? 0 : ikTargets.Count;

			for (int i = 0; i < numIKTargets; ++i)
			{
				if (ikTargets[i].IsDataSourceTrackingBone(bone))
				{
					return ikTargets[i];
				}
			}

			// Return null if there are no targets tracking
			return null;
		}
	}
		
	[RequireComponent(typeof(VRIK))]
	public class VRIKController : MonoBehaviour
	{
		public List<IKGoal> ikGoals;
		protected VRIK _vrik;

		private void Start()
		{
			_vrik = GetComponent<VRIK>();
			int numIKGoals = ikGoals.Count;
		}

		private void LateUpdate()
		{
			if (_vrik)
			{
				// Set IK Goals
				if (ikGoals != null)
				{
					int numIKGoals = ikGoals.Count;

					for (int i = 0; i < numIKGoals; ++i)
					{
						UpdateIKGoal(ikGoals[i]);
					}
				}
			}
			// If the IK is not active, reset the positions and rotations back to original
			else
			{
				ResetIK();
			}
		}

		private void UpdateIKGoal(IKGoal goal)
		{
			IKTarget bestIKTarget = goal.GetBestIKTarget();

			Transform targetTransform = bestIKTarget != null ? bestIKTarget.ikTargetTransform : null;
			float positionWeight = bestIKTarget != null ? bestIKTarget.positionWeight : 0f;
			float rotationWeight = bestIKTarget != null ? bestIKTarget.rotationWeight : 0f;

			switch (goal.VRIKGoal)
			{
				case VRIKGoal.LEFT_ARM:
				{
					_vrik.solver.leftArm.target = targetTransform;
					_vrik.solver.leftArm.positionWeight = positionWeight;
					_vrik.solver.leftArm.rotationWeight = rotationWeight;
					break;
				}
				case VRIKGoal.RIGHT_ARM:
				{
					_vrik.solver.rightArm.target = targetTransform;
					_vrik.solver.rightArm.positionWeight = positionWeight;
					_vrik.solver.rightArm.rotationWeight = rotationWeight;
					break;
				}
				case VRIKGoal.LEFT_LEG:
				{
					_vrik.solver.leftLeg.target = targetTransform;
					_vrik.solver.leftLeg.positionWeight = positionWeight;
					_vrik.solver.leftLeg.rotationWeight = rotationWeight;
					break;
				}
				case VRIKGoal.RIGHT_LEG:
				{
					_vrik.solver.rightLeg.target = targetTransform;
					_vrik.solver.rightLeg.positionWeight = positionWeight;
					_vrik.solver.rightLeg.rotationWeight = rotationWeight;
					break;
				}
			}
		}

		private void ResetIKGoal(IKGoal goal)
		{
			switch (goal.VRIKGoal)
			{
				case VRIKGoal.LEFT_ARM:
				{
					_vrik.solver.leftArm.target = null;
					_vrik.solver.leftArm.positionWeight = 0f;
					_vrik.solver.leftArm.rotationWeight = 0f;
					break;
				}
				case VRIKGoal.RIGHT_ARM:
				{
					_vrik.solver.rightArm.target = null;
					_vrik.solver.rightArm.positionWeight = 0f;
					_vrik.solver.rightArm.rotationWeight = 0f;
					break;
				}
				case VRIKGoal.LEFT_LEG:
				{
					_vrik.solver.leftLeg.target = null;
					_vrik.solver.leftLeg.positionWeight = 0f;
					_vrik.solver.leftLeg.rotationWeight = 0f;
					break;
				}
				case VRIKGoal.RIGHT_LEG:
				{
					_vrik.solver.rightLeg.target = null;
					_vrik.solver.rightLeg.positionWeight = 0f;
					_vrik.solver.rightLeg.rotationWeight = 0f;
					break;
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
					ResetIKGoal(ikGoals[i]);
				}
			}
		}

		private void ResetIK()
		{
			if (!_vrik)
			{
				return;
			}
			
			ResetIKGoals();
		}

	}
}