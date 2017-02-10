using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{

    public class BasketTrigger : MonoBehaviour
    {
        public BasketHoop hoopMaster;
        public void OnTriggerEnter(Collider other)
        {
            //TODO: Make sure it's a basketball
            hoopMaster.Triggered(this, other.gameObject);
        }
    }
}
