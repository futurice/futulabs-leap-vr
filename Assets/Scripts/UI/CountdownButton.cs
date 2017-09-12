using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

namespace Futulabs
{
    public class CountdownButton : MonoBehaviour
    {
        private const float _countDownTime = 10f;
        private float dt = 0;

        public GameManager game;
        public Image RadialImage;

        public void TimerStart()
        {
			AudioManager.Instance.PlayAudioClip(GameAudioClipType.BUTTON_LOAD);
			EmptyTimer();
        }

		private void EmptyTimer() 
		{
			RadialImage.fillAmount = 1;
			Func<long, bool> f = (x) => RadialImage.fillAmount > 0;
            Observable.EveryUpdate().TakeWhile(f).Subscribe(_ =>
            {
                dt += Time.deltaTime;
				var percentage = dt/_countDownTime;
				if (RadialImage.fillAmount > 0) 
				{
					var timeLeft = Mathf.RoundToInt(_countDownTime - dt);
					game.SetTimer(timeLeft);
					RadialImage.fillAmount = 1 - percentage;
				}
            });
		}
    }
}
