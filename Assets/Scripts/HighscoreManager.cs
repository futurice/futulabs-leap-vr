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
		public static readonly ReactiveProperty<List<Highscore>> HighScores = new ReactiveProperty<List<Highscore>>();
		private const string _highScoreKey = "futuVrHighscore";
		public static void LoadHighscores()
		{
			HighScores.Value = new List<Highscore>();
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

		public static void TryAddHighscore(Highscore highscore)
		{
			var minHighscore = HighScores.Value.Count > 0? HighScores.Value.Min(x => x.Score) : 0;
			bool maxScoresReached = HighScores.Value.Count >= _maxScores;
			bool minScoreBeat = highscore.Score > minHighscore;
			if(maxScoresReached && !minScoreBeat)
			{
				Debug.Log("Didn't add highscore - max scores reached and didn't beat min score");
				return;
			}
			if(minScoreBeat && maxScoresReached)
			{
				Debug.Log("Beat min highscore and max scores reached");
				HighScores.Value.Remove(HighScores.Value.FirstOrDefault(x => x.Score == minHighscore));
			}
			if(HighScores.Value.Count > 0)
			{
				var maxScore = HighScores.Value.Max(x => x.Score);
				if( highscore.Score > maxScore)
				{
					Debug.Log("Beat max highscore");
				}
			}
			HighScores.Value.Add(highscore);
		}

	}
}