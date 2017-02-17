using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{
    public class Stickyness : MonoBehaviour
    {
        public InteractableCubeController CubeComponent;

        public void Init(InteractableCubeController parent)
        {
            CubeComponent = parent;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (CubeComponent == null)
                return;
            if (collision.gameObject.tag.Equals("Wall"))
                CubeComponent.Stick();
        }

    }
}