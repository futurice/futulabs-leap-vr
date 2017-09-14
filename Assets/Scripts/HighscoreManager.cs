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

		public static void LoadHighscores()
		{
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

		public static void SaveHighscores()
		{
			for(int i = 0; i < HighScores.Value.Count; i++)
			{
				PlayerPrefs.SetInt(string.Format("{0}_{1}", _highScoreKey, i), HighScores.Value[i].Score);
			}
		}

		public static void AddFakeHighScores()
		{
			Enumerable
				.Range(0, _maxScores)
				.ToList()
				.ForEach(x => HighScores.Value.Add(new Highscore(x)));
		}

		public static void TryAddHighscore(Highscore highscore)
		{
			/*var scoreToBeat = 0;
			var highestScore = 1;
			var maxScoresReached = HighScores.Value.Count >= _maxScores;

			if (HighScores.Value.Count > 0) 
			{
				scoreToBeat = HighScores.Value.Min(x => x.Score);
				highestScore = HighScores.Value.Max(x => x.Score);
			}

			if (highscore.Score == 0) 
			{
				// laugh track, haha!
				AudioManager.Instance.PlayAudioClip(GameAudioClipType.HAHA_LAUGH);
				Debug.Log("Sorry, but you are terrible");
			}

			if (highscore.Score > scoreToBeat && highscore.Score <= highestScore)
			{
				// clap track, olé
				AudioManager.Instance.PlayAudioClip(GameAudioClipType.CLAP_TRACK);
				Debug.Log("Not so bad");
				if (maxScoresReached)
				{
					HighScores.Value.Remove(HighScores.Value.FirstOrDefault(x => x.Score == scoreToBeat));
				}
				HighScores.Value.Add(highscore);
			}

			if (highscore.Score > highestScore)
			{
				// cheer track, hurra!
				AudioManager.Instance.PlayAudioClip(GameAudioClipType.CHEERING);
				Debug.Log("You are awesome");
				if (maxScoresReached) 
				{
					HighScores.Value.Remove(HighScores.Value.FirstOrDefault(x => x.Score == highestScore));
				}
				HighScores.Value.Add(highscore);
			}

			HighScores.OnNext(HighScores.Value);*/
			var minHighscore = HighScores.Value.Count > 0? HighScores.Value.Min(x => x.Score) : 0;
			bool maxScoresReached = HighScores.Value.Count >= _maxScores;
			bool minScoreBeat = highscore.Score > minHighscore;

			if (highscore.Score == 0) 
			{
				AudioManager.Instance.PlayAudioClip(GameAudioClipType.HAHA_LAUGH);
				Debug.Log("Sorry, but you are terrible");
			}

			if(maxScoresReached && !minScoreBeat)
			{
				Debug.Log("Didn't add highscore - max scores reached and didn't beat min score");
				return;
			}
			if(minScoreBeat && maxScoresReached)
			{
				AudioManager.Instance.PlayAudioClip(GameAudioClipType.CLAP_TRACK);				
				Debug.Log("Beat min highscore and max scores reached");
				HighScores.Value.Remove(HighScores.Value.FirstOrDefault(x => x.Score == minHighscore));
			}
			if(HighScores.Value.Count > 0)
			{
				var maxScore = HighScores.Value.Count > 0 ? HighScores.Value.Max(x => x.Score) : 0;
				if( highscore.Score > maxScore)
				{
					AudioManager.Instance.PlayAudioClip(GameAudioClipType.CHEERING);									
					Debug.Log("Beat max highscore");
				}
			}
			HighScores.Value.Add(highscore);
			HighScores.OnNext(HighScores.Value);
		}

	}
}