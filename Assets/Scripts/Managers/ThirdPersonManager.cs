using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;

namespace Futulabs
{
    public class ThirdPersonManager : MonoBehaviour
    {
        [SerializeField]
        private Camera _thirdPersonCamera;
        [SerializeField]
        private KeyCode _toggleCameraViewKey = KeyCode.Space;
        [SerializeField]
        private Transform _lookAtTarget;
        [SerializeField]
        private float _yOffset;
        [SerializeField]
        private float _radius = 2;
        [SerializeField]
        private float _rotationSpeed;

        private float _theta;
        private bool _thirdPersonCameraActive;

        private void Update()
        {
            if (_thirdPersonCameraActive)
            {
                float input = Input.GetAxis("Horizontal");
                _theta += input * _rotationSpeed;
                float x = 0 + _radius * Mathf.Cos(_theta * Mathf.PI / 180);
                float z = 0 + _radius * Mathf.Sin(_theta * Mathf.PI / 180);
                transform.position = new Vector3(x, _yOffset, z);
                transform.LookAt(_lookAtTarget);
            }

            if (Input.GetKeyDown(_toggleCameraViewKey))
            {
                ToggleThirdPersonCameraView();
            }
        }

        private void ToggleThirdPersonCameraView()
        {
            _thirdPersonCameraActive = !_thirdPersonCameraActive;

            VRSettings.showDeviceView = !_thirdPersonCameraActive;
            _thirdPersonCamera.enabled = _thirdPersonCameraActive;
        }
    }
}