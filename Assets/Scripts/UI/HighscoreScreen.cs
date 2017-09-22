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
			HighscoreManager.HighScores.TakeUntilDestroy(this).Subscribe(scores =>
			{
				InitializeHighscores(scores);
			});
		}

		private bool _highscoreDeleteToggling = false;

		void Update()
		{
			if(Input.GetKey(KeyCode.H) && Input.GetKey(KeyCode.S) && !_highscoreDeleteToggling)
			{
				_highscoreDeleteToggling = true;
				PlayerPrefs.DeleteAll();
				Debug.Log("Highscores deleted");
				Start();
			}
			if(!Input.GetKey(KeyCode.H) && !Input.GetKey(KeyCode.S))
			{
				_highscoreDeleteToggling = false;
			}
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

			_instances.Clear();

			scores = scores
				.OrderBy(x => x.Score)
				.Reverse()
				.ToList();
			
			for(int i = 0; i < scores.Count; i++)
			{
				var instance = Instantiate(_highscoreInstancePrefab) as HighscoreInstance;
				instance.gameObject.transform.SetParent(_highscoresRoot);
				instance.transform.localScale = Vector3.one;
				instance.transform.localRotation = Quaternion.identity;
				instance.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0,0,-1f);
				instance.Init(scores[i], i+1);
				_instances.Add(instance);
			}
		}
	}
}