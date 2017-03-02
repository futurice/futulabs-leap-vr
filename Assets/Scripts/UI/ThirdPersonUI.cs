using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Futulabs
{
    public class ThirdPersonUI : MonoBehaviour
    {
        private bool _textEnabled = true;
        /*[SerializeField]
        private float _textMoveLength = 300f;
        [SerializeField]
        private float _textMoveSpeed = 1f;*/
        [SerializeField]
        private float _textFadeTime = 0.5f;
        [SerializeField]
        private Text _text;

        // Use this for initialization
        void Start()
        {
            //TODO: Animate text?
        }

        // Update is called once per frame
        void Update()
        {
            /* if (_textEnabled)
             {
                 if (Input.GetAxis("Horizontal") != 0 || Input.GetButton("Space"))
                 {
                     FadeText();
                     _textEnabled = false;
                 }
             }*/
        }

        private void FadeText()
        {
            _text.DOFade(0, _textFadeTime);
        }

    }
}
