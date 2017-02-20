using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlasmaCutEffect : MonoBehaviour
{

    private Material _material;
    [SerializeField]
    private MeshRenderer _renderer;
    [SerializeField]
    private float _animateTime = 0.2f;
    void Start()
    {
        _material = _renderer.material;
        AnimateToDeath();
    }

    void AnimateToDeath()
    {
        Color tint = _material.GetColor("_TintColor");
        tint.a = 0;
        _material.DOColor(tint, "_TintColor", _animateTime).SetEase(Ease.OutExpo).OnComplete(Death);
        _material.DOFloat(0, "_EmissionGain", _animateTime).SetEase(Ease.OutExpo);
    }

    void Death()
    {
        Destroy(gameObject);
    }
}
