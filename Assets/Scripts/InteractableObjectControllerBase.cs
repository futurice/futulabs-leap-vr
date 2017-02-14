using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;
using DG.Tweening;

namespace Futulabs
{
    [RequireComponent(typeof(AudioSource))]             // Required to play sound effects related to this object
    [RequireComponent(typeof(Rigidbody))]               // Required by InteractionBehaviour
    [RequireComponent(typeof(LeapRTS))]                 // Required to do transform manipulations with Leap Motion
    [RequireComponent(typeof(InteractionBehaviour))]    // Required to for physics interactions with the Leap Motions hands
    public class InteractableObjectControllerBase : MonoBehaviour, IInteractableObjectController
    {
        [Header("Meshes")]
        [SerializeField]
        [Tooltip("Meshes that should be enabled when the object is being created i.e. outlines")]
        protected MeshRenderer[] _outlineMeshes;
        [SerializeField]
        [Tooltip("Meshes that should be enabled when the object has materialized")]
        protected MeshRenderer[] _materializedMeshes;

        [Header("Scaling")]
        [SerializeField]
        [Tooltip("Does the script implement its own scaling or should it use the Leap RTS default uniform scaling")]
        protected bool _overrideLeapRTSScaling = false;

        protected LeapRTS _leapRTSComponent;
        protected InteractionBehaviour _leapInteractionBehaviour;
        protected AudioSource _effectAudioSource;
        protected Collider[] _colliders;
        protected Rigidbody[] _rigidbodies;

        protected Vector3[] _velocityFrames;
        protected bool _creating = false;
        protected int _velocityIndex = 0;

        protected Vector3 _currentFramePos;
        protected Vector3 _lastFramePos;
        protected Quaternion _lastRotationFrame;
        protected Quaternion _currentRotationFrame;

        protected Tweener _minEmissionTween;
        protected Tweener _minDiffuseTween;
        protected Tweener _minGainTween;

        public LeapRTS LeapRTSComponent
        {
            get
            {
                if (_leapRTSComponent == null)
                {
                    _leapRTSComponent = GetComponent<LeapRTS>();
                }

                return _leapRTSComponent;
            }
        }

        public InteractionBehaviour LeapInteractionBehaviour
        {
            get
            {
                if (_leapInteractionBehaviour == null)
                {
                    _leapInteractionBehaviour = GetComponent<InteractionBehaviour>();
                }

                return _leapInteractionBehaviour;
            }
        }

        public bool OverrideLeapRTSScaling
        {
            get
            {
                return _overrideLeapRTSScaling;
            }
        }

        public bool UseGravity
        {
            get
            {
                return LeapInteractionBehaviour.useGravity;
            }

            set
            {
                LeapInteractionBehaviour.useGravity = value;

                // Apply a little bit of force to get the object moving upwards
                if (value == false)
                {
                    LeapInteractionBehaviour.rigidbody.AddForce(10 * new Vector3(0, 1, 0));
                }
            }
        }

        public void ChangePitch(Vector3 leftPos, Vector3 rightPos)
        {
            var size = (leftPos - rightPos).magnitude;
            var pitch = Mathf.Round(100f * (Mathf.Pow(size, 0.4f) + 0.4f)) / 100f;
            EffectAudioSource.pitch = pitch;
        }

        protected AudioSource EffectAudioSource
        {
            get
            {
                if (_effectAudioSource == null)
                {
                    _effectAudioSource = GetComponent<AudioSource>();
                }

                return _effectAudioSource;
            }
        }

        protected Collider[] Colliders
        {
            get
            {
                if (_colliders == null)
                {
                    _colliders = GetComponentsInChildren<Collider>();
                }

                return _colliders;
            }
        }

        protected Rigidbody[] Rigidbodies
        {
            get
            {
                if (_rigidbodies == null)
                {
                    _rigidbodies = GetComponentsInChildren<Rigidbody>();
                }

                return _rigidbodies;
            }
        }

        virtual public Color EmissionColor
        {
            get
            {
                return _outlineMeshes[0].material.GetColor("_EmissionColor");
            }
        }

        virtual public float WallImpactLightIntensityMultiplier
        {
            get
            {
                return 1.0f;
            }
        }

        virtual protected void Start()
        {
            EnableMaterializedMeshes(false);
            EnableOutlineMeshes(false);
            StartCoroutine(Activate());
        }

        virtual protected IEnumerator Activate()
        {
            yield return new WaitForEndOfFrame();
            EnableOutlineMeshes(true);
        }

        virtual protected void Update()
        {
            if(_creating)
            {

            }
        }

        virtual protected void FixedUpdate()
        {
            if (_creating)
            {
                _lastRotationFrame = _currentRotationFrame;
                _currentRotationFrame = transform.rotation;
                CaptureFrame();
            }
        }

        virtual protected void CaptureFrame()
        {
            if (_currentFramePos != null)
                _lastFramePos = _currentFramePos;
            _currentFramePos = transform.position;
            if (_lastFramePos == null)
                return;

            Vector3 difference = _currentFramePos - _lastFramePos;
            _velocityFrames[_velocityIndex] = difference;
            _velocityIndex++;
            _velocityIndex = _velocityIndex % ObjectManager.Instance.CreationForceWindowSize;
        }

        virtual protected Vector3 CalculateCurrentVelocity()
        {
            Vector3 average = new Vector3();
            for (int i = 0; i < _velocityFrames.Length; i++)
            {
                average = average + _velocityFrames[i];
            }
            average /= ObjectManager.Instance.CreationForceWindowSize;
            return average;
        }

        virtual public void Create(InteractionManager interactionManager, PinchDetector leftPinchDetector, PinchDetector rightPinchDetector)
        {
            _velocityFrames = new Vector3[ObjectManager.Instance.CreationForceWindowSize];
            _creating = true;
            LeapRTSComponent.AllowScale = !OverrideLeapRTSScaling;
            LeapRTSComponent.enabled = true;
            LeapRTSComponent.PinchDetectorA = leftPinchDetector;
            LeapRTSComponent.PinchDetectorB = rightPinchDetector;

            LeapInteractionBehaviour.enabled = false;
            LeapInteractionBehaviour.Manager = interactionManager;

            // Turn off Collider and Rigidbody components to disable physics
            EnableCollidersAndRigidbodies(false);

            // Turn off materialized meshes
            EnableMaterializedMeshes(false);

            // Turn on outline meshes
            EnableOutlineMeshes(true);
            IlluminateOutlineBloom();

            // Enable looping and play the creation sound effect
            EffectAudioSource.loop = true;
            EffectAudioSource.clip = AudioManager.Instance.GetAudioClip(GameAudioClipType.INTERACTABLE_OBJECT_CREATING);
            EffectAudioSource.Play();

        }

        virtual protected void AddCreationForce()
        {
            Vector3 velocity = CalculateCurrentVelocity();
            Rigidbodies[0].velocity = velocity * ObjectManager.Instance.CreationForceScaleFactor;
            Vector3 angularVel = ((_currentRotationFrame) * Quaternion.Inverse(_lastRotationFrame)).eulerAngles;
            angularVel = new Vector3(
                Mathf.DeltaAngle(0, angularVel.x) * Mathf.Deg2Rad,
                Mathf.DeltaAngle(0, angularVel.y) * Mathf.Deg2Rad,
                Mathf.DeltaAngle(0, angularVel.z) * Mathf.Deg2Rad) / Time.fixedDeltaTime;
            Rigidbodies[0].angularVelocity = angularVel;
        }

        virtual public void Materialize()
        {
            // Turn off the Leap RTS controller - there can be only one active at a time
            LeapRTSComponent.enabled = false;
            LeapInteractionBehaviour.enabled = true;

            AddCreationForce();

            // Turn on Collider and Rigidbody components to enable physics
            EnableCollidersAndRigidbodies(true);

            // Turn off the outline meshes
            EnableOutlineMeshes(false);

            // Turn on materialized meshes
            EnableMaterializedMeshes(true);

            // Disable looping and play the materialization sound effect
            EffectAudioSource.pitch = 1;
            EffectAudioSource.loop = false;
            EffectAudioSource.Stop();
            EffectAudioSource.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.INTERACTABLE_OBJECT_MATERIALIZATION));
        }

        virtual public void Morph(Vector3 leftPinchPosition, Vector3 rightPinchPosition)
        {

        }

        virtual protected void EnableOutlineMeshes(bool enabled)
        {
            int numOutlineMeshes = _outlineMeshes.Length;

            for (int i = 0; i < numOutlineMeshes; ++i)
            {
                _outlineMeshes[i].enabled = enabled;
            }
        }

        virtual protected void EnableMaterializedMeshes(bool enabled)
        {
            int numMaterializedMeshes = _materializedMeshes.Length;

            for (int i = 0; i < numMaterializedMeshes; ++i)
            {
                _materializedMeshes[i].enabled = enabled;
            }
        }

        virtual protected void EnableCollidersAndRigidbodies(bool enabled)
        {
            Collider[] colliders = Colliders;
            int numColliders = colliders.Length;

            for (int i = 0; i < numColliders; ++i)
            {
                colliders[i].enabled = enabled;
            }

            LeapInteractionBehaviour.isKinematic = !enabled;

            Rigidbody[] rigidbodies = Rigidbodies;
            int numRigidbodies = rigidbodies.Length;

            for (int i = 0; i < numRigidbodies; ++i)
            {
                rigidbodies[i].detectCollisions = enabled;
            }
        }

        virtual protected void OnCollisionEnter(Collision collision)
        {
            var velocityMag = collision.relativeVelocity.magnitude;
            DimOutlineBloom(velocityMag);

            // Play sound if the collision was 'energetic' enough
            if (velocityMag > 2.0f) //TODO: nice magic number broonas
            {
                EffectAudioSource.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.INTERACTABLE_OBJECT_COLLISION));
            }
        }

        virtual protected void DimOutlineBloom(float magnitude)
        {
            magnitude *= SettingsManager.Instance.InteractableMaterialOutlineTransitionFactor;
            IlluminateOutlineBloom(magnitude);
            _minEmissionTween.Kill();
            _minDiffuseTween.Kill();
            _minGainTween.Kill();

            magnitude = Mathf.Max(SettingsManager.Instance.InteractableMaterialOutlineMinGlowTime, magnitude);
            magnitude = Mathf.Min(SettingsManager.Instance.InteractableMaterialOutlineMaxGlowTime, magnitude);

            _minEmissionTween = _outlineMeshes[0].material.DOColor(SettingsManager.Instance.InteractableMaterialMinEmissionColor, "_EmissionColor", magnitude).SetEase(Ease.OutExpo);
            _minDiffuseTween = _outlineMeshes[0].material.DOColor(SettingsManager.Instance.InteractableMaterialMinDiffuseColor, "_DiffuseColor", magnitude).SetEase(Ease.OutExpo);
            _minGainTween = _outlineMeshes[0].material.DOFloat(SettingsManager.Instance.InteractableMaterialMinEmissionGain, "_EmissionGain", magnitude).SetEase(Ease.OutExpo);
        }

        virtual protected void IlluminateOutlineBloom(float amount = 1)
        {
            amount = Mathf.Clamp01(amount);

            Color emission = Color.Lerp(SettingsManager.Instance.InteractableMaterialMinEmissionColor, SettingsManager.Instance.InteractableMaterialMaxEmissionColor, amount);
            Color diffuse = Color.Lerp(SettingsManager.Instance.InteractableMaterialMinDiffuseColor, SettingsManager.Instance.InteractableMaterialMaxDiffuseColor, amount);
            float gain = Mathf.Lerp(SettingsManager.Instance.InteractableMaterialMinEmissionGain, SettingsManager.Instance.InteractableMaterialMaxEmissionGain, amount);

            _outlineMeshes[0].material.SetColor("_EmissionColor", emission);
            _outlineMeshes[0].material.SetColor("_DiffuseColor", diffuse);
            _outlineMeshes[0].material.SetFloat("_EmissionGain", gain);
        }
    }
}