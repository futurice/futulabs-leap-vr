using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace Futulabs
{
    [RequireComponent(typeof(Light))]
    public class ImpactLightController : MonoBehaviour
    {
        [SerializeField]
        private Light _light;
        public void Init(float magnitude, Color color)
        {
            _light.color = color;
            _light.range = Mathf.Clamp(magnitude, 0.1f, 2f);
            _light.intensity = Mathf.Clamp(magnitude*2, 0.1f, 8f);
            _light.DOIntensity(0, magnitude).SetEase(Ease.OutExpo).OnComplete(Kill);
        }

        private void Kill()
        {
            Destroy(gameObject);
        }
    }
}
