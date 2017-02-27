using UnityEngine;
//using Windows.Kinect;

using System;
using System.Collections;

public class CubemanController : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("Whether the cubeman is allowed to move vertically or not.")]
	public bool verticalMovement = true;

	[Tooltip("Whether the cubeman is facing the player or not.")]
	public bool mirroredMovement = false;

	[Tooltip("Rate at which the cubeman will move through the scene.")]
	public float moveRate = 1f;

    [Tooltip("Is the root motion of the model handled externally?")]
    public bool externalRootMotion = false;

	public GameObject Hip_Center;
	public GameObject Spine;
	public GameObject Neck;
	public GameObject Head;
	public GameObject Shoulder_Left;
	public GameObject Elbow_Left;
	public GameObject Wrist_Left;
	public GameObject Hand_Left;
	public GameObject Shoulder_Right;
	public GameObject Elbow_Right;
	public GameObject Wrist_Right;
	public GameObject Hand_Right;
	public GameObject Hip_Left;
	public GameObject Knee_Left;
	public GameObject Ankle_Left;
	public GameObject Foot_Left;
	public GameObject Hip_Right;
	public GameObject Knee_Right;
	public GameObject Ankle_Right;
	public GameObject Foot_Right;
	public GameObject Spine_Shoulder;
    public GameObject Hand_Tip_Left;
    public GameObject Thumb_Left;
    public GameObject Hand_Tip_Right;
    public GameObject Thumb_Right;

	public bool drawJoints;
	public bool drawSkeleton;
	public LineRenderer skeletonLine;

	private GameObject[] bones;
	private MeshRenderer[] boneMeshRenderers;
	private LineRenderer[] lines;

	private LineRenderer lineTLeft;
	private LineRenderer lineTRight;
	private LineRenderer lineFLeft;
	private LineRenderer lineFRight;

	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private Vector3 initialPosOffset = Vector3.zero;
	private Int64 initialPosUserID = 0;

	public Vector3 RightLegDirection
	{
		get {
			return bones [17].transform.position - bones [16].transform.position;
		}
	}

	public Vector3 LeftLegDirection
	{
		get {
			return bones [13].transform.position - bones [12].transform.position;
		}
	}

	public bool Visible
	{
		get {
			return drawJoints || drawSkeleton;
		}

		set {
			drawJoints = value;
			drawSkeleton = value;
		}
	}

	private void Start() 
	{
		//store bones in a list for easier access
		bones = new GameObject[] {
			Hip_Center,
            Spine,
            Neck,
            Head,
            Shoulder_Left,
            Elbow_Left,
            Wrist_Left,
            Hand_Left,
            Shoulder_Right,
            Elbow_Right,
            Wrist_Right,
            Hand_Right,
            Hip_Left,
            Knee_Left,
            Ankle_Left,
            Foot_Left,
            Hip_Right,
            Knee_Right,
            Ankle_Right,
            Foot_Right,
            Spine_Shoulder,
            Hand_Tip_Left,
            Thumb_Left,
            Hand_Tip_Right,
            Thumb_Right
		};

		// Array holding the bone mesh renderers (for rendering joints)
		boneMeshRenderers = new MeshRenderer[bones.Length];

		for (int i = 0; i < boneMeshRenderers.Length; ++i)
		{
			boneMeshRenderers[i] = bones[i].GetComponent<MeshRenderer>();
		}
		
		// array holding the skeleton lines
		lines = new LineRenderer[bones.Length];
		initialPosition = transform.position;
		initialRotation = transform.rotation;
	}

	private void Update() 
	{
		KinectManager manager = KinectManager.Instance;
		
		// get 1st player
		Int64 userID = manager ? manager.GetUserIdByIndex(playerIndex) : 0;
		
		if (userID <= 0)
		{
			initialPosUserID = 0;
			initialPosOffset = Vector3.zero;

			// reset the pointman position and rotation
			if (transform.position != initialPosition)
			{
				transform.position = initialPosition;
			}
			
			if (transform.rotation != initialRotation)
			{
				transform.rotation = initialRotation;
			}

			for (int i = 0; i < bones.Length; i++) 
			{
				bones[i].gameObject.SetActive(true);

				bones[i].transform.localPosition = Vector3.zero;
				bones[i].transform.localRotation = Quaternion.identity;
				
				if (lines[i] != null)
				{
					lines[i].gameObject.SetActive(false);
				}
			}

			return;
		}
		
		// set the position in space
		Vector3 posPointMan = manager.GetUserPosition(userID);
		Vector3 posPointManMP = new Vector3(posPointMan.x, posPointMan.y, !mirroredMovement ? -posPointMan.z : posPointMan.z);
		
		// store the initial position
		if (initialPosUserID != userID)
		{
			initialPosUserID = userID;
			//initialPosOffset = transform.position - (verticalMovement ? posPointMan * moveRate : new Vector3(posPointMan.x, 0, posPointMan.z) * moveRate);
			initialPosOffset = posPointMan;
		}
        
        if (!externalRootMotion)
        { 
		    if (moveRate > 0.0f)
		    {
			    Vector3 relPosUser = (posPointMan - initialPosOffset);
			    relPosUser.z =!mirroredMovement ? -relPosUser.z : relPosUser.z;

			    transform.position = initialPosOffset +
			    (verticalMovement ? relPosUser * moveRate : new Vector3 (relPosUser.x, 0, relPosUser.z) * moveRate);
		    }
        }

        // update the local positions of the bones
        for (int i = 0; i < bones.Length; ++i) 
		{
			if (bones[i] != null)
			{
				boneMeshRenderers[i].enabled = drawJoints;

				if (lines[i] != null)
				{
					lines[i].enabled = drawSkeleton;
				}

				int joint = !mirroredMovement ? i : (int)KinectInterop.GetMirrorJoint((KinectInterop.JointType)i);

				if (joint < 0)
				{
					continue;
				}

				if (manager.IsJointTracked(userID, joint))
				{
					bones[i].gameObject.SetActive(true);
					
					Vector3 posJoint = manager.GetJointPosition(userID, joint);
					posJoint.z = !mirroredMovement ? -posJoint.z : posJoint.z;
					
					Quaternion rotJoint = manager.GetJointOrientation(userID, joint, !mirroredMovement);
					rotJoint = initialRotation * rotJoint;
					posJoint -= posPointManMP;
					
					if (mirroredMovement)
					{
						posJoint.x = -posJoint.x;
						posJoint.z = -posJoint.z;
					}

					bones[i].transform.localPosition = posJoint;
					bones[i].transform.rotation = rotJoint;
				}
				else
				{
					bones[i].gameObject.SetActive(false);
					
					if (lines[i] != null)
					{
						lines[i].gameObject.SetActive(false);
					}
				}
			}	
		}

		if (drawSkeleton)
		{
			DrawSkeleton();
		}
	}

	private void DrawSkeleton ()
	{
		if (bones == null)
		{
			return;
		}

		KinectManager manager = KinectManager.Instance;
		Int64 userID = manager ? manager.GetUserIdByIndex(playerIndex) : 0;

		for (int i = 0; i < bones.Length; ++i)
		{
			if (!manager.IsJointTracked(userID, i))
			{
				continue;
			}

			int joint = !mirroredMovement ? i : (int)KinectInterop.GetMirrorJoint((KinectInterop.JointType)i);
			int parentJoint = (int)manager.GetParentJoint((KinectInterop.JointType)joint);

			if (joint < 0 || parentJoint < 0)
			{
				continue;
			}

			if (lines[i] == null && skeletonLine != null)
			{
				lines[i] = Instantiate(skeletonLine) as LineRenderer;
				lines[i].transform.SetParent(transform, false);
				lines[i].transform.localPosition = Vector3.zero;
				lines[i].useWorldSpace = false;
			}

			if (lines[i] != null)
			{
				lines[i].SetPosition(0, bones[parentJoint].transform.localPosition);
				lines[i].SetPosition(1, bones[i].transform.localPosition);
				lines[i].gameObject.SetActive(true);
			}
		}
	}

}