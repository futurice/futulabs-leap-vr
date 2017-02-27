using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using UnityEngine.UI; 

public delegate void Kick ();

/// <summary>
/// Avatar controller is the component that transfers the captured user motion to a humanoid model (avatar).
/// </summary>
[RequireComponent(typeof(Animator))]
public class AvatarController : MonoBehaviour
{	
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Whether the avatar is facing the player or not.")]
	public bool mirroredMovement = false;

	[Tooltip("Whether the avatar is allowed to move vertically or not.")]
	public bool verticalMovement = false;

	[Tooltip("Whether the avatar's root motion is applied by other component or script.")]
	public bool externalRootMotion = false;

	[Tooltip("Whether the finger orientations are allowed or not.")]
	public bool fingerOrientations = false;

	[Tooltip("Rate at which the avatar will move through the scene.")]
	public float moveRate = 1f;

	[Tooltip("Smooth factor used for avatar movements and joint rotations.")]
	public float smoothFactor = 5f;

	[Tooltip("Game object this transform is relative to (optional).")]
	public GameObject offsetNode;

	[Tooltip("If enabled, makes the avatar position relative to this camera to be the same as the player's position to the sensor.")]
	public Camera posRelativeToCamera;

	[Tooltip("Whether the avatar's position should match the color image (in Pos-rel-to-camera mode only).")]
	public bool posRelOverlayColor = false;

	[Tooltip("Whether z-axis movement needs to be inverted (Pos-Relative mode only).")]
	public bool posRelInvertedZ = false;

	[Tooltip("Whether the avatar's feet must stick to the ground.")]
	public bool groundedFeet = false;

	[Tooltip("Vertical offset of the avatar to the user's spine-base.")]
	public float verticalOffset = 0f;

	// userId of the player
	[NonSerialized]
	public Int64 playerId = 0;

	// The body root node
	protected Transform bodyRoot;

	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	protected Transform[] bones;

	// Rotations of the bones when the Kinect tracking starts.
	protected Quaternion[] initialRotations;
	protected Quaternion[] inverseInitialRotations;
	protected Quaternion[] localRotations;
	protected bool[] isBoneDisabled;

	// Local rotations of finger bones
	protected Dictionary<HumanBodyBones, Quaternion> fingerBoneLocalRotations = new Dictionary<HumanBodyBones, Quaternion>();
	protected Dictionary<HumanBodyBones, Vector3> fingerBoneLocalAxes = new Dictionary<HumanBodyBones, Vector3>();

	// Initial position and rotation of the transform
	protected Vector3 initialPosition;
	protected Quaternion initialRotation;
	protected Quaternion inverseInitialRotation;
	protected Vector3 offsetNodePos;
	protected Quaternion offsetNodeRot;
	protected Vector3 bodyRootPosition;

	// Calibration Offset Variables for Character Position.
	protected bool offsetCalibrated = false;
	protected Vector3 offsetPos = Vector3.zero;

	// whether the parent transform obeys physics
	protected bool isRigidBody = false;

	// private instance of the KinectManager
	protected KinectManager kinectManager;

	// last hand events
	private InteractionManager.HandEventType lastLeftHandEvent = InteractionManager.HandEventType.Release;
	private InteractionManager.HandEventType lastRightHandEvent = InteractionManager.HandEventType.Release;

	// fist states
	private bool bLeftFistDone = false;
	private bool bRightFistDone = false;

	// grounder constants and variables
	private const int raycastLayers = ~2;  // Ignore Raycast
	private const float maxFootDistanceGround = 0.05f;  // maximum distance from lower foot to the ground
	private const float maxFootDistanceTime = 0.5f; // 1.0f;  // maximum allowed time, the lower foot to be distant from the ground
	private Transform leftFoot, rightFoot;

	private float fFootDistanceInitial = 0f;
	private float fFootDistance = 0f;
	private float fFootDistanceTime = 0f;


	/// <summary>
	/// Gets the number of bone transforms (array length).
	/// </summary>
	/// <returns>The number of bone transforms.</returns>
	public int GetBoneTransformCount()
	{
		return bones != null ? bones.Length : 0;
	}

	/// <summary>
	/// Gets the bone transform by index.
	/// </summary>
	/// <returns>The bone transform.</returns>
	/// <param name="index">Index</param>
	public Transform GetBoneTransform(int index)
	{
		if (index >= 0 && index < bones.Length)
		{
			return bones[index];
		}

		return null;
	}

	/// <summary>
	/// Disables the bone and optionally resets its orientation.
	/// </summary>
	/// <param name="index">Bone index.</param>
	/// <param name="resetBone">If set to <c>true</c> resets bone orientation.</param>
	public void DisableBone(int index, bool resetBone)
	{
		if (index >= 0 && index < bones.Length)
		{
			isBoneDisabled[index] = true;

			if (resetBone && bones[index] != null) 
			{
				bones[index].rotation = localRotations[index];
			}
		}
	}

	/// <summary>
	/// Enables the bone, so AvatarController could update its orientation.
	/// </summary>
	/// <param name="index">Bone index.</param>
	public void EnableBone(int index)
	{
		if (index >= 0 && index < bones.Length)
		{
			isBoneDisabled[index] = false;
		}
	}

	/// <summary>
	/// Determines whether the bone orientation update is enabled or not.
	/// </summary>
	/// <returns><c>true</c> if the bone update is enabled; otherwise, <c>false</c>.</returns>
	/// <param name="index">Bone index.</param>
	public bool IsBoneEnabled(int index)
	{
		if (index >= 0 && index < bones.Length)
		{
			return !isBoneDisabled[index];
		}

		return false;
	}

	/// <summary>
	/// Gets the bone index by joint type.
	/// </summary>
	/// <returns>The bone index.</returns>
	/// <param name="joint">Joint type</param>
	/// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
	public int GetBoneIndexByJoint(KinectInterop.JointType joint, bool bMirrored)
	{
		int boneIndex = -1;

		if (jointMap2boneIndex.ContainsKey(joint))
		{
			boneIndex = !bMirrored ? jointMap2boneIndex[joint] : mirrorJointMap2boneIndex[joint];
		}

		return boneIndex;
	}

	/// <summary>
	/// Gets the special index by two joint types.
	/// </summary>
	/// <returns>The spec index by joint.</returns>
	/// <param name="joint1">Joint 1 type.</param>
	/// <param name="joint2">Joint 2 type.</param>
	/// <param name="bMirrored">If set to <c>true</c> gets the mirrored joint index.</param>
	public int GetSpecIndexByJoint(KinectInterop.JointType joint1, KinectInterop.JointType joint2, bool bMirrored)
	{
		int boneIndex = -1;

		if ((joint1 == KinectInterop.JointType.ShoulderLeft && joint2 == KinectInterop.JointType.SpineShoulder) ||
			(joint2 == KinectInterop.JointType.ShoulderLeft && joint1 == KinectInterop.JointType.SpineShoulder))
		{
			return (!bMirrored ? 25 : 26);
		}
		else if ((joint1 == KinectInterop.JointType.ShoulderRight && joint2 == KinectInterop.JointType.SpineShoulder) ||
			(joint2 == KinectInterop.JointType.ShoulderRight && joint1 == KinectInterop.JointType.SpineShoulder))
		{
			return (!bMirrored ? 26 : 25);
		}
		else if ((joint1 == KinectInterop.JointType.HandTipLeft && joint2 == KinectInterop.JointType.HandLeft) ||
			(joint2 == KinectInterop.JointType.HandTipLeft && joint1 == KinectInterop.JointType.HandLeft))
		{
			return (!bMirrored ? 27 : 28);
		}
		else if ((joint1 == KinectInterop.JointType.HandTipRight && joint2 == KinectInterop.JointType.HandRight) ||
			(joint2 == KinectInterop.JointType.HandTipRight && joint1 == KinectInterop.JointType.HandRight))
		{
			return (!bMirrored ? 28 : 27);
		}
		else if ((joint1 == KinectInterop.JointType.ThumbLeft && joint2 == KinectInterop.JointType.HandLeft) ||
			(joint2 == KinectInterop.JointType.ThumbLeft && joint1 == KinectInterop.JointType.HandLeft))
		{
			return (!bMirrored ? 29 : 30);
		}
		else if ((joint1 == KinectInterop.JointType.ThumbRight && joint2 == KinectInterop.JointType.HandRight) ||
			(joint2 == KinectInterop.JointType.ThumbRight && joint1 == KinectInterop.JointType.HandRight))
		{
			return (!bMirrored ? 30 : 29);
		}

		return boneIndex;
	}


	// transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
	private Transform _transformCache;
	public new Transform transform
	{
		get
		{
			if (!_transformCache) 
			{
				_transformCache = base.transform;
			}

			return _transformCache;
		}
	}


	public void Awake()
	{	
		// check for double start
		if (bones != null)
		{
			return;
		}

		if (!gameObject.activeInHierarchy)
		{
			return;
		}

		// Set model's arms to be in T-pose, if needed
		SetModelArmsInTpose();

		// inits the bones array
		bones = new Transform[31];

		// Initial rotations and directions of the bones.
		initialRotations = new Quaternion[bones.Length];
		inverseInitialRotations = new Quaternion[bones.Length];
		localRotations = new Quaternion[bones.Length];
		isBoneDisabled = new bool[bones.Length];

		// Map bones to the points the Kinect tracks
		MapBones();

		// Get initial bone rotations
		GetInitialRotations();

		// get initial distance to ground
		fFootDistanceInitial = fFootDistance = GetDistanceToGround();
		fFootDistanceTime = 0f;

		// if parent transform uses physics
		isRigidBody = (gameObject.GetComponent<Rigidbody>() != null);
	}

	/// <summary>
	/// Updates the avatar each frame.
	/// </summary>
	/// <param name="UserID">User ID</param>
	public void UpdateAvatar(Int64 UserID)
	{
		if (!gameObject.activeInHierarchy)
		{
			return;
		}

		// Get the KinectManager instance
		if (kinectManager == null)
		{
			kinectManager = KinectManager.Instance;
		}

		// move the avatar to its Kinect position
		if (!externalRootMotion)
		{
			MoveAvatar(UserID);
		}

		// Calculate exaggeration data
		if (_exaggerate)
		{
			CalculateExaggerationData (UserID);
		}

		// get the left hand state and event
		if (kinectManager && kinectManager.GetJointTrackingState(UserID, (int)KinectInterop.JointType.HandLeft) != KinectInterop.TrackingState.NotTracked)
		{
			KinectInterop.HandState leftHandState = kinectManager.GetLeftHandState(UserID);
			InteractionManager.HandEventType leftHandEvent = InteractionManager.HandStateToEvent(leftHandState, lastLeftHandEvent);

			if (leftHandEvent != InteractionManager.HandEventType.None)
			{
				lastLeftHandEvent = leftHandEvent;
			}
		}

		// get the right hand state and event
		if (kinectManager && kinectManager.GetJointTrackingState(UserID, (int)KinectInterop.JointType.HandRight) != KinectInterop.TrackingState.NotTracked)
		{
			KinectInterop.HandState rightHandState = kinectManager.GetRightHandState(UserID);
			InteractionManager.HandEventType rightHandEvent = InteractionManager.HandStateToEvent(rightHandState, lastRightHandEvent);

			if (rightHandEvent != InteractionManager.HandEventType.None)
			{
				lastRightHandEvent = rightHandEvent;
			}
		}

		// rotate the avatar bones
		for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!bones [boneIndex] || isBoneDisabled [boneIndex])
			{
				continue;
			}

			if (boneIndex2JointMap.ContainsKey(boneIndex))
			{
				KinectInterop.JointType joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];
				TransformBone(UserID, joint, boneIndex, !mirroredMovement);
			}
			else if (specIndex2JointMap.ContainsKey(boneIndex))
			{
				// special bones (clavicles)
				List<KinectInterop.JointType> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorMap[boneIndex];

				if (alJoints.Count >= 2)
				{
					Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
					TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
				}
			}
		}

		// Check whether we are kicking and whether we have sent the kick event
		if (OnKick != null)
		{
			ExaggerationData leftLegExaggerationData = null;
			ExaggerationData rightLegExaggerationData = null;

			_exaggerationData.TryGetValue (KinectInterop.JointType.HipLeft, out leftLegExaggerationData);
			_exaggerationData.TryGetValue (KinectInterop.JointType.HipRight, out rightLegExaggerationData);

			if ( (leftLegExaggerationData != null && LegIsKicking (leftLegExaggerationData._exaggerationMultiplier, leftLegExaggerationData._altitude)) ||
			     (rightLegExaggerationData != null && LegIsKicking (rightLegExaggerationData._exaggerationMultiplier, rightLegExaggerationData._altitude)) )
			{
				if (!_kickEventSent && Time.time - _lastKickEventSent > _minKickEventInterval)
				{
					_kickEventSent = true;
					_lastKickEventSent = Time.time;
					OnKick.Invoke ();
				}
			}
			else
			{
				_kickEventSent = false;
			}
		}
	}
	/// <summary>
	/// Resets bones to their initial positions and rotations.
	/// </summary>
	public virtual void ResetToInitialPosition()
	{
		playerId = 0;

		if (bones == null)
		{
			return;
		}

		// For each bone that was defined, reset to initial position.
		transform.rotation = Quaternion.identity;

		for (int pass = 0; pass < 2; pass++)  // 2 passes because clavicles are at the end
		{
			for (int i = 0; i < bones.Length; i++)
			{
				if (bones[i] != null)
				{
					bones[i].rotation = initialRotations[i];
				}
			}
		}

		// reset finger bones to initial position
		Animator animatorComponent = GetComponent<Animator>();
		foreach (HumanBodyBones bone in fingerBoneLocalRotations.Keys)
		{
			Transform boneTransform = animatorComponent ? animatorComponent.GetBoneTransform(bone) : null;

			if (boneTransform)
			{
				boneTransform.localRotation = fingerBoneLocalRotations[bone];
			}
		}

		// Restore the offset's position and rotation
		if (offsetNode != null)
		{
			offsetNode.transform.position = offsetNodePos;
			offsetNode.transform.rotation = offsetNodeRot;
		}

		transform.position = initialPosition;
		transform.rotation = initialRotation;
	}

	/// <summary>
	/// Invoked on the successful calibration of the player.
	/// </summary>
	/// <param name="userId">User identifier.</param>
	public virtual void SuccessfulCalibration(Int64 userId)
	{
		playerId = userId;

		// reset the models position
		if (offsetNode != null)
		{
			offsetNode.transform.position = offsetNodePos;
			offsetNode.transform.rotation = offsetNodeRot;
		}

		transform.position = initialPosition;
		transform.rotation = initialRotation;

		// enable all bones
		for (int i = 0; i < bones.Length; i++)
		{
			isBoneDisabled[i] = false;
		}

		// re-calibrate the position offset
		offsetCalibrated = false;
	}

	#region Exaggeration

	public enum Leg
	{
		NONE,
		LEFT_LEG,
		RIGHT_LEG
	}

	protected enum ExaggerationRotationType
	{
		ROTATE_BY_ANGLE,
		ROTATE_TO_ANGLE
	}

	protected class LegJoints
	{
		public KinectInterop.JointType hip;
		public KinectInterop.JointType knee;
		public KinectInterop.JointType ankle;

		public LegJoints(KinectInterop.JointType hipJoint, KinectInterop.JointType kneeJoint, KinectInterop.JointType ankleJoint)
		{
			hip = hipJoint;
			knee = kneeJoint;
			ankle = ankleJoint;
		}
	}

	protected class ExaggerationData
	{
		public ExaggerationRotationType _type;
		public Vector3 					_exaggerationRotationAxis;
		public float 					_exaggerationAngle;
		public Quaternion				_rotation;

		public float 					_exaggerationMultiplier;
		public float 					_altitude;

		public Quaternion Rotation
		{
			get
			{
				return _rotation;
			}
		}

		public ExaggerationData(ExaggerationRotationType type, Vector3 exaggerationRotationAxis, float exaggerationAngle, float altitude, float exaggerationMultiplier)
		{
			_type = type;
			_exaggerationRotationAxis = exaggerationRotationAxis;
			_exaggerationAngle = exaggerationAngle;
			_rotation = Quaternion.AngleAxis(_exaggerationAngle, _exaggerationRotationAxis);

			_altitude = altitude;
			_exaggerationMultiplier = exaggerationMultiplier;
		}

		public ExaggerationData(ExaggerationData other)
		{
			_type = other._type;
			_exaggerationRotationAxis = other._exaggerationRotationAxis;
			_exaggerationAngle = other._exaggerationAngle;
			_rotation = other._rotation;

			_altitude = other._altitude;
			_exaggerationMultiplier = other._exaggerationMultiplier;
		}
	}

	[Header("Exaggeration options")]

	[Tooltip("Enables exaggeration if selected.")]
	[SerializeField]
	protected bool _exaggerate = false;

	[SerializeField]
	[Tooltip("Curve that describes the amount of exaggeration wrt. to the kicking angle [0.0, 180.0] deg.")]
	protected AnimationCurve _exaggerationCurve;

	[SerializeField]
	[Tooltip("A ratio that controls the the amount of the exaggeration between the kicking leg and other joints between [0.0, 1.0].")]
	[Range(0.0f, 1.0f)]
	protected float _exaggerationDivisionRatio = 0.3f;

	[SerializeField]
	[Tooltip("Multiplier that adjusts the leg exaggeration effects.")]
	[Range(1.0f, 5.0f)]
	protected float _legExaggerationMultiplier = 1.0f;

	[SerializeField]
	[Tooltip("Multiplier that adjusts the upper body exaggeration effects.")]
	[Range(1.0f, 5.0f)]
	protected float _upperBodyExaggerationMultiplier = 1.0f;

	[SerializeField]
	[Tooltip("An angle threshold for a limit that is considered kicking, used in conjunction with the exaggeration curve to determine if we are currently kicking.")]
	protected float _kickThreshold = 40.0f;

	[SerializeField]
	protected CubemanController _referenceSkeleton;

	[Header("Debug options")]
	[SerializeField]
	protected bool _showReferenceSkeleton;
	[SerializeField]
	protected Text _rightLegDebugText;
	[SerializeField]
	protected Text _leftLegDebugText;
	[SerializeField]
	protected bool _showRightLegDebug;
	[SerializeField]
	protected bool _showLeftLegDebug;
	[SerializeField]
	protected bool _showKickTrails;
	[SerializeField]
	protected TrailRenderer[] _kickTrails;

	protected Dictionary<KinectInterop.JointType, ExaggerationData> _exaggerationData = new Dictionary<KinectInterop.JointType, ExaggerationData> ();

	public event Kick OnKick;
	protected bool _kickEventSent = false;
	protected float _lastKickEventSent = 0.0f;
	protected float _minKickEventInterval = 0.3f;

	protected Vector3 BaseUp
	{
		get
		{
			return Vector3.up;
		}
	}

	protected Vector3 BaseDown
	{
		get
		{
			return -1.0f * BaseUp;
		}
	}

	protected Vector3 BaseForward
	{
		get
		{
			return inverseInitialRotations[0] * bones[0].forward;
		}
	}

	// Normalizes the angle to range [0, 360]
	protected float NormalizeAngle360(float angle)
	{
		float temp = angle;

		if (temp < 0.0f)
		{
			temp += 360.0f;
		}
		else if (temp > 360.0f)
		{
			temp -= 360.0f;
		}

		return temp;
	}

	// Normalizes the angle to range [-180, 180]
	protected float NormalizeAngle180(float angle)
	{
		float temp = angle;

		if (temp < -180.0f)
		{
			temp += 180.0f;
		}
		else if (temp > 180.0f)
		{
			temp -= 180.0f;
		}

		return temp;
	}

	protected Leg GetLeg(KinectInterop.JointType legJoint)
	{
		switch (legJoint)
		{
			case KinectInterop.JointType.HipLeft:
			case KinectInterop.JointType.KneeLeft:
			case KinectInterop.JointType.AnkleLeft:
				return Leg.LEFT_LEG;
			case KinectInterop.JointType.HipRight:
			case KinectInterop.JointType.KneeRight:
			case KinectInterop.JointType.AnkleRight:
				return Leg.RIGHT_LEG;
			default:
				return Leg.NONE;
		}
	}

	protected LegJoints GetLegJoints(Leg leg)
	{
		switch (leg)
		{
			case Leg.LEFT_LEG:
				return new LegJoints (KinectInterop.JointType.HipLeft, KinectInterop.JointType.KneeLeft, KinectInterop.JointType.AnkleLeft);
			case Leg.RIGHT_LEG:
				return new LegJoints (KinectInterop.JointType.HipRight, KinectInterop.JointType.KneeRight, KinectInterop.JointType.AnkleRight);
			default:
				Debug.LogWarningFormat ("AvatarController GetLegJoints: Invalid leg enum {0}", leg);
				return null;
		}
	}

	protected Vector3 CalculateLegDirection(Leg leg)
	{
		switch (leg)
		{
			case Leg.LEFT_LEG:
				return _referenceSkeleton.LeftLegDirection;
			case Leg.RIGHT_LEG:
				return _referenceSkeleton.RightLegDirection;
			default:
				Debug.LogWarningFormat ("AvatarController CalculateLegDirection: Invalid leg enum {0}", leg);
				return Vector3.zero;
		}
	}

	protected float CalculateLegAzimuth(Vector3 legDir)
	{
		// Determine the leg angle wrt. character's forward vector to determine kick azimuth (deg)
		// The azimuth is defined in range [0, 360] deg
		// Project the leg and forward directions to xz-plane
		Vector3 forward = BaseForward;
		return Vector2.Angle(new Vector2(forward.x, forward.z), new Vector2(legDir.x, legDir.z));
	}

	protected float CalculateLegAltitude(Vector3 legDir)
	{
		// Determine the leg angle wrt. global down vector to determine kick altitude (deg)
		// The altitude is defined in range [0, 360] deg
		float legAltitude = NormalizeAngle360(Vector3.Angle(BaseDown, legDir));
		return legAltitude;
	}

	protected ExaggerationData GetExaggerationData(KinectInterop.JointType joint)
	{
		ExaggerationData data = null;
		bool found = _exaggerationData.TryGetValue(joint, out data);
		return found ? data : null;
	}

	protected float CalculateExaggerationMultiplier(float legAltitude)
	{
		if (legAltitude < 0.0f || legAltitude > 180.0f)
		{
			Debug.LogWarning(string.Format("AvatarController CalculateMultipliers: Leg angle should be between 0 - 180 degrees, got {0} deg", 180.0f));
			return 1.0f;
		}

		float multiplier = _exaggerationCurve.Evaluate(legAltitude/180.0f);
		return Mathf.Max(1.0f, multiplier);
	}
		
	protected Vector3 CalculateExaggerationAxis(Vector3 legDirection)
	{
		// Project to xz-plane
		Vector3 legDir = legDirection;
		legDir.y = 0.0f;

		// Rotate 90 degrees clockwise from the leg direction to get the exaggeration axis
		Vector3 axis = Quaternion.AngleAxis(-90.0f, BaseUp) * legDir;
		return axis;
	}

	protected bool LegIsKicking(float legMult, float legAltitude)
	{
		return legMult > 1.0f || legAltitude > _kickThreshold; 
	}

	protected ExaggerationData CalculateLegExaggerationData(Int64 userId, Leg leg)
	{
		Vector3 legDirection = CalculateLegDirection(leg);
		float legAltitude = CalculateLegAltitude(legDirection);
		float legAzimuth = CalculateLegAzimuth(legDirection);
		float exaggerationMultiplier = CalculateExaggerationMultiplier(legAltitude);
		Vector3 legExaggerationAxis = CalculateExaggerationAxis(legDirection);
		float mult = 1.0f + ((exaggerationMultiplier - 1.0f) * (1.0f - _exaggerationDivisionRatio));
		float legDeltaAngle = _legExaggerationMultiplier * ((legAltitude * mult) - legAltitude);
		ExaggerationData legExaggerationData = new ExaggerationData(ExaggerationRotationType.ROTATE_BY_ANGLE, legExaggerationAxis, 1.0f * legDeltaAngle, legAltitude, exaggerationMultiplier);

		// DEBUG
		_leftLegDebugText.enabled = _showLeftLegDebug;
		_rightLegDebugText.enabled = _showRightLegDebug;

		for (int i = 0; i < _kickTrails.Length; ++i)
		{
			if (_kickTrails [i] != null)
			{
				_kickTrails [i].enabled = _showKickTrails;
			}
		}

		if (_showLeftLegDebug || _showRightLegDebug)
		{
			if (leg == Leg.LEFT_LEG && _showLeftLegDebug)
			{
				_leftLegDebugText.text = string.Format("Left leg azimuth: {0}\nLeft leg altitude: {1}", legAzimuth, legAltitude);
				Vector3 leftHipPosition = bones[GetBoneIndexByJoint (KinectInterop.JointType.HipLeft, mirroredMovement)].position;
				Debug.DrawRay(leftHipPosition, legDirection, Color.green);
				Debug.DrawRay(leftHipPosition, legExaggerationAxis, Color.red);
				Debug.DrawRay(leftHipPosition, BaseForward, Color.black);
				Debug.DrawRay(leftHipPosition, BaseUp, Color.white);
			}
			else if (leg == Leg.RIGHT_LEG && _showRightLegDebug)
			{
				_rightLegDebugText.text = string.Format("Right leg azimuth: {0}\nRight leg altitude: {1}", legAzimuth, legAltitude);
				Vector3 rightHipPosition = bones[GetBoneIndexByJoint(KinectInterop.JointType.HipRight, mirroredMovement)].position;
				Debug.DrawRay(rightHipPosition, legDirection, Color.blue);
				Debug.DrawRay(rightHipPosition, legExaggerationAxis, Color.yellow);
				Debug.DrawRay(rightHipPosition, BaseForward, Color.black);
				Debug.DrawRay(rightHipPosition, BaseUp, Color.white);
			}
		}
		// END OF DEBUG

		// This part is only here to always enable debug to be visible - even when not kicking
		// If the leg is not kicking no need to calculate the exaggeration axis or store exaggeration data
		if (!LegIsKicking (exaggerationMultiplier, legAltitude))
		{
			return null;
		}

		return legExaggerationData;
	}

	protected ExaggerationData CalculateUpperBodyExaggerationData(ExaggerationData leftLegExaggerationData, ExaggerationData rightLegExaggerationData)
	{
		// If both legs are kicking or neither is kicking, return null i.e. don't exaggerate
		ExaggerationData upperBodyExaggerationData = null;

		// Left kick
		if (leftLegExaggerationData != null && rightLegExaggerationData == null)
		{
			float legDeltaAngle = (leftLegExaggerationData._altitude * leftLegExaggerationData._exaggerationMultiplier) - leftLegExaggerationData._altitude;
			float exaggerationAngle = _upperBodyExaggerationMultiplier * legDeltaAngle * _exaggerationDivisionRatio;
			upperBodyExaggerationData = new ExaggerationData (ExaggerationRotationType.ROTATE_BY_ANGLE, leftLegExaggerationData._exaggerationRotationAxis, 1.0f * exaggerationAngle, leftLegExaggerationData._altitude, leftLegExaggerationData._exaggerationMultiplier);
		}
		// Right kick
		else if (rightLegExaggerationData != null && leftLegExaggerationData == null)
		{
			float legDeltaAngle = (rightLegExaggerationData._altitude * rightLegExaggerationData._exaggerationMultiplier) - rightLegExaggerationData._altitude;
			float exaggerationAngle = _upperBodyExaggerationMultiplier * legDeltaAngle * _exaggerationDivisionRatio;
			upperBodyExaggerationData = new ExaggerationData (ExaggerationRotationType.ROTATE_BY_ANGLE, rightLegExaggerationData._exaggerationRotationAxis, 1.0f * exaggerationAngle, rightLegExaggerationData._altitude, rightLegExaggerationData._exaggerationMultiplier);
		}

		return upperBodyExaggerationData;
	}

	protected void CalculateExaggerationData(Int64 userId)
	{
		ExaggerationData leftLegExaggerationData = CalculateLegExaggerationData(userId, Leg.LEFT_LEG);
		ExaggerationData rightLegExaggerationData = CalculateLegExaggerationData(userId, Leg.RIGHT_LEG);
		ExaggerationData upperBodyExaggerationData = CalculateUpperBodyExaggerationData(leftLegExaggerationData, rightLegExaggerationData);

		// Left leg
		_exaggerationData [KinectInterop.JointType.HipLeft] = leftLegExaggerationData;
		_exaggerationData [KinectInterop.JointType.KneeLeft] = leftLegExaggerationData;

		// Right leg
		_exaggerationData[KinectInterop.JointType.HipRight] = rightLegExaggerationData;
		_exaggerationData [KinectInterop.JointType.KneeRight] = rightLegExaggerationData;

		// Head
		//_exaggerationData[KinectInterop.JointType.Neck] = upperBodyExaggerationData;
		//_exaggerationData[KinectInterop.JointType.Head] = upperBodyExaggerationData;

		// Middle body
		_exaggerationData[KinectInterop.JointType.SpineBase] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.SpineMid] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.SpineShoulder] = upperBodyExaggerationData;

		// Left arm
		_exaggerationData[KinectInterop.JointType.ShoulderLeft] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.ElbowLeft] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.WristLeft] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.HandLeft] = upperBodyExaggerationData;

		// Right arm
		_exaggerationData[KinectInterop.JointType.ShoulderRight] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.ElbowRight] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.WristRight] = upperBodyExaggerationData;
		_exaggerationData[KinectInterop.JointType.HandRight] = upperBodyExaggerationData;
	}

	protected Quaternion ExaggerateBoneRotation(Int64 userId, KinectInterop.JointType joint, Quaternion unexaggeratedRotation)
	{
		ExaggerationData data = GetExaggerationData(joint);

		if (data == null || data._exaggerationMultiplier == 1.0f)
		{
			return unexaggeratedRotation;
		}

		// Rotate clockwise around the exaggeration axis
		switch (data._type)
		{
			case ExaggerationRotationType.ROTATE_TO_ANGLE:
				return data.Rotation;
			case ExaggerationRotationType.ROTATE_BY_ANGLE:
				return data.Rotation * unexaggeratedRotation;
			default:
				Debug.LogWarningFormat ("AvatarController ExaggerateBoneRotation: Unknown exaggeration rotation type {0}", data._type);
				return unexaggeratedRotation;
		}
	}

	// Apply the rotations tracked by kinect to the joints.
	protected void TransformBone(Int64 userId, KinectInterop.JointType joint, int boneIndex, bool flip)
	{
		Transform boneTransform = bones[boneIndex];

		if (boneTransform == null || kinectManager == null)
		{
			return;
		}

		int iJoint = (int)joint;

		if (iJoint < 0 || !kinectManager.IsJointTracked(userId, iJoint))
		{
			return;
		}

		// Get Kinect joint orientation
		Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);

		if (jointRotation == Quaternion.identity)
		{
			return;
		}

		Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

		if (externalRootMotion)
		{
			newRotation = transform.rotation * newRotation;
		}

		if (_exaggerate)
		{
			newRotation = ExaggerateBoneRotation (userId, joint, newRotation);
		}

		// Smoothly transition to the new rotation
		if (smoothFactor != 0f)
		{
			boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
		}
		else
		{
			boneTransform.rotation = newRotation;
		}
	}

	private void Update()
	{
		_referenceSkeleton.Visible = _showReferenceSkeleton;
	}

    #endregion

    #if VIVE
    #region Vive integration

    [Header("Strech options")]

	[SerializeField]
	protected float 					_stretchRate;

	[Header("Vive options")]

	[SerializeField]
	protected SteamVR_TrackedObject		_leftController;
	[SerializeField]
	protected SteamVR_TrackedObject 	_rightController;

	private SteamVR_Controller.Device	_rightControllerDevice;
	private SteamVR_Controller.Device	_leftControllerDevice;

	private bool					 	_leftControllerNullWarningGiven = false;
	private bool 						_rightControllerNullWarningGiven = false;

	protected SteamVR_Controller.Device RightController
	{
		get
		{
			if (_rightController == null)
			{
				return null;
			}

			if (_rightControllerDevice == null)
			{
				int index = (int)_rightController.index;

				if (index >= 0 && index < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount)
				{
					_rightControllerDevice = SteamVR_Controller.Input ((int)_rightController.index);
				}
			}

			return _rightControllerDevice;
		}
	}

	protected SteamVR_Controller.Device LeftController
	{
		get
		{
			if (_leftController == null)
			{
				return null;
			}

			if (_leftControllerDevice == null)
			{
				int index = (int)_leftController.index;

				if (index >= 0 && index < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount)
				{
					_leftControllerDevice = SteamVR_Controller.Input ((int)_leftController.index);
				}
			}

			return _leftControllerDevice;
		}
	}

	private void Stretch(SteamVR_Controller.Device controllerDevice, KinectInterop.JointType joint, ref bool controllerWarning)
	{
		if (controllerDevice != null)
		{
			controllerWarning = false;
			int boneIdx = !mirroredMovement ? jointMap2boneIndex[joint] : mirrorJointMap2boneIndex[joint];
			Transform boneTransform = bones[boneIdx];

			if (boneTransform == null)
			{
				Debug.LogWarning("AvatarController Stretch: boneTransform is null");
				return;
			}

			Vector3 boneLocalScale = boneTransform.localScale;

			if (controllerDevice.GetPress (Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger))
			{
				boneLocalScale.y += _stretchRate * Time.fixedDeltaTime;
				boneTransform.localScale = boneLocalScale;
			}
			else
			{
				if (boneLocalScale.y > 1.0f)
				{
					boneLocalScale.y = Mathf.Max(1.0f, boneLocalScale.y - _stretchRate * Time.fixedDeltaTime);
					boneTransform.localScale = boneLocalScale;
				}
			}
		}
		else
		{
			if (!controllerWarning)
			{
				controllerWarning = true;
				Debug.LogWarning ("AvatarController Stretch: controllerDevice is null");
			}
		}		
	}

	private void FixedUpdate()
	{
		//Stretch (LeftController, KinectInterop.JointType.HipLeft, ref _leftControllerNullWarningGiven);
		//Stretch (RightController, KinectInterop.JointType.HipRight, ref _rightControllerNullWarningGiven);
	}

    #endregion
    #endif

    // Apply the rotations tracked by kinect to a special joint
    protected void TransformSpecialBone(Int64 userId, KinectInterop.JointType joint, KinectInterop.JointType jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
		Transform boneTransform = bones[boneIndex];

		if (boneTransform == null || kinectManager == null)
		{
			return;
		}

		if (!kinectManager.IsJointTracked(userId, (int)joint) || !kinectManager.IsJointTracked(userId, (int)jointParent))
		{
			return;
		}

		if (boneIndex >= 27 && boneIndex <= 30)
		{
			// fingers or thumbs
			if (fingerOrientations)
			{
				TransformSpecialBoneFingers(userId, (int)joint, boneIndex, flip);
			}

			return;
		}

		Vector3 jointDir = kinectManager.GetJointDirection(userId, (int)joint, false, true);
		Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

		if (!flip)
		{
			Vector3 mirroredAngles = jointRotation.eulerAngles;
			mirroredAngles.y = -mirroredAngles.y;
			mirroredAngles.z = -mirroredAngles.z;

			jointRotation = Quaternion.Euler(mirroredAngles);
		}

		if (jointRotation != Quaternion.identity)
		{
			// Smoothly transition to the new rotation
			Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

			if (externalRootMotion)
			{
				newRotation = transform.rotation * newRotation;
			}

			if (smoothFactor != 0f)
			{
				boneTransform.rotation = Quaternion.Slerp (boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			}
			else
			{
				boneTransform.rotation = newRotation;
			}
		}

	}

	// Apply the rotations tracked by kinect to fingers (one joint = multiple bones)
	protected void TransformSpecialBoneFingers(Int64 userId, int joint, int boneIndex, bool flip)
	{
		// check for hand grips
		if (joint == (int)KinectInterop.JointType.HandTipLeft || joint == (int)KinectInterop.JointType.ThumbLeft)
		{
			if (lastLeftHandEvent == InteractionManager.HandEventType.Grip)
			{
				if (!bLeftFistDone)
				{
					float angleSign = !mirroredMovement /**(boneIndex == 27 || boneIndex == 29)*/ ? -1f : -1f;
					float angleRot = angleSign * 60f;

					TransformSpecialBoneFist(boneIndex, angleRot);
					bLeftFistDone = (boneIndex >= 29);
				}

				return;
			}
			else if(bLeftFistDone && lastLeftHandEvent == InteractionManager.HandEventType.Release)
			{
				TransformSpecialBoneUnfist(boneIndex);
				bLeftFistDone = !(boneIndex >= 29);
			}
		}
		else if (joint == (int)KinectInterop.JointType.HandTipRight || joint == (int)KinectInterop.JointType.ThumbRight)
		{
			if (lastRightHandEvent == InteractionManager.HandEventType.Grip)
			{
				if (!bRightFistDone)
				{
					float angleSign = !mirroredMovement /**(boneIndex == 27 || boneIndex == 29)*/ ? -1f : -1f;
					float angleRot = angleSign * 60f;

					TransformSpecialBoneFist(boneIndex, angleRot);
					bRightFistDone = (boneIndex >= 29);
				}

				return;
			}
			else if (bRightFistDone && lastRightHandEvent == InteractionManager.HandEventType.Release)
			{
				TransformSpecialBoneUnfist(boneIndex);
				bRightFistDone = !(boneIndex >= 29);
			}
		}

		// get the animator component
		Animator animatorComponent = GetComponent<Animator>();

		if (!animatorComponent)
		{
			return;
		}

		// Get Kinect joint orientation
		Quaternion jointRotation = kinectManager.GetJointOrientation(userId, joint, flip);

		if (jointRotation == Quaternion.identity)
		{
			return;
		}

		// calculate the new orientation
		Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

		if (externalRootMotion)
		{
			newRotation = transform.rotation * newRotation;
		}

		// get the list of bones
		//List<HumanBodyBones> alBones = flip ? specialIndex2MultiBoneMap[boneIndex] : specialIndex2MirrorBoneMap[boneIndex];
		List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];

		// Smoothly transition to the new rotation
		for (int i = 0; i < alBones.Count; i++)
		{
			Transform boneTransform = animatorComponent.GetBoneTransform(alBones[i]);

			if (!boneTransform)
			{
				continue;
			}

			if (smoothFactor != 0f)
			{
				boneTransform.rotation = Quaternion.Slerp (boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			}
			else
			{
				boneTransform.rotation = newRotation;
			}
		}
	}

	// Apply the rotations needed to transform fingers to fist
	protected void TransformSpecialBoneFist(int boneIndex, float angle)
	{
		// get the animator component
		Animator animatorComponent = GetComponent<Animator>();
		if(!animatorComponent)
			return;

		// get the list of bones
		List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];

		for(int i = 0; i < alBones.Count; i++)
		{
			if(i < 2 && (boneIndex == 29 || boneIndex == 30))  // skip the first two thumb bones
				continue;

			HumanBodyBones bone = alBones[i];
			Transform boneTransform = animatorComponent.GetBoneTransform(bone);

			// set the fist rotation
			if(boneTransform)
			{
				Quaternion qRotFinger = Quaternion.AngleAxis(angle, fingerBoneLocalAxes[bone]);
				boneTransform.localRotation = fingerBoneLocalRotations[bone] * qRotFinger;
			}
		}

	}

	// Apply the initial rotations fingers
	protected void TransformSpecialBoneUnfist(int boneIndex)
	{
		//		// do fist only for fingers
		//		if(boneIndex != 27 && boneIndex != 28)
		//			return;

		// get the animator component
		Animator animatorComponent = GetComponent<Animator>();
		if(!animatorComponent)
			return;

		// get the list of bones
		List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];

		for(int i = 0; i < alBones.Count; i++)
		{
			HumanBodyBones bone = alBones[i];
			Transform boneTransform = animatorComponent.GetBoneTransform(bone);

			// set the initial rotation
			if(boneTransform)
			{
				boneTransform.localRotation = fingerBoneLocalRotations[bone];
			}
		}
	}

	// Moves the avatar - gets the tracked position of the user and applies it to avatar.
	protected void MoveAvatar(Int64 UserID)
	{
		if ((moveRate == 0f) || !kinectManager ||
			!kinectManager.IsJointTracked(UserID, (int)KinectInterop.JointType.SpineBase))
		{
			return;
		}

		// get the position of user's spine base
		Vector3 trans = kinectManager.GetUserPosition(UserID);

		// use the color overlay position if needed
		if (posRelativeToCamera && posRelOverlayColor)
		{
			Rect backgroundRect = posRelativeToCamera.pixelRect;
			PortraitBackground portraitBack = PortraitBackground.Instance;

			if(portraitBack && portraitBack.enabled)
			{
				backgroundRect = portraitBack.GetBackgroundRect();
			}

			trans = kinectManager.GetJointPosColorOverlay(UserID, (int)KinectInterop.JointType.SpineBase, posRelativeToCamera, backgroundRect);
		}

		// invert the z-coordinate, if needed
		if (posRelativeToCamera && posRelInvertedZ)
		{
			trans.z = -trans.z;
		}

		if (!offsetCalibrated)
		{
			offsetCalibrated = true;

			offsetPos.x = trans.x;  // !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			offsetPos.y = trans.y;  // trans.y * moveRate;
			offsetPos.z = !mirroredMovement && !posRelativeToCamera ? -trans.z : trans.z;  // -trans.z * moveRate;

			if (posRelativeToCamera)
			{
				Vector3 cameraPos = posRelativeToCamera.transform.position;
				Vector3 bodyRootPos = bodyRoot != null ? bodyRoot.position : transform.position;
				Vector3 hipCenterPos = bodyRoot != null ? bodyRoot.position : (bones != null && bones.Length > 0 && bones[0] != null ? bones[0].position : Vector3.zero);

				float yRelToAvatar = 0f;
				if(verticalMovement)
				{
					yRelToAvatar = (trans.y - cameraPos.y) - (hipCenterPos - bodyRootPos).magnitude;
				}
				else
				{
					yRelToAvatar = bodyRootPos.y - cameraPos.y;
				}

				Vector3 relativePos = new Vector3(trans.x, yRelToAvatar, trans.z);
				Vector3 newBodyRootPos = cameraPos + relativePos;

				if (bodyRoot != null)
				{
					bodyRoot.position = newBodyRootPos;
				}
				else
				{
					transform.position = newBodyRootPos;
				}

				bodyRootPosition = newBodyRootPos;
			}
		}

		// transition to the new position
		Vector3 targetPos = bodyRootPosition + Kinect2AvatarPos(trans, verticalMovement);

		if (isRigidBody && !verticalMovement)
		{
			// workaround for obeying the physics (e.g. gravity falling)
			targetPos.y = bodyRoot != null ? bodyRoot.position.y : transform.position.y;
		}

		if (verticalMovement && verticalOffset != 0f && 
			bones[0] != null && bones[3] != null) 
		{
			Vector3 dirSpine = bones[3].position - bones[0].position;
			targetPos += dirSpine.normalized * verticalOffset;
		}

		if (groundedFeet)
		{
			targetPos.y += (fFootDistanceInitial - fFootDistance);

			float fNewDistance = GetDistanceToGround();
			float fNewDistanceTime = Time.time;

			if (fNewDistance == 0f)
			{
				fNewDistance = fFootDistanceInitial;
			}

			if (Mathf.Abs(fNewDistance - fFootDistanceInitial) >= maxFootDistanceGround)
			{
				if ((fNewDistanceTime - fFootDistanceTime) >= maxFootDistanceTime)
				{
					fFootDistance += (fNewDistance - fFootDistanceInitial);
					fFootDistanceTime = fNewDistanceTime;
				}
			}
			else
			{
				fFootDistanceTime = fNewDistanceTime;
			}
		}

		if(bodyRoot != null)
		{
			bodyRoot.position = smoothFactor != 0f ? 
				Vector3.Lerp(bodyRoot.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
		}
		else
		{
			transform.position = smoothFactor != 0f ? 
				Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.deltaTime) : targetPos;
		}
	}

	// Set model's arms to be in T-pose
	protected void SetModelArmsInTpose()
	{
		Vector3 vTposeLeftDir = transform.TransformDirection(Vector3.left);
		Vector3 vTposeRightDir = transform.TransformDirection(Vector3.right);
		Animator animator = GetComponent<Animator>();

		Transform transLeftUarm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
		Transform transLeftLarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
		Transform transLeftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

		if (transLeftUarm != null && transLeftLarm != null)
		{
			Vector3 vUarmLeftDir = transLeftLarm.position - transLeftUarm.position;
			float fUarmLeftAngle = Vector3.Angle(vUarmLeftDir, vTposeLeftDir);

			if (Mathf.Abs(fUarmLeftAngle) >= 5f)
			{
				Quaternion vFixRotation = Quaternion.FromToRotation(vUarmLeftDir, vTposeLeftDir);
				transLeftUarm.rotation = vFixRotation * transLeftUarm.rotation;
			}

			if (transLeftHand != null)
			{
				Vector3 vLarmLeftDir = transLeftHand.position - transLeftLarm.position;
				float fLarmLeftAngle = Vector3.Angle(vLarmLeftDir, vTposeLeftDir);

				if (Mathf.Abs(fLarmLeftAngle) >= 5f)
				{
					Quaternion vFixRotation = Quaternion.FromToRotation(vLarmLeftDir, vTposeLeftDir);
					transLeftLarm.rotation = vFixRotation * transLeftLarm.rotation;
				}
			}
		}

		Transform transRightUarm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
		Transform transRightLarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
		Transform transRightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

		if (transRightUarm != null && transRightLarm != null)
		{
			Vector3 vUarmRightDir = transRightLarm.position - transRightUarm.position;
			float fUarmRightAngle = Vector3.Angle(vUarmRightDir, vTposeRightDir);

			if (Mathf.Abs(fUarmRightAngle) >= 5f)
			{
				Quaternion vFixRotation = Quaternion.FromToRotation(vUarmRightDir, vTposeRightDir);
				transRightUarm.rotation = vFixRotation * transRightUarm.rotation;
			}

			if (transRightHand != null)
			{
				Vector3 vLarmRightDir = transRightHand.position - transRightLarm.position;
				float fLarmRightAngle = Vector3.Angle(vLarmRightDir, vTposeRightDir);

				if (Mathf.Abs(fLarmRightAngle) >= 5f)
				{
					Quaternion vFixRotation = Quaternion.FromToRotation(vLarmRightDir, vTposeRightDir);
					transRightLarm.rotation = vFixRotation * transRightLarm.rotation;
				}
			}
		}

	}

	// If the bones to be mapped have been declared, map that bone to the model.
	protected virtual void MapBones()
	{
		// get bone transforms from the animator component
		Animator animatorComponent = GetComponent<Animator>();

		for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!boneIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;

			bones[boneIndex] = animatorComponent ? animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]) : null;
		}
	}

	// Capture the initial rotations of the bones
	protected void GetInitialRotations()
	{
		// save the initial rotation
		if (offsetNode != null)
		{
			offsetNodePos = offsetNode.transform.position;
			offsetNodeRot = offsetNode.transform.rotation;
		}

		initialPosition = transform.position;
		initialRotation = transform.rotation;
		inverseInitialRotation = Quaternion.Inverse(transform.rotation);
		transform.rotation = Quaternion.identity;

		// save the body root initial position
		if (bodyRoot != null)
		{
			bodyRootPosition = bodyRoot.position;
		}
		else
		{
			bodyRootPosition = transform.position;
		}

		if (offsetNode != null)
		{
			bodyRootPosition = bodyRootPosition - offsetNodePos;
		}

		// save the initial bone rotations
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation;
				inverseInitialRotations[i] = Quaternion.Inverse(bones[i].rotation);
				localRotations[i] = bones[i].localRotation;
			}
		}

		// get finger bones' local rotations
		Animator animatorComponent = GetComponent<Animator>();
		foreach (int boneIndex in specialIndex2MultiBoneMap.Keys)
		{
			List<HumanBodyBones> alBones = specialIndex2MultiBoneMap[boneIndex];
			Transform handTransform = animatorComponent.GetBoneTransform((boneIndex == 27 || boneIndex == 29) ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);

			for (int b = 0; b < alBones.Count; b++)
			{
				HumanBodyBones bone = alBones[b];
				Transform boneTransform = animatorComponent ? animatorComponent.GetBoneTransform(bone) : null;

				if (boneTransform)
				{
					fingerBoneLocalRotations[bone] = boneTransform.localRotation;

					Transform bparTransform = boneTransform ? boneTransform.parent : null;
					Transform bchildTransform = boneTransform && boneTransform.childCount > 0 ? boneTransform.GetChild(0) : null;

					// get the finger base transform (1st joint)
					Transform fingerBaseTransform = animatorComponent.GetBoneTransform(alBones[b - (b % 3)]);
					Vector3 vBoneDir2 = (handTransform.position - fingerBaseTransform.position).normalized;

					// set the fist rotation
					if (boneTransform && fingerBaseTransform && handTransform)
					{
						Vector3 vBoneDir = bchildTransform ? (bchildTransform.position - boneTransform.position).normalized :
							(bparTransform ? (boneTransform.position - bparTransform.position).normalized : Vector3.zero);

						Vector3 vOrthoDir = Vector3.Cross(vBoneDir2, vBoneDir).normalized;
						fingerBoneLocalAxes[bone] = boneTransform.InverseTransformDirection(vOrthoDir);
					}
				}
			}
		}

		// Restore the initial rotation
		transform.rotation = initialRotation;
	}

	// Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
	protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
		Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		newRotation = initialRotation * newRotation;
		return newRotation;
	}

	// Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
		float xPos = (jointPosition.x - offsetPos.x) * moveRate;
		float yPos = (jointPosition.y - offsetPos.y) * moveRate;
		float zPos = !mirroredMovement && !posRelativeToCamera ? (-jointPosition.z - offsetPos.z) * moveRate : (jointPosition.z - offsetPos.z) * moveRate;

		Vector3 newPosition = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);

		Quaternion posRotation = mirroredMovement ? Quaternion.Euler (0f, 180f, 0f) * initialRotation : initialRotation;
		newPosition = posRotation * newPosition;

		if (offsetNode != null)
		{
			//newPosition += offsetNode.transform.position;
			newPosition = offsetNode.transform.position;
		}

		return newPosition;
	}

	// returns distance from the given transform to the underlying object. The player must be in IgnoreRaycast layer.
	protected float GetTransformDistanceToGround(Transform trans)
	{
		if (!trans)
		{
			return 0f;
		}

		RaycastHit hit;

		if (Physics.Raycast(trans.position, Vector3.down, out hit, 2f, raycastLayers))
		{
			return hit.distance;
		}
		else if (Physics.Raycast(trans.position, Vector3.up, out hit, 2f, raycastLayers))
		{
			return -hit.distance;
		}

		return 1000f;
	}

	// returns the lower distance distance from left or right foot to the ground, or 1000f if no LF/RF transforms are found
	protected float GetDistanceToGround()
	{
		if (leftFoot == null && rightFoot == null)
		{
			Animator animatorComponent = GetComponent<Animator>();

			if(animatorComponent)
			{
				leftFoot = animatorComponent.GetBoneTransform(HumanBodyBones.LeftToes);
				rightFoot = animatorComponent.GetBoneTransform(HumanBodyBones.RightToes);
			}
		}

		float fDistMin = 1000f;
		float fDistLeft = leftFoot ? GetTransformDistanceToGround(leftFoot) : fDistMin;
		float fDistRight = rightFoot ? GetTransformDistanceToGround(rightFoot) : fDistMin;
		fDistMin = Mathf.Abs(fDistLeft) < Mathf.Abs(fDistRight) ? fDistLeft : fDistRight;

		if(fDistMin == 1000f)
		{
			fDistMin = fFootDistanceInitial;
		}

		return fDistMin;
	}

	// dictionaries to speed up bones' processing
	// the author of the terrific idea for kinect-joints to mecanim-bones mapping
	// along with its initial implementation, including following dictionary is
	// Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
	protected static readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
		//        {2, HumanBodyBones.Chest},
		{3, HumanBodyBones.Neck},
		//		{4, HumanBodyBones.Head},

		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
		//		{8, HumanBodyBones.LeftIndexProximal},
		//		{9, HumanBodyBones.LeftIndexIntermediate},
		//		{10, HumanBodyBones.LeftThumbProximal},

		{11, HumanBodyBones.RightUpperArm},
		{12, HumanBodyBones.RightLowerArm},
		{13, HumanBodyBones.RightHand},
		//		{14, HumanBodyBones.RightIndexProximal},
		//		{15, HumanBodyBones.RightIndexIntermediate},
		//		{16, HumanBodyBones.RightThumbProximal},

		{17, HumanBodyBones.LeftUpperLeg},
		{18, HumanBodyBones.LeftLowerLeg},
		{19, HumanBodyBones.LeftFoot},
		//		{20, HumanBodyBones.LeftToes},

		{21, HumanBodyBones.RightUpperLeg},
		{22, HumanBodyBones.RightLowerLeg},
		{23, HumanBodyBones.RightFoot},
		//		{24, HumanBodyBones.RightToes},

		{25, HumanBodyBones.LeftShoulder},
		{26, HumanBodyBones.RightShoulder},
		{27, HumanBodyBones.LeftIndexProximal},
		{28, HumanBodyBones.RightIndexProximal},
		{29, HumanBodyBones.LeftThumbProximal},
		{30, HumanBodyBones.RightThumbProximal},
	};

	protected static readonly Dictionary<int, KinectInterop.JointType> boneIndex2JointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},

		{5, KinectInterop.JointType.ShoulderLeft},
		{6, KinectInterop.JointType.ElbowLeft},
		{7, KinectInterop.JointType.WristLeft},
		{8, KinectInterop.JointType.HandLeft},

		{9, KinectInterop.JointType.HandTipLeft},
		{10, KinectInterop.JointType.ThumbLeft},

		{11, KinectInterop.JointType.ShoulderRight},
		{12, KinectInterop.JointType.ElbowRight},
		{13, KinectInterop.JointType.WristRight},
		{14, KinectInterop.JointType.HandRight},

		{15, KinectInterop.JointType.HandTipRight},
		{16, KinectInterop.JointType.ThumbRight},

		{17, KinectInterop.JointType.HipLeft},
		{18, KinectInterop.JointType.KneeLeft},
		{19, KinectInterop.JointType.AnkleLeft},
		{20, KinectInterop.JointType.FootLeft},

		{21, KinectInterop.JointType.HipRight},
		{22, KinectInterop.JointType.KneeRight},
		{23, KinectInterop.JointType.AnkleRight},
		{24, KinectInterop.JointType.FootRight},
	};

	protected static readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2JointMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
		{27, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipLeft, KinectInterop.JointType.HandLeft} },
		{28, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipRight, KinectInterop.JointType.HandRight} },
		{29, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbLeft, KinectInterop.JointType.HandLeft} },
		{30, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbRight, KinectInterop.JointType.HandRight} },
	};

	protected static readonly Dictionary<int, KinectInterop.JointType> boneIndex2MirrorJointMap = new Dictionary<int, KinectInterop.JointType>
	{
		{0, KinectInterop.JointType.SpineBase},
		{1, KinectInterop.JointType.SpineMid},
		{2, KinectInterop.JointType.SpineShoulder},
		{3, KinectInterop.JointType.Neck},
		{4, KinectInterop.JointType.Head},

		{5, KinectInterop.JointType.ShoulderRight},
		{6, KinectInterop.JointType.ElbowRight},
		{7, KinectInterop.JointType.WristRight},
		{8, KinectInterop.JointType.HandRight},

		{9, KinectInterop.JointType.HandTipRight},
		{10, KinectInterop.JointType.ThumbRight},

		{11, KinectInterop.JointType.ShoulderLeft},
		{12, KinectInterop.JointType.ElbowLeft},
		{13, KinectInterop.JointType.WristLeft},
		{14, KinectInterop.JointType.HandLeft},

		{15, KinectInterop.JointType.HandTipLeft},
		{16, KinectInterop.JointType.ThumbLeft},

		{17, KinectInterop.JointType.HipRight},
		{18, KinectInterop.JointType.KneeRight},
		{19, KinectInterop.JointType.AnkleRight},
		{20, KinectInterop.JointType.FootRight},

		{21, KinectInterop.JointType.HipLeft},
		{22, KinectInterop.JointType.KneeLeft},
		{23, KinectInterop.JointType.AnkleLeft},
		{24, KinectInterop.JointType.FootLeft},
	};

	protected static readonly Dictionary<int, List<KinectInterop.JointType>> specIndex2MirrorMap = new Dictionary<int, List<KinectInterop.JointType>>
	{
		{25, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderRight, KinectInterop.JointType.SpineShoulder} },
		{26, new List<KinectInterop.JointType> {KinectInterop.JointType.ShoulderLeft, KinectInterop.JointType.SpineShoulder} },
		{27, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipRight, KinectInterop.JointType.HandRight} },
		{28, new List<KinectInterop.JointType> {KinectInterop.JointType.HandTipLeft, KinectInterop.JointType.HandLeft} },
		{29, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbRight, KinectInterop.JointType.HandRight} },
		{30, new List<KinectInterop.JointType> {KinectInterop.JointType.ThumbLeft, KinectInterop.JointType.HandLeft} },
	};

	protected static readonly Dictionary<KinectInterop.JointType, int> jointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
	{
		{KinectInterop.JointType.SpineBase, 0},
		{KinectInterop.JointType.SpineMid, 1},
		{KinectInterop.JointType.SpineShoulder, 2},
		{KinectInterop.JointType.Neck, 3},
		{KinectInterop.JointType.Head, 4},

		{KinectInterop.JointType.ShoulderLeft, 5},
		{KinectInterop.JointType.ElbowLeft, 6},
		{KinectInterop.JointType.WristLeft, 7},
		{KinectInterop.JointType.HandLeft, 8},

		{KinectInterop.JointType.HandTipLeft, 9},
		{KinectInterop.JointType.ThumbLeft, 10},

		{KinectInterop.JointType.ShoulderRight, 11},
		{KinectInterop.JointType.ElbowRight, 12},
		{KinectInterop.JointType.WristRight, 13},
		{KinectInterop.JointType.HandRight, 14},

		{KinectInterop.JointType.HandTipRight, 15},
		{KinectInterop.JointType.ThumbRight, 16},

		{KinectInterop.JointType.HipLeft, 17},
		{KinectInterop.JointType.KneeLeft, 18},
		{KinectInterop.JointType.AnkleLeft, 19},
		{KinectInterop.JointType.FootLeft, 20},

		{KinectInterop.JointType.HipRight, 21},
		{KinectInterop.JointType.KneeRight, 22},
		{KinectInterop.JointType.AnkleRight, 23},
		{KinectInterop.JointType.FootRight, 24},
	};

	protected static readonly Dictionary<KinectInterop.JointType, int> mirrorJointMap2boneIndex = new Dictionary<KinectInterop.JointType, int>
	{
		{KinectInterop.JointType.SpineBase, 0},
		{KinectInterop.JointType.SpineMid, 1},
		{KinectInterop.JointType.SpineShoulder, 2},
		{KinectInterop.JointType.Neck, 3},
		{KinectInterop.JointType.Head, 4},

		{KinectInterop.JointType.ShoulderRight, 5},
		{KinectInterop.JointType.ElbowRight, 6},
		{KinectInterop.JointType.WristRight, 7},
		{KinectInterop.JointType.HandRight, 8},

		{KinectInterop.JointType.HandTipRight, 9},
		{KinectInterop.JointType.ThumbRight, 10},

		{KinectInterop.JointType.ShoulderLeft, 11},
		{KinectInterop.JointType.ElbowLeft, 12},
		{KinectInterop.JointType.WristLeft, 13},
		{KinectInterop.JointType.HandLeft, 14},

		{KinectInterop.JointType.HandTipLeft, 15},
		{KinectInterop.JointType.ThumbLeft, 16},

		{KinectInterop.JointType.HipRight, 17},
		{KinectInterop.JointType.KneeRight, 18},
		{KinectInterop.JointType.AnkleRight, 19},
		{KinectInterop.JointType.FootRight, 20},

		{KinectInterop.JointType.HipLeft, 21},
		{KinectInterop.JointType.KneeLeft, 22},
		{KinectInterop.JointType.AnkleLeft, 23},
		{KinectInterop.JointType.FootLeft, 24},
	};


	protected static readonly Dictionary<int, List<HumanBodyBones>> specialIndex2MultiBoneMap = new Dictionary<int, List<HumanBodyBones>>
	{
		{27, new List<HumanBodyBones> {  // left fingers
				HumanBodyBones.LeftIndexProximal,
				HumanBodyBones.LeftIndexIntermediate,
				HumanBodyBones.LeftIndexDistal,
				HumanBodyBones.LeftMiddleProximal,
				HumanBodyBones.LeftMiddleIntermediate,
				HumanBodyBones.LeftMiddleDistal,
				HumanBodyBones.LeftRingProximal,
				HumanBodyBones.LeftRingIntermediate,
				HumanBodyBones.LeftRingDistal,
				HumanBodyBones.LeftLittleProximal,
				HumanBodyBones.LeftLittleIntermediate,
				HumanBodyBones.LeftLittleDistal,
			}},
		{28, new List<HumanBodyBones> {  // right fingers
				HumanBodyBones.RightIndexProximal,
				HumanBodyBones.RightIndexIntermediate,
				HumanBodyBones.RightIndexDistal,
				HumanBodyBones.RightMiddleProximal,
				HumanBodyBones.RightMiddleIntermediate,
				HumanBodyBones.RightMiddleDistal,
				HumanBodyBones.RightRingProximal,
				HumanBodyBones.RightRingIntermediate,
				HumanBodyBones.RightRingDistal,
				HumanBodyBones.RightLittleProximal,
				HumanBodyBones.RightLittleIntermediate,
				HumanBodyBones.RightLittleDistal,
			}},
		{29, new List<HumanBodyBones> {  // left thumb
				HumanBodyBones.LeftThumbProximal,
				HumanBodyBones.LeftThumbIntermediate,
				HumanBodyBones.LeftThumbDistal,
			}},
		{30, new List<HumanBodyBones> {  // right thumb
				HumanBodyBones.RightThumbProximal,
				HumanBodyBones.RightThumbIntermediate,
				HumanBodyBones.RightThumbDistal,
			}},
	};

}
