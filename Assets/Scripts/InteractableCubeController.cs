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
                _outlineMesh.sharedMaterial = SettingsManager.Instance.StickyOutlineMaterial;
        }


        protected override void OnCollisionEnter(Collision collision)
        {
            if (!_isSticky)
                base.OnCollisionEnter(collision);
            else
                if (collision.gameObject.tag.Equals("Wall"))
                Stick();
        }

        private void Stick()
        {
            _isStuck = true;
            transform.rotation = Quaternion.identity;
            Rigidbody.isKinematic = true;
            gameObject.tag = "Wall";
            EffectAudioSource.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.INTERACTABLE_OBJECT_STICK));
            gameObject.layer = LayerMask.NameToLayer("Environment");
            StartCoroutine(DelayAddScript());
        }

        private IEnumerator DelayAddScript()
        {
            yield return new WaitForEndOfFrame();
            LightWallController wallScript = gameObject.AddComponent<LightWallController>();
            wallScript.LightPrefab = LightControllerObject;
        }
    }

}