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
            ImpactLightController light = Instantiate(LightPrefab, position, Quaternion.identity) as ImpactLightController;
            InteractableObjectControllerBase icb = collision.gameObject.GetComponentInParent<InteractableObjectControllerBase>();

            if (icb == null)
            {
                Debug.LogWarningFormat("LightWallController OnCollisionEnter: No InteractableObjectControllerBase present in the colliding game object: {0}", gameObject.name);
                return;
            }

            Color lightcolor = icb.EmissionColor;
            float intensityMultiplier = icb.WallImpactLightIntensityMultiplier;
            light.Init(collision.relativeVelocity.magnitude * intensityMultiplier, lightcolor);
        }
    }
}