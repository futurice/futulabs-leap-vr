using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using System.Linq;

namespace Futulabs
{
	public class Highscore
	{
		public int Score;
		public Texture2D Selfie;
		public Highscore(int score, Texture2D selfie)
		{
			Score = score;
			Selfie = selfie;
		}

		public Highscore(int score)
		{
			Score = score;
		}

		public Highscore()
		{

		}
	}

	public static class HighscoreManager 
	{
		private const int _maxScores = 10;
		public static readonly BehaviorSubject<List<Highscore>> HighScores = new BehaviorSubject<List<Highscore>>(new List<Highscore>());
		private const string _highScoreKey = "futuVrHighscore";
		private static WebCamTexture wct;
		private static Texture2D a;

		public static void LoadHighscores()
		{
			HighScores.Value.Clear();
			for(int i = 0; i < _maxScores; i++)
			{
				var highscore = new Highscore();
				var score = PlayerPrefs.GetInt(string.Format("{0}_{1}", _highScoreKey, i), -1);
				if(score != -1)
				{
					highscore.Score = score;
					HighScores.Value.Add(highscore);
				}
			}
		}

		public static void TakeSelfie()
		{
			WebCamDevice[] devices = WebCamTexture.devices;
			var deviceName = devices[1].name; // Baaaad!!!
			wct = new WebCamTexture(deviceName, 400, 300, 12);
			wct.Play();
			Observable.TimerFrame(1, FrameCountType.EndOfFrame).Subscribe(_ => 
			{
				Texture2D snap = new Texture2D(wct.width, wct.height);
				snap.SetPixels(wct.GetPixels());
				snap.Apply();
				System.IO.File.WriteAllBytes(@"D:\Selfie.png", snap.EncodeToPNG());
			});
		}

		public static void SaveHighscores()
		{
			for(int i = 0; i < HighScores.Value.Count; i++)
			{
				PlayerPrefs.SetInt(string.Format("{0}_{1}", _highScoreKey, i), HighScores.Value[i].Score);
			}
		}
		
		public static void TryAddHighscore(Highscore highscore)
		{
			if (highscore.Score == 0)
			{
				AudioManager.Instance.PlayAudioClip(GameAudioClipType.HAHA_LAUGH);
				return;
			} 

			var maxScore = HighScores.Value.Count > 0 ? HighScores.Value.Max(x => x.Score) : 0;
			var scoreToBeat = HighScores.Value.Count > 0 ? HighScores.Value.Min(x => x.Score) : 0;

			if (HighScores.Value.Count >= _maxScores)
			{
				if (highscore.Score > scoreToBeat && highscore.Score <= maxScore)
				{
					AudioManager.Instance.PlayAudioClip(GameAudioClipType.CLAP_TRACK);														
					HighScores.Value.Remove(HighScores.Value.FirstOrDefault(x => x.Score == highscore.Score));
					HighScores.Value.Add(highscore);										
				}

				if (highscore.Score > maxScore)
				{
					AudioManager.Instance.PlayAudioClip(GameAudioClipType.CHEERING);					
					HighScores.Value.Remove(HighScores.Value.Find(x => x.Score == maxScore));				
					HighScores.Value.Add(highscore);					
				}
			}
			else
			{
				if (highscore.Score > maxScore)
				{
					AudioManager.Instance.PlayAudioClip(GameAudioClipType.CHEERING);
				}
				else 
				{
					AudioManager.Instance.PlayAudioClip(GameAudioClipType.CLAP_TRACK);									
				}
				HighScores.Value.Add(highscore);
			}

			HighScores.OnNext(HighScores.Value);

		}

	}
}