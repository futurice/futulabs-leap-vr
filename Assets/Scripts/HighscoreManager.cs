using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using System.Linq;
using System.IO;

namespace Futulabs
{
	public class Highscore
	{
		public int Score;
		public ReactiveProperty<string> Selfie = new ReactiveProperty<string>();

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
		private const int _maxScores = 5;
		public static readonly BehaviorSubject<List<Highscore>> HighScores = new BehaviorSubject<List<Highscore>>(new List<Highscore>());
		private const string _highScoreKey = "futuVrHighscore";
		private const string _highScoreImagePath = @"D:\FutuVRSelfies\";
		private static WebCamTexture wct;

		public static void TakeSelfie(Highscore score)
		{
			WebCamDevice[] devices = WebCamTexture.devices;
			var deviceName = devices[0].name; // Sometimes device 1 is the leap motion, sometimes it isn't
			wct = new WebCamTexture(deviceName, 512, 512, 60);
			wct.Play();
			Observable.TimerFrame(60, FrameCountType.EndOfFrame).Subscribe(_ => 
			{
				var snap = new Texture2D(wct.width, wct.height);
				snap.SetPixels(wct.GetPixels());
				snap.Apply();
				var randomName = Path.GetRandomFileName();
				var filePath = $"{_highScoreImagePath}{randomName}.png"; 
				File.WriteAllBytes(filePath, snap.EncodeToPNG());
				wct.Stop();
				score.Selfie.Value = filePath;
			});
		}

		public static void LoadHighscores()
		{
			HighScores.Value.Clear();
			for(int i = 0; i < _maxScores; i++)
			{
				var highscore = new Highscore();
				var score = PlayerPrefs.GetInt($"{_highScoreKey}_{i}", -1);
				var selfiePath = PlayerPrefs.GetString($"{_highScoreKey}_{i}_selfiePath", string.Empty);
				Debug.Log(i + " - " + score);
				if(score != -1)
				{
					highscore.Score = score;
					highscore.Selfie.Value = selfiePath;
					HighScores.Value.Add(highscore);
				}
			}
		}

		public static void SaveHighscores()
		{
			for(int i = 0; i < HighScores.Value.Count; i++)
			{
				PlayerPrefs.SetInt($"{_highScoreKey}_{i}", HighScores.Value[i].Score);
				if(!string.IsNullOrEmpty(HighScores.Value[i].Selfie.Value))
				{
					PlayerPrefs.SetString($"{_highScoreKey}_{i}_selfiePath", HighScores.Value[i].Selfie.Value);
				}	
			}
		}

		private static void AddScore(Highscore score, bool beatMax)
		{
			AudioManager.Instance.PlayAudioClip(beatMax? GameAudioClipType.CHEERING : GameAudioClipType.CLAP_TRACK);
			HighScores.Value.Add(score);
			int time = 5;
			GameManager.Instance.Countdown(time);
			Observable.Timer(TimeSpan.FromSeconds(time)).Subscribe(_ =>
			{
				TakeSelfie(score);	
			});		
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
				if (highscore.Score > scoreToBeat)
				{
					HighScores.Value.Remove(HighScores.Value.FirstOrDefault(x => x.Score == scoreToBeat));					
					if (highscore.Score > maxScore)
					{
						AddScore(highscore, false);										
					} 
					else 
					{
						AddScore(highscore, false);				
					}
				}
			}
			else
			{
				AddScore(highscore, highscore.Score > maxScore);
			}
			HighScores.OnNext(HighScores.Value);

		}

	}
}