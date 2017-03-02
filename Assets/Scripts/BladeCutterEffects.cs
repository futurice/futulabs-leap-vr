using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Futulabs
{
    public class BladeCutterEffects : MonoBehaviour
    {

        [SerializeField]
        private GameObject _leftBlade;
        [SerializeField]
        private GameObject _rightBlade;
        [SerializeField]
        private float _rotationX;
        [SerializeField]
        private float _rotationY;
        [SerializeField]
        private float _rotationYSlowly;

        [SerializeField]
        private float _positionZDeactivated;

        [SerializeField]
        private float _positionZActivated;

        private Tweener _deactivationTweener;
        private Tweener _deactivationOtherTweener;

        [SerializeField]
        private float _animateInTime = 0.25f;
        [SerializeField]
        private float _animateOutTime;

        [SerializeField]
        private Material _cutterMaterial;

        [SerializeField]
        private PlasmaCutEffect _plasmaCutPrefab;

        [SerializeField]
        private Color _originalColor; //Original color for the cutter material
        private Color _invisibleColor;
        [SerializeField]
        private BladeCutter _bladeScript;
        [SerializeField]
        private Collider _bladeCollider;

        [Header("Audio")]
        [SerializeField]
        private AudioSource _loopingAudio;
        public AudioSource oneTimeAudio;
        [SerializeField]
        private float _swingVelocitySoundActivation = 2f;
        [SerializeField]
        private float _swingCooldown = 0.5f;

        private float _swingDt = 0;
        private Vector3 _lastPosition;
        private bool _bladeActivated = false;
        private bool _swinged = false;
        private bool _handExtended = false;


        private void Start()
        {
            _leftBlade.transform.localRotation = Quaternion.Euler(_rotationX, -_rotationY, 0);
            _rightBlade.transform.localRotation = Quaternion.Euler(_rotationX, _rotationY, 0);
            _leftBlade.transform.localPosition = new Vector3(0, 0, _positionZDeactivated);
            _rightBlade.transform.localPosition = new Vector3(0, 0, _positionZDeactivated);
            _invisibleColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0);
            _cutterMaterial.SetColor("_TintColor", _invisibleColor);
            _loopingAudio.clip = AudioManager.Instance.GetAudioClip(GameAudioClipType.PLASMA_CUTTER_LOOP);
            _loopingAudio.loop = true;
            oneTimeAudio.loop = false;
            DisableCutting();
        }

        private void FixedUpdate()
        {
            CheckForSwing();
        }


        private void CheckForSwing()
        {
            _swinged = false;
            Vector3 currentPos = transform.position;
            if (_lastPosition != null && _swingDt >= _swingCooldown)
            {
                float diff = (currentPos - _lastPosition).magnitude;
                if (diff >= _swingVelocitySoundActivation && _bladeActivated)
                {
                    PlaySwingAudio();
                }
                else if (diff >= _swingVelocitySoundActivation && !_bladeActivated && _handExtended)
                {
                    ActivateScripts();
                }
            }
            _swingDt += Time.deltaTime;
            _lastPosition = currentPos;
        }

        #region Audio

        private void PlaySwingAudio()
        {
            _swingDt = 0;
            float swingPitch = Random.Range(0.9f, 1.1f);
            oneTimeAudio.pitch = swingPitch;
            int randomSwing = Mathf.RoundToInt(Random.Range((int)GameAudioClipType.PLASMA_CUTTER_SWING0, (int)GameAudioClipType.PLASMA_CUTTER_SWING3));
            oneTimeAudio.PlayOneShot(AudioManager.Instance.GetAudioClip((GameAudioClipType)randomSwing));
        }


        private void ActivateSound()
        {
            _loopingAudio.Play();
            oneTimeAudio.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.PLASMA_CUTTER_ACTIVATE));
        }

        private void DeactivateSound()
        {
            _loopingAudio.Stop();
            oneTimeAudio.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.PLASMA_CUTTER_DEACTIVATE));
        }
        #endregion

        /// <summary>
        /// This is called when the user's hand is open. 
        /// The user still has to move his hand fast to activate the script.
        /// </summary>
        public void ExtendHand()
        {
            _handExtended = true;
            _deactivationTweener.Kill();
            _deactivationOtherTweener.Kill();
        }

        /// <summary>
        /// This is called when the user's hand is closed. It will stop the script immediately.
        /// </summary>
        public void UnextendHand()
        {
            _handExtended = false;
            if (_bladeActivated)
            {
                SlowlyAnimateOut();
            }
        }


        private void ActivateScripts()
        {
            AnimateIn();
            ActivateSound();
            _bladeActivated = true;
        }

        private void DeactivateScripts()
        {
            AnimateOut();
            DeactivateSound();
            _bladeActivated = false;
        }

        private void AnimateIn()
        {
            _deactivationTweener.Kill();
            _deactivationOtherTweener.Kill();
            _leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, 0, 0), _animateInTime).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, 0, 0), _animateInTime).SetEase(Ease.OutExpo);
            _leftBlade.transform.DOLocalMove(new Vector3(0, 0, _positionZActivated), _animateInTime).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalMove(new Vector3(0, 0, _positionZActivated), _animateInTime).SetEase(Ease.OutExpo).OnComplete(EnableCutting);
            _cutterMaterial.DOColor(_originalColor, "_TintColor", _animateInTime).SetEase(Ease.OutExpo);
        }

        private void AnimateOut()
        {
            _deactivationTweener.Kill();
            _deactivationOtherTweener.Kill();
            _cutterMaterial.DOColor(_invisibleColor, "_TintColor", _animateInTime / 2f).SetEase(Ease.OutExpo);
            DisableCutting();
        }

        private void SlowlyAnimateOut()
        {
            _deactivationOtherTweener = _leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, -_rotationYSlowly, 0), _animateOutTime).SetEase(Ease.OutSine);
            _deactivationTweener = _rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, _rotationYSlowly, 0), _animateOutTime).SetEase(Ease.OutSine).OnComplete(DeactivateScripts);
            _deactivationTweener.OnKill(QuicklyAnimateIn);
        }

        private void QuicklyAnimateIn()
        {
            _leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, 0, 0), 0.25f).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, 0, 0), 0.25f).SetEase(Ease.OutExpo);
        }

        private void EnableCutting()
        {
            _bladeScript.enabled = true;
            _bladeCollider.enabled = true;
        }

        private void DisableCutting()
        {
            _bladeScript.enabled = false;
            _bladeCollider.enabled = false;
        }

        public void PlaceCutEffect(Vector3 pos)
        {
            var instance = Instantiate(_plasmaCutPrefab, pos, transform.rotation) as PlasmaCutEffect;
        }
    }
}
