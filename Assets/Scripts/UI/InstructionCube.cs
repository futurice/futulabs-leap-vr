using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using DG.Tweening;
namespace Futulabs
{
	public class InstructionCube : MonoBehaviour 
	{
		private float _hiddenPosition = 0f;
		private float _shownPosition = 0.125f;

		[SerializeField] private List<MeshRenderer> _sideCubes = new List<MeshRenderer>();
		private float _minEmission = 0.15f;
		private float _maxEmission = 0.4f;
		[SerializeField] private Color _emissionColor = Color.white;

		[SerializeField] private CanvasGroup _canvasGroup;
		[SerializeField] private float _minAlpha = 0.25f;

		[SerializeField] private Animator _instructionAnimator;
		[SerializeField] private float _animationMaxSpeed;
		private float _animSpeed;
		private Tweener _currentAnimSpeedTween;

		private const float _animationTime = 0.3f;

		private void Start()
		{
			HideInstruction(true);
		}

		private void KillTweens()
		{
			if(_currentAnimSpeedTween != null)
			{
				_currentAnimSpeedTween.Kill();
			}
			_canvasGroup.DOKill();
			transform.DOKill();
			foreach(var c in _sideCubes)
			{
				c.material.DOKill();
			}
		}

		public void ShowInstruction(bool instant = false)
		{
			KillTweens();
			FireTweens(_animationMaxSpeed, 1, _shownPosition, _maxEmission, instant);
		}

		public void HideInstruction(bool instant = false)
		{
			KillTweens();
			FireTweens(0, _minAlpha, _hiddenPosition, _minEmission, instant);
		}

		private void FireTweens(float animSpeed, float alpha, float cubePosition, float emission, bool instant = false)
		{
			DOTween.To(x=> _animSpeed = x, _animSpeed, animSpeed, instant? 0 : _animationTime).OnUpdate(() =>
			{
				_instructionAnimator.SetFloat("speed", _animSpeed);
			});
			_canvasGroup.DOFade(alpha, instant? 0 :_animationTime);
			transform.DOLocalMoveX(cubePosition, instant? 0 : _animationTime);
			foreach(var c in _sideCubes)
			{
				c.material.SetColor("_EmissionColor", _emissionColor);
				if(instant)
				{
					c.material.SetFloat("_EmissionGain", emission);
				}
				else
				{
					c.material.DOFloat(emission, "_EmissionGain", _animationTime);
				}
			}
		}
	}
}