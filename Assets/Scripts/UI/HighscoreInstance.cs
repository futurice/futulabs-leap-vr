using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HighscoreInstance : MonoBehaviour 
{

	[SerializeField] private Text _scoreText;
	[SerializeField] private Text _positionText;

	public void SetText(int position, int score)
	{
		_positionText.text = string.Format("{0}.", position.ToString("00"));
		_scoreText.text = score.ToString();
	}
}
