using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraNeckOffsetter : MonoBehaviour
{
    [SerializeField]
    protected Transform _headtracker;
    [SerializeField]
    protected Transform _cameraTarget;
    [SerializeField]
    [Range(-1f, 1f)]
    protected float _yOffset = 0f;

    protected float _previousYOffset = 0f;

    protected float _originalHeadTrackerPos;
    protected float _originalCameraTargetPos;

    protected void Awake()
    {
        _previousYOffset = _yOffset;
        RecordOriginalPositions();
    }

    protected void RecordOriginalPositions()
    {
        _originalHeadTrackerPos = _headtracker.localPosition.y;
        _originalCameraTargetPos = _cameraTarget.localPosition.y;
    }

    protected void Update()
    {
        if(_yOffset != _previousYOffset)
        {
            var oldPos = _cameraTarget.transform.localPosition;
            oldPos.y = _originalCameraTargetPos + _yOffset;
            _cameraTarget.transform.localPosition = oldPos;

            var oldHeadPos = _headtracker.transform.localPosition;
            oldHeadPos.y = _originalHeadTrackerPos - _yOffset;
            _headtracker.transform.localPosition = oldHeadPos;

            _previousYOffset = _yOffset;
        }
    }
}
