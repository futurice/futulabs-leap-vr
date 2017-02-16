using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futulabs
{
    public class ChilicornEasteregg : MonoBehaviour
    {
        private bool _activated = false;
        private Color _originalEmissionColor;
        [Tooltip("The time it should take to 'go through the rainbow'")]
        [SerializeField]
        private float _spectrumTime;

        [SerializeField]
        private Material _floorMaterial;
        private float _hue;
        [SerializeField]
        private float _saturation;
        [SerializeField]
        private float _value;

        private float _dt = 0;

        private void Start()
        {
            _originalEmissionColor = _floorMaterial.GetColor("_EmissionColor");
        }

        private void OnCollisionEnter(Collision collision)
        {
            //Check if it's a star. Only stars!
            var star = collision.gameObject.GetComponent<InteractableStarController>();
            if (star == null)
                return;
            if (_activated)
                DeactivateEasteregg();
            else
                ActivateEasteregg();
        }

        private void Update()
        {
            if (_activated)
            {
                _dt += Time.deltaTime;
                _hue = _dt / _spectrumTime;
                if (_dt >= _spectrumTime)
                    _dt = 0;
                Debug.Log(_hue);
                Color emissiveFloorColor = Color.HSVToRGB(_hue, _saturation, _value);
                _floorMaterial.SetColor("_EmissionColor", emissiveFloorColor);
            }
        }

        void ActivateEasteregg()
        {
            _dt = 0;
            _activated = true;
        }

        void DeactivateEasteregg()
        {
            _activated = false;
            _floorMaterial.SetColor("_EmissionColor", _originalEmissionColor);
        }
    }
}