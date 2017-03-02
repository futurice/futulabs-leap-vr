using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
namespace Futulabs
{
    public class TimeButton : MonoBehaviour
    {
        public float TimeToActivate = 3f;
        private float dt = 0;

        public GameManager game;
        public Image RadialImage;
        private bool isDown;

        public void ButtonDown()
        {
            isDown = true;
			AudioManager.Instance.PlayAudioClip(GameAudioClipType.BUTTON_LOAD);
        }

        public void ButtonUp()
        {
            isDown = false;
            RadialImage.DOFillAmount(0, 0.5f).SetEase(Ease.OutExpo);
			AudioManager.Instance.StopAudioClip();
        }

        private void Update()
        {
            if (isDown)
			{
                dt += Time.deltaTime;
			}
			else
			{
                dt = 0;
			}

			if (dt == 0)
			{
                return;
			}

            float percentage = dt / TimeToActivate;
            
			RadialImage.fillAmount = percentage;

            if (dt >= TimeToActivate)
            {
                Activate();
            }
        }

        private void Activate()
        {
            game.ResetGame();
        }
    }
}
