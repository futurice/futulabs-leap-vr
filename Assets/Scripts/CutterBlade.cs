using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace Futulabs
{
    public class CutterBlade : MonoBehaviour
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

        private void Start()
        {
            _leftBlade.transform.localRotation = Quaternion.Euler(_rotationX, -_rotationY, 0);
            _rightBlade.transform.localRotation = Quaternion.Euler(_rotationX, _rotationY, 0);
            _leftBlade.transform.localPosition = new Vector3(0, 0, _positionZDeactivated);
            _rightBlade.transform.localPosition = new Vector3(0, 0, _positionZDeactivated);
            _invisibleColor = new Color(_originalColor.r, _originalColor.g, _originalColor.b, 0);
            _cutterMaterial.SetColor("_TintColor", _invisibleColor);
        }


        public void Activate()
        {
            AnimateIn();
        }

        public void Deactivate()
        {
            AnimateOut();
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
            _cutterMaterial.DOColor(_invisibleColor, "_TintColor", _animateTime/2f).SetEase(Ease.OutExpo);
            _bladeScript.enabled = false;
            _bladeCollider.enabled = false;
        }
    }
}
