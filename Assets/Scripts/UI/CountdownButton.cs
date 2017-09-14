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
        private const float _countDownTime = 15f;
        private float dt;

        public GameManager game;
        public Image RadialImage;

		private IDisposable _countDownDisposable;

        public void TimerStart()
        {
			EmptyTimer();
			game.ResetBasketScore();
        }

		private void EmptyTimer() 
		{
			RadialImage.fillAmount = 1;
			var dtMaxSec = -1;
			dt = 0;
			Func<long, bool> f = (x) => _countDownTime - dt > 0;
			if(_countDownDisposable != null)
			{
				_countDownDisposable.Dispose();
			}
           	_countDownDisposable = Observable.EveryUpdate().TakeWhile(f).Subscribe(_ =>
            {
                dt += Time.deltaTime;
				var timeLeft = Mathf.RoundToInt(_countDownTime - dt);
				var dtInt = Mathf.RoundToInt(dt);
				if(dtInt > dtMaxSec)
				{
					dtMaxSec = dtInt;
					AudioManager.Instance.PlayAudioClip(dtInt % 2 == 0 ? GameAudioClipType.CLOCK_TICK : GameAudioClipType.CLOCK_TOCK);
				}
				game.SetTimer(timeLeft);
				var percentage = dt/_countDownTime;
				RadialImage.fillAmount = Mathf.Max(0, 1 - percentage);
            });

			Observable.Timer(TimeSpan.FromSeconds(_countDownTime)).Subscribe(_ =>
			{
				HighscoreManager.TryAddHighscore(new Highscore(game.CurrentBasketScore));
			});
		}
    }
}
