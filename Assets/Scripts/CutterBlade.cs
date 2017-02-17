using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace Futulabs
{
    public class CutterBlade : MonoBehaviour
    {

        [SerializeField]
        private GameObject leftBlade;
        [SerializeField]
        private GameObject rightBlade;
        [SerializeField]
        private float RotationX;
        [SerializeField]
        private float RotationY;

        [SerializeField]
        private float PositionZ_Deactivated;

        [SerializeField]
        private float PositionZ_Activated;

        private Tweener LeftTweener;
        private Tweener RightTweener;

        [SerializeField]
        private float AnimateTime = 0.25f;

        private void Start()
        {
            leftBlade.transform.localRotation = Quaternion.Euler(RotationX, -RotationY, 0);
            rightBlade.transform.localRotation = Quaternion.Euler(RotationX, RotationY, 0);
            leftBlade.transform.localPosition = new Vector3(0, 0, PositionZ_Deactivated);
            rightBlade.transform.localPosition = new Vector3(0, 0, PositionZ_Deactivated);
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
            LeftTweener.Kill();
            RightTweener.Kill();
            leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(RotationX, 0, 0), AnimateTime).SetEase(Ease.OutExpo);
            rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(RotationX, 0, 0), AnimateTime).SetEase(Ease.OutExpo);
            leftBlade.transform.DOLocalMove(new Vector3(0, 0, PositionZ_Activated), AnimateTime).SetEase(Ease.OutExpo);
            rightBlade.transform.DOLocalMove(new Vector3(0, 0, PositionZ_Activated), AnimateTime).SetEase(Ease.OutExpo);
        }

        private void AnimateOut()
        {
            LeftTweener.Kill();
            RightTweener.Kill();
            leftBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(RotationX, -RotationY, 0), AnimateTime).SetEase(Ease.OutExpo);
            rightBlade.transform.DOLocalRotateQuaternion(Quaternion.Euler(RotationX, RotationY, 0), AnimateTime).SetEase(Ease.OutExpo);
            leftBlade.transform.DOLocalMove(new Vector3(0, 0, PositionZ_Deactivated), AnimateTime).SetEase(Ease.OutExpo);
            rightBlade.transform.DOLocalMove(new Vector3(0, 0, PositionZ_Deactivated), AnimateTime).SetEase(Ease.OutExpo);
        }
    }
}
