using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using DG.Tweening;
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
                return _isSticky ? 1.0f : 1.0f;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected override void DimOutlineBloom(float magnitude)
        {
            if (!_isSticky)
                base.DimOutlineBloom(magnitude);
            else
            {
                magnitude *= SettingsManager.Instance.InteractableMaterialOutlineTransitionFactor;
                IlluminateOutlineBloom(magnitude);
                _minEmissionTween.Kill();
                _minDiffuseTween.Kill();
                _minGainTween.Kill();

                magnitude = Mathf.Max(SettingsManager.Instance.InteractableMaterialOutlineMinGlowTime, magnitude);
                magnitude = Mathf.Min(SettingsManager.Instance.InteractableMaterialOutlineMaxGlowTime, magnitude);

                _minEmissionTween = _outlineMesh.material.DOColor(SettingsManager.Instance.StickyMaterialMinEmissionColor, "_EmissionColor", magnitude).SetEase(Ease.OutExpo);
                _minDiffuseTween = _outlineMesh.material.DOColor(SettingsManager.Instance.StickyMaterialMinDiffuseColor, "_DiffuseColor", magnitude).SetEase(Ease.OutExpo);
                _minGainTween = _outlineMesh.material.DOFloat(SettingsManager.Instance.StickyMaterialMinEmissionGain, "_EmissionGain", magnitude).SetEase(Ease.OutExpo);
            }
        }

        public override void Create(InteractionManager interactionManager, PinchDetector leftPinchDetector, PinchDetector rightPinchDetector)
        {
            base.Create(interactionManager, leftPinchDetector, rightPinchDetector);
            _isSticky = GameManager.Instance.StickyCubes;
            if (_isSticky)
            {
                _outlineMesh.material = SettingsManager.Instance.StickyOutlineMaterial;
                IlluminateOutlineBloom();
                DimOutlineBloom(1);
            }
        }

        protected override void IlluminateOutlineBloom(float amount = 1)
        {
            if (!_isSticky)
                base.IlluminateOutlineBloom(amount);
            else
            {
                amount = Mathf.Clamp01(amount);

                Color emission = Color.Lerp(SettingsManager.Instance.StickyMaterialMinEmissionColor, SettingsManager.Instance.StickyMaterialMaxEmissionColor, amount);
                Color diffuse = Color.Lerp(SettingsManager.Instance.StickyMaterialMinDiffuseColor, SettingsManager.Instance.StickyMaterialMaxDiffuseColor, amount);
                float gain = Mathf.Lerp(SettingsManager.Instance.StickyMaterialMinEmissionGain, SettingsManager.Instance.InteractableMaterialMaxEmissionGain, amount);

                _outlineMesh.material.SetColor("_EmissionColor", emission);
                _outlineMesh.material.SetColor("_DiffuseColor", diffuse);
                _outlineMesh.material.SetFloat("_EmissionGain", gain);
            }
        }

        public override void Materialize()
        {
            base.Materialize();
            if (_isSticky)
            {
                var stickyComponent = SolidMeshGameObject.AddComponent<Stickyness>();
                stickyComponent.Init(this);
                IlluminateOutlineBloom();
                DimOutlineBloom(1);
            }
        }

        public void Stick()
        {
            _isStuck = true;
            Rigidbody.transform.rotation = Quaternion.identity;
            Rigidbody.isKinematic = true;
            Rigidbody.gameObject.tag = "WallCube";
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