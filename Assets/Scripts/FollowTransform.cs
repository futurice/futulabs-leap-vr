using UnityEngine;
using System.Collections;

namespace Futulabs
{
    public class FollowTransform : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private bool _followPosition = true;
        [SerializeField]
        private bool _followRotation = true;
        [SerializeField]
        private Vector3 _positionOffset = Vector3.zero;

        [Header("Y-Axis Alignment")]
        [SerializeField]
        private bool _yAlign;
        [SerializeField]
        private Transform _yAlignTarget;
        [SerializeField]
        private Transform _yAlignTransform;

        private Vector3 _currentPosition = Vector3.zero;

        private void LateUpdate()
        {
            _currentPosition = _target.position + _positionOffset;

            if (_followPosition)
            {
                transform.position = _currentPosition;
            }

            if (_yAlign)
            {
                float diff = _yAlignTarget.position.y - _yAlignTransform.position.y;
                _currentPosition.y += diff + _positionOffset.y;
                transform.position = _currentPosition;
            }

            if (_followRotation)
            {
                transform.rotation = _target.rotation;
            }
        }
    }
}