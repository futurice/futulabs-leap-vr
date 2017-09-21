using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.IO;

namespace Futulabs
{
	public class HighscoreInstance : MonoBehaviour 
	{

		[SerializeField] private Text _scoreText;
		[SerializeField] private Text _positionText;
		[SerializeField] private RawImage _selfieImage;

		public void Init(Highscore score, int position)
		{
			_positionText.text = position.ToString();
			_scoreText.text = score.Score.ToString();
			
			score.Selfie.Subscribe(path =>
			{
				if(!string.IsNullOrEmpty(path))
				{
					Texture2D tex = null;
					byte[] fileData;
				
					if (File.Exists(path))
					{
						fileData = File.ReadAllBytes(path);
						tex = new Texture2D(512, 512);
						tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
						if(tex != null)
						{
							_selfieImage.texture = tex;
							_selfieImage.gameObject.SetActive(true);
						}
					}
				}
			});
		}
	}
}