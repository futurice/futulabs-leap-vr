using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using DG.Tweening;

public class InstructionCube : MonoBehaviour 
{
	[SerializeField] private Animator _instructionAnimator;
	[SerializeField] private float _animationMaxSpeed;
	private float _animSpeed;
	private Tweener _currentAnimSpeedTween;

	private void Start()
	{
		ShowInstruction();
	}

	private void KillTweens()
	{
		if(_currentAnimSpeedTween != null)
		{
			_currentAnimSpeedTween.Kill();
		}
	}

	public void ShowInstruction()
	{
		KillTweens();
		_currentAnimSpeedTween = DOTween.To(x=> _animSpeed = x, 0, _animationMaxSpeed, 0.3f).OnUpdate(() =>
		{
			_instructionAnimator.SetFloat("speed", _animSpeed);
		});
	}

	public void HideInstruction()
	{
		KillTweens();
		DOTween.To(x=> _animSpeed = x, _animSpeed, 0, 0.3f).OnUpdate(() =>
		{
			_instructionAnimator.SetFloat("speed", _animSpeed);
		});
	}
}
