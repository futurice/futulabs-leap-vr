using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoHeightener : MonoBehaviour
{
    [SerializeField]
    private float _captureTime = 3f;
    private float _currentCapturingTime = 0f;
    private float _accumulatedHeight;
    private int _heightFrames;

    [SerializeField]
    private Transform _heightInput;
    private bool _heightSet = false;

    [SerializeField]
    private float _oculusToMetersFactor = 1.169f;
    [SerializeField]
    private float _eyeToTopOfHead = 0.05f;
    [SerializeField]
    private float _initialDummyHeight = 1.75f;
    private void Start()
    {

    }

    private void Update()
    {
        _currentCapturingTime += Time.deltaTime;
        if (_currentCapturingTime <= _captureTime)
        {
            _heightFrames++;
            CaptureHeight();
        }
        else if(!_heightSet)
        {
            _heightSet = true;
            SetHeight();
        }
    }

    private void CaptureHeight()
    {
        _accumulatedHeight += _heightInput.position.y;
    }

    private void SetHeight()
    {
        var height = (_accumulatedHeight / _heightFrames) * _oculusToMetersFactor + _eyeToTopOfHead;
        Debug.Log(height);
        var dummyScale = height / _initialDummyHeight;
        Debug.Log(dummyScale);
    }

    /*
    private float MagicMath(float personHeight)
    {
        Debug.Log("Input: " + personHeight);
        float diff = 9.375f * Mathf.Pow(personHeight, 2) - 32.75f * personHeight + 28.6f;
        if (personHeight > _initialDummyHeight)
            diff *= -1;
        return diff;
    }

    private void SetHeight()
    {
        float averageHeight = _accumulatedHeight / _heightFrames - _oculusHeightOffset;
        var oldHeadPos = _headMountedRig.transform.localPosition;
        var oldLeftPos = _leftFootTracker.transform.localPosition;
        var oldRightPos = _rightFootTracker.transform.localPosition;
        float heightDiff = MagicMath(averageHeight);
        oldHeadPos.y = -0.20f;
        oldLeftPos.y = heightDiff;
        oldRightPos.y = heightDiff;

        _headMountedRig.transform.localPosition = oldHeadPos;
        _leftFootTracker.transform.localPosition = oldLeftPos;
        _rightFootTracker.transform.localPosition = oldRightPos;
        _avatar.transform.localPosition = oldHeadPos;
    }*/
}
