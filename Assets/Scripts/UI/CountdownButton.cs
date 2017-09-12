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
        private float dt;

        public GameManager game;
        public Image RadialImage;

		private IDisposable _countDownDisposable;

        public void TimerStart()
        {
			EmptyTimer();
        }

		private void EmptyTimer() 
		{
			RadialImage.fillAmount = 1;
			Func<long, bool> f = (x) => RadialImage.fillAmount > 0;
			var dtMaxSec = -1;
			dt = 0;
			if(_countDownDisposable != null)
			{
				_countDownDisposable.Dispose();
			}
           	_countDownDisposable = Observable.EveryUpdate().TakeWhile(f).Subscribe(_ =>
            {
                dt += Time.deltaTime;
				var percentage = dt/_countDownTime;
				if (RadialImage.fillAmount > 0) 
				{
					var timeLeft = Mathf.RoundToInt(_countDownTime - dt);
					var dtInt = Mathf.RoundToInt(dt);
					if(dtInt > dtMaxSec)
					{
						dtMaxSec = dtInt;
						if (dtInt % 2 == 0) 
						{
							AudioManager.Instance.PlayAudioClip(GameAudioClipType.CLOCK_TICK);
						}
						else
						{
							AudioManager.Instance.PlayAudioClip(GameAudioClipType.CLOCK_TOCK);
						}
					}
					game.SetTimer(timeLeft);
					RadialImage.fillAmount = 1 - percentage;
				}
            });
		}
    }
}
