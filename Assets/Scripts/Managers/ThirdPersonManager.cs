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

        void Start()
        {
            Debug.Log("displays connected: " + Display.displays.Length);
            // Display.displays[0] is the primary, default display and is always ON.
            // Check if additional displays are available and activate each.
            if (Display.displays.Length > 1)
                Display.displays[1].Activate();
            if (Display.displays.Length > 2)
                Display.displays[2].Activate();
        }
        private void Update()
        {
            float input = Input.GetAxis("Horizontal");
            _theta += input * _rotationSpeed;
            float x = 0 + _radius * Mathf.Cos(_theta * Mathf.PI / 180);
            float z = 0 + _radius * Mathf.Sin(_theta * Mathf.PI / 180);
            transform.position = new Vector3(x, _yOffset, z);
            transform.LookAt(_target);
        }



    }
}