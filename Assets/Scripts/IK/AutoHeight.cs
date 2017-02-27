using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{
    public class AutoHeight : MonoBehaviour
    {
        [SerializeField]
        private float _preCalibrationHeight = 1.75f;
        [SerializeField]
        private float _samplingTime;
        private bool _calibrated;
        [SerializeField]
        private Transform _avatar;
        [SerializeField]
        private Transform _headMountedRig;
        [SerializeField]
        private Transform _cameraTransform;
        private float _accumulation;
        private int _samples;
        void Awake()
        {

        }

        void Update()
        {
            if(!_calibrated && Time.time < _samplingTime)
            {
                _samples++;
                _accumulation += _cameraTransform.localPosition.y;
            }
            else if(!_calibrated && Time.time > _samplingTime)
                Calibrate();
        }

        void Calibrate()
        {
            _calibrated = true;
            float adjustment = _accumulation / _samples;
            float diff = _preCalibrationHeight - adjustment;
            var aPos = _avatar.localPosition;
            var rigPos = _headMountedRig.localPosition;
            _avatar.localPosition = new Vector3(aPos.x, aPos.y - diff, aPos.z);
            _headMountedRig.localPosition = new Vector3(rigPos.x, rigPos.y + diff, rigPos.z);
        }
    }
}
