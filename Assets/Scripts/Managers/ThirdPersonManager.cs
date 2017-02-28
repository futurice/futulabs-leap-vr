using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{
    public class ThirdPersonManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private float _yOffset;
        [SerializeField]
        private float _radius = 2;
        private float _theta;
        [SerializeField]
        private float _rotationSpeed;

        private void Update()
        {
            float input = Input.GetAxis("Horizontal");
            _theta += input* _rotationSpeed;
            float x = 0 + _radius * Mathf.Cos(_theta * Mathf.PI / 180);
            float z = 0 + _radius * Mathf.Sin(_theta * Mathf.PI / 180);
            transform.position = new Vector3(x, _yOffset, z);
            transform.LookAt(_target);
        }



    }
}