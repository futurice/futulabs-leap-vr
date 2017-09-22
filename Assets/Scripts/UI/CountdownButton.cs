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
        private const int _countDownTime = 20;
        private float dt;

        public GameManager game;
        public Image RadialImage;

		private IDisposable _countDownDisposable;
		private IDisposable _tryAddHighscoreDisposable;

        public void TimerStart()
        {
			EmptyTimer();
			game.CountDownPercentage.Subscribe(percent => 
			{
				RadialImage.fillAmount = Mathf.Max(0, 1 - percent);
			});
			game.ResetBasketScore();
        }

		private void EmptyTimer() 
		{
			RadialImage.fillAmount = 1;
			game.Countdown(_countDownTime);
			if(_tryAddHighscoreDisposable != null)
			{
				_tryAddHighscoreDisposable.Dispose();
			}
			_tryAddHighscoreDisposable = Observable.Timer(TimeSpan.FromSeconds(_countDownTime)).TakeUntilDestroy(this).Subscribe(_ =>
			{
				HighscoreManager.TryAddHighscore(new Highscore(game.CurrentBasketScore));
			});
		}
    }
}
