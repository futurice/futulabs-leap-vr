using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{
    public class Gib : MonoBehaviour
    {
        public float disappearanceTime;
        private float _timeLived = 0;

        private void Update()
        {
            _timeLived += Time.deltaTime;
            if (_timeLived > disappearanceTime)
            {
                Disappear();
            }
        }

        private void Disappear()
        {
            Destroy(gameObject);
        }
    }
}