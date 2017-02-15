using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace Futulabs
{
    public class HandUI : MonoBehaviour
    {
        public GameObject Menu;
        public CanvasGroup canvasGroup;
        public float RotationThreshold = 200f;
        public float TimeThresHold = 1f;

        public Vector3 StartPos;
        public Vector3 EndPos;
        public float FadeTime = 0.25f;

        private bool _menuShown = false;
        private float _timeDT = 0;
        private Tweener _moveTween;
        private Tweener _alphaTween;

        private void Awake()
        {
            HideMenu();
        }

        private void KillMenuTweens()
        {
            if (_moveTween != null)
            {
                _moveTween.Kill();
            }

            if (_alphaTween != null)
            {
                _alphaTween.Kill();
            }
        }

        public void ShowMenu()
        {
            KillMenuTweens();

            _menuShown = true;
            _moveTween = Menu.transform.DOLocalMove(EndPos, FadeTime).SetEase(Ease.OutExpo);
            _alphaTween = canvasGroup.DOFade(1, FadeTime).SetEase(Ease.OutExpo);
            canvasGroup.interactable = true;
        }

        public void HideMenu()
        {
            KillMenuTweens();

            _menuShown = false;
            _moveTween = Menu.transform.DOLocalMove(StartPos, FadeTime / 2).SetEase(Ease.OutExpo);
            canvasGroup.interactable = false;
            _alphaTween = canvasGroup.DOFade(0, FadeTime / 2).SetEase(Ease.OutExpo);
        }
    }
}