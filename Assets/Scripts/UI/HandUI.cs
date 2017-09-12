using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
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

        [SerializeField] private MultiToggle _multiToggle;

        [Header("Buttons")]
        [SerializeField] private Button _buttonBox;
        [SerializeField] private Button _buttonIcosphere;
        [SerializeField] private Button _buttonStar;
        [SerializeField] private Button _buttonGravity;
        [SerializeField] private Button _buttonSticky;
        [SerializeField] private Button _buttonTimer;

        private void Awake()
        {
            HideMenu();
            InitButtons();
        }

        private void InitButtons()
        {
            _buttonBox.onClick.AddListener(() => GameManager.Instance.ChangeCreatedInteractableObjectType(0));
            _buttonIcosphere.onClick.AddListener(() => GameManager.Instance.ChangeCreatedInteractableObjectType(1));
            _buttonStar.onClick.AddListener(() => GameManager.Instance.ChangeCreatedInteractableObjectType(2));

            _buttonBox.onClick.AddListener( () => _multiToggle.ChangeButton(0));
            _buttonIcosphere.onClick.AddListener( () => _multiToggle.ChangeButton(1));
            _buttonStar.onClick.AddListener( () => _multiToggle.ChangeButton(2));

            _buttonGravity.onClick.AddListener(() => GameManager.Instance.ToggleGravity());
            _buttonGravity.onClick.AddListener(() => _buttonGravity.GetComponent<ToggleOutline>().Toggle());

            _buttonSticky.onClick.AddListener(() => GameManager.Instance.ToggleStickyCubes());
            _buttonSticky.onClick.AddListener(() => _buttonSticky.GetComponent<ToggleOutline>().Toggle());

            _buttonTimer.onClick.AddListener(() => _buttonTimer.GetComponent<CountdownButton>().TimerStart());

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

			_alphaTween = canvasGroup
				.DOFade(1, FadeTime).SetEase(Ease.OutExpo)
				.OnComplete(() => {
					AudioManager.Instance.PlayAudioClip(GameAudioClipType.MENU_APPEAR);
				});
			
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