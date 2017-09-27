using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using DG.Tweening;
using UniRx;
using System;
using UniRx.Triggers;

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

        public void Stick(Collider stickedObject)
        {
            if(!_isStuck)
            {
                _isStuck = true;
                Rigidbody.transform.rotation = Quaternion.identity;
                Rigidbody.isKinematic = true;
                Rigidbody.gameObject.tag = "WallCube";
                EffectAudioSource.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.INTERACTABLE_OBJECT_STICK));
                Rigidbody.gameObject.layer = LayerMask.NameToLayer("Environment");
                Observable.TimerFrame(0, FrameCountType.EndOfFrame).TakeUntilDestroy(this).Subscribe(_ =>
                {
                    LightWallController wallScript = Rigidbody.gameObject.AddComponent<LightWallController>();
                    wallScript.LightPrefab = LightControllerObject;
                });
                SetupUnsticking(stickedObject);
            }
        }

        // one in case something sticky unsticks, another in case it just gets destroyed
        private IDisposable _stickingSub;
        private IDisposable _otherStickingSub;

        private void Dispose(IDisposable disposable)
        {
            if(disposable != null)
            {
                disposable.Dispose();
            }
        }

        private void SetupUnsticking(Collider c)
        {
            if(_stickingSub != null)
            {
                _stickingSub.Dispose();
            }

            if(_otherStickingSub != null)
            {
                _otherStickingSub.Dispose();
            }

            var rigidbody = c.gameObject.GetComponentInChildren<Rigidbody>();
            
            if(rigidbody != null)
            {
                _stickingSub = rigidbody.ObserveEveryValueChanged(x => x.isKinematic).TakeUntilDestroy(this).Subscribe(kinematic => 
                {
                    if(!kinematic)
                    {
                        Unstick();
                        Dispose(_otherStickingSub);
                        Dispose(_stickingSub);
                    }
                });
            }
            _otherStickingSub = c.gameObject.OnDestroyAsObservable().TakeUntilDestroy(this).Subscribe(_ =>
            {
                Unstick();
                Dispose(_otherStickingSub);
                Dispose(_stickingSub);
            });
        }

        private void Unstick()
        {
            _isStuck = false;
            Rigidbody.isKinematic = false;
            Rigidbody.gameObject.tag = "InteractableObject";
            Rigidbody.gameObject.layer = LayerMask.NameToLayer("Interaction");
        }

    }

}