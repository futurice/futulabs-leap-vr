using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{ 
    public class TouchController : MonoBehaviour
    {
        [SerializeField]
        OVRInput.Controller _controller;

        private void Update()
        {
            Vector3 localPosition = OVRInput.GetLocalControllerPosition(_controller);
            Quaternion localRotation = OVRInput.GetLocalControllerRotation(_controller);

            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
        }
    }
}