using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System.Linq;
namespace Futulabs
{
	public class HighscoreScreen : MonoBehaviour 
	{
		[SerializeField] private HighscoreInstance _highscoreInstancePrefab;
		[SerializeField] private RectTransform _highscoresRoot;

		private List<HighscoreInstance> _instances = new List<HighscoreInstance>();
		void Start()
		{
			HighscoreManager.LoadHighscores();
			HighscoreManager.HighScores.Subscribe(scores =>
			{
				InitializeHighscores(scores);
			});
		}

		void InitializeHighscores(List<Highscore> scores)
		{
			if(scores == null || scores.Count == 0)
			{
				return;
			}
			foreach(var h in _instances)
			{
				Destroy(h.gameObject);
			}

			scores.OrderBy(x => x.Score);
			scores.Reverse();
			
			for(int i = 0; i < scores.Count; i++)
			{
				var instance = Instantiate(_highscoreInstancePrefab) as HighscoreInstance;
				instance.gameObject.transform.SetParent(_highscoresRoot);
				instance.transform.localScale = Vector3.one;
				instance.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,-1f);
				instance.SetText(i+1, scores[i].Score);
				_instances.Add(instance);
			}
		}
	}
}