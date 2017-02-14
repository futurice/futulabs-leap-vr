using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace Futulabs
{

    public class InteractableCubeController : InteractableObjectControllerBase
    {
        private bool _isSticky = false;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public void MakeSticky()
        {
            _isSticky = true;
            StickyOutline();
        }

        private void StickyOutline()
        {

        }
    }

}