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
        private float _positionZDeactivated;

        [SerializeField]
        private float _positionZActivated;

        private Tweener _leftTweener;
        private Tweener _rightTweener;

        [SerializeField]
        private float _animateTime = 0.25f;

        [SerializeField]
        private Material _cutterMaterial;

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
        [SerializeField]
        private AudioSource _oneTimeAudio;
        [SerializeField]
        private float _swingVelocitySoundActivation = 2f;
        [SerializeField]
        private float _swingCooldown = 0.5f;

        private float _swingDt = 0;
        private Vector3 _lastPosition;
        private bool _bladeActivated = false;

        



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
            _oneTimeAudio.loop = false;
        }
        #region Audio
        private void FixedUpdate()
        {
            CheckForSwing();
        }

        private void CheckForSwing()
        {
            Vector3 currentPos = transform.position;
            if (_lastPosition != null && _swingDt >= _swingCooldown && _bladeActivated)
            {
                float diff = (currentPos - _lastPosition).magnitude;
                Debug.Log(diff);
                if (diff >= _swingVelocitySoundActivation)
                    PlaySwing();
            }
            _swingDt += Time.deltaTime;
            _lastPosition = currentPos;
        }

        private void PlaySwing()
        {
            _swingDt = 0;
            float swingPitch = Random.Range(0.9f, 1.1f);
            _oneTimeAudio.pitch = swingPitch;
            int randomSwing = Mathf.RoundToInt(Random.Range((int)GameAudioClipType.PLASMA_CUTTER_SWING0, (int)GameAudioClipType.PLASMA_CUTTER_SWING3));
            _oneTimeAudio.PlayOneShot(AudioManager.Instance.GetAudioClip((GameAudioClipType)randomSwing));
        }


        private void ActivateSound()
        {
            _loopingAudio.Play();
            _oneTimeAudio.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.PLASMA_CUTTER_ACTIVATE));
        }

        private void DeactivateSound()
        {
            _loopingAudio.Stop();
            _oneTimeAudio.PlayOneShot(AudioManager.Instance.GetAudioClip(GameAudioClipType.PLASMA_CUTTER_DEACTIVATE));
        }
        #endregion 

        public void Activate()
        {
            AnimateIn();
            ActivateSound();
            _bladeActivated = true;
        }

        public void Deactivate()
        {
            AnimateOut();
            DeactivateSound();
            _bladeActivated = false;
        }

        private void AnimateIn()
        {
            _leftTweener.Kill();
            _rightTweener.Kill();
            _leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, 0, 0), _animateTime).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, 0, 0), _animateTime).SetEase(Ease.OutExpo);
            _leftBlade.transform.DOLocalMove(new Vector3(0, 0, _positionZActivated), _animateTime).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalMove(new Vector3(0, 0, _positionZActivated), _animateTime).SetEase(Ease.OutExpo);
            _cutterMaterial.DOColor(_originalColor, "_TintColor", _animateTime).SetEase(Ease.OutExpo);
            _bladeScript.enabled = true;
            _bladeCollider.enabled = true;
        }

        private void AnimateOut()
        {
            _leftTweener.Kill();
            _rightTweener.Kill();
            _leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, -_rotationY, 0), _animateTime).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(_rotationX, _rotationY, 0), _animateTime).SetEase(Ease.OutExpo);
            _leftBlade.transform.DOLocalMove(new Vector3(0, 0, _positionZDeactivated), _animateTime).SetEase(Ease.OutExpo);
            _rightBlade.transform.DOLocalMove(new Vector3(0, 0, _positionZDeactivated), _animateTime).SetEase(Ease.OutExpo);
            _cutterMaterial.DOColor(_invisibleColor, "_TintColor", _animateTime / 2f).SetEase(Ease.OutExpo);
            _bladeScript.enabled = false;
            _bladeCollider.enabled = false;
        }
    }
}
