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
        private bool _isStuck = false;

        private string _lightControllerPrefabPath = "Prefabs/ImpactLight";

        private ImpactLightController LightControllerObject
        {
            get
            {
                return (Resources.Load(_lightControllerPrefabPath) as GameObject).GetComponent<ImpactLightController>();
            }
        }

        public override float WallImpactLightIntensityMultiplier
        {
            get
            {
                return _isSticky ? 12.0f : 1.0f;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void Materialize()
        {
            base.Materialize();
            _isSticky = GameManager.Instance.StickyCubes;
            if (_isSticky)
            {
                _outlineMesh.sharedMaterial = SettingsManager.Instance.StickyOutlineMaterial;
                var stickyComponent = SolidMeshGameObject.AddComponent<Stickyness>();
                stickyComponent.Init(this);
            }
        }

        public void Stick()
        {
            _isStuck = true;
            Rigidbody.transform.rotation = Quaternion.identity;
            Rigidbody.isKinematic = true;
            Rigidbody.gameObject.tag = "Wall";
            EffectAudioSource.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.INTERACTABLE_OBJECT_STICK));
            Rigidbody.gameObject.layer = LayerMask.NameToLayer("Environment");
            StartCoroutine(DelayAddScript());
        }

        private IEnumerator DelayAddScript()
        {
            yield return new WaitForEndOfFrame();
            LightWallController wallScript = Rigidbody.gameObject.AddComponent<LightWallController>();
            wallScript.LightPrefab = LightControllerObject;
        }
    }

}