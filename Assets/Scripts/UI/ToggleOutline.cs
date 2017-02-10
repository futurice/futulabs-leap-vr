using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Futulabs
{
    public class ToggleOutline : MonoBehaviour
    {
        public bool StartsOn = true;
        private bool Toggled;
        public Color On;
        public Color Off;
        public Outline Outline;

        private void Start()
        {
            Toggled = !StartsOn;
            Toggle();
        }

        public void Toggle()
        {
            Toggled = !Toggled;
            if (Toggled)
                Outline.effectColor = On;
            else
                Outline.effectColor = Off;
        }
    }
}