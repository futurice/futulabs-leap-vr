using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Futulabs
{
    [RequireComponent(typeof(Collider))]
    public class LightWallController : MonoBehaviour
    {
        public ImpactLightController LightPrefab;

        private void OnCollisionEnter(Collision collision)
        {
            Vector3 position = collision.transform.position;
            InteractableObjectControllerBase icb = collision.gameObject.GetComponentInParent<InteractableObjectControllerBase>();

            if (icb == null)
            {
                Debug.LogWarningFormat("LightWallController OnCollisionEnter: No InteractableObjectControllerBase present in the colliding game object: {0}", gameObject.name);
                return;
            }
            float intensityMultiplier = icb.WallImpactLightIntensityMultiplier;
            var magnitude = collision.relativeVelocity.magnitude * intensityMultiplier;

            if(magnitude > 0)
            {
                Color lightcolor = icb.EmissionColor;
                ImpactLightController light = Instantiate(LightPrefab, position, Quaternion.identity) as ImpactLightController;
                light.Init(magnitude, lightcolor);
            }
        }
    }
}