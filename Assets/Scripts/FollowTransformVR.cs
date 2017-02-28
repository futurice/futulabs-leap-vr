using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransformVR : MonoBehaviour
{
    [SerializeField]
    protected Transform _target;
    [SerializeField]
    protected Vector3 _positionOffset;
    [SerializeField]
    protected Vector3 _rotationOffset;
    [SerializeField]
    protected bool _followPosition;
    [SerializeField]
    protected bool _followRotation;

    protected void Update()
    {
        if (_followPosition)
            transform.position = _target.position + _positionOffset;

        if (_followRotation)
            transform.rotation = _target.rotation * Quaternion.Euler(_rotationOffset);
    }
}
