using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace Futulabs
{
    public class HandUI : MonoBehaviour
    {
        /// <summary>
        /// Object used to figure out when to introduce UI
        /// </summary>
        public Transform Palm;
        /// <summary>
        /// Menu to show/hide
        /// </summary>
        public GameObject Menu;
        public CanvasGroup canvasGroup;
        public float RotationThreshold = 200f;
        public float TimeThresHold = 1f;
        private float TimeDT = 0;

        public Vector3 StartPos;
        public Vector3 EndPos;
        public float FadeTime = 0.25f;


        private bool MenuShown = false;

        void Start()
        {
            Menu.transform.localPosition = StartPos;
            canvasGroup.alpha = 0;
        }

        // Update is called once per frame
        void Update()
        {
            RotateToShowMenu(Palm);
        }

        private void RotateToShowMenu(Transform t)
        {
            Vector3 rot = t.localRotation.eulerAngles;
            var rotZ = rot.z;
            if (rotZ < 0)
                rotZ += 360;
            if (rotZ >= RotationThreshold && !MenuShown && rotZ >= 0)
            {
                TimeDT += Time.deltaTime;
                if (TimeDT >= TimeThresHold)
                {
                    ShowMenu();
                    MenuShown = true;
                    TimeDT = 0;
                }
            }
            if (rotZ < RotationThreshold && MenuShown)
            {
                TimeDT += Time.deltaTime;
                if (TimeDT >= TimeThresHold)
                {
                    HideMenu();
                    MenuShown = false;
                    TimeDT = 0;
                }
            }
        }

        Tweener moveTween;
        Tweener alphaTween;

        private void ShowMenu()
        {
            if(moveTween != null)
                moveTween.Kill();
            if (alphaTween != null)
                alphaTween.Kill();
            moveTween = Menu.transform.DOLocalMove(EndPos, FadeTime).SetEase(Ease.OutExpo);
            alphaTween = canvasGroup.DOFade(1, FadeTime).SetEase(Ease.OutExpo);
            canvasGroup.interactable = true;
        }

        private void HideMenu()
        {
            if (moveTween != null)
                moveTween.Kill();
            if (alphaTween != null)
                alphaTween.Kill();
            moveTween = Menu.transform.DOLocalMove(StartPos, 0.25f).SetEase(Ease.OutExpo);
            canvasGroup.interactable = false;
            alphaTween = canvasGroup.DOFade(0, FadeTime).SetEase(Ease.OutExpo);
        }
    }
}