using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace Futulabs
{

    public class GameManager : Singleton<GameManager>
    {
        [Header("GameManager")]
        [Header("Instructions")]
        [SerializeField]
        private Text _instructionsText = null;
        [SerializeField]
        private List<string> _instructionTexts = null;
        [SerializeField]
        private float _instructionChangeInterval = 10.0f;

        [Header("Wall Cube Lights")]
        [SerializeField]
        private LeverController _wallCubeLightsLever;
        [SerializeField]
        private Material _wallCubeEmissiveOutlineMaterial;
        [SerializeField]
        private Color _wallCubeOutlineOffEmissionColor;
        [SerializeField]
        private Color _wallCubeOutlineOnEmissionColor;

        //TODO: Hey let's move all this crap in some settings file (refactor)
        [Header("Interactable Material Outline Colors")]
        [SerializeField]
        private Color _maxEmission;
        [SerializeField]
        private Color _maxDiffuse;
        [SerializeField]
        private float _maxEmissionGain = 0.4f;
        [SerializeField]
        private Color _minEmission;
        [SerializeField]
        private Color _minDiffuse;
        [SerializeField]
        private float _minEmissionGain = 0f;
        [SerializeField]
        private float _outlineTransitionFactor = 3f;
        [SerializeField]
        private float _outlineMinGlowTime = 0.5f;
        [SerializeField]
        private float _outlineMaxGlowTime = 2f;

        public Color MaxEmissionColor { get { return _maxEmission; } }
        public Color MaxDiffuseColor { get { return _maxDiffuse; } }
        public float MaxEmissionGain { get { return _maxEmissionGain; } }
        public Color MinEmissionColor { get { return _minEmission; } }
        public Color MinDiffuseColor { get { return _minDiffuse; } }
        public float MinEmissionGain { get { return _minEmissionGain; } }
        public float OutlineTransitionFactor { get { return _outlineTransitionFactor; } }
        public float OutlineMinGlowTime { get { return _outlineMinGlowTime; } }
        public float OutlineMaxGlowTime { get { return _outlineMaxGlowTime; } }

        [Header("Score Board")]
        [SerializeField]
        private Text _scoreboardTextOnes;
        [SerializeField]
        private Text _scoreboardTextTens;

        private int _currentInstructionIndex = 0;
        private float _lastInstructionChangeTime = 0.0f;
        private bool _isGravityOn = true;

        private int BasketballScore = 0;

        /// <summary>
        /// Returns the object that will be created by the ObjectManager.
        /// </summary>
        public InteractableObjectType CurrentlySelectedObject
        {
            get
            {
                return ObjectManager.Instance.CurrentCreatableObjectType;
            }
        }

        /// <summary>
        /// Returns whether the gravity is on
        /// </summary>
        public bool IsGravityOn
        {
            get
            {
                return _isGravityOn;
            }

            private set
            {
                _isGravityOn = value;
            }
        }

        //public MeshRenderer cubeMeshR;
        private void Awake()
        {
            //  _wallCubeEmissiveOutlineMaterial = cubeMeshR.sharedMaterial;
            // Change to first instruction
            ChangeToInstruction(0, true);

            // Set the lights off at start
            SetCubeLightsOff(true);

            // Register lever callbacks
            _wallCubeLightsLever.OnLeverTurnedOn += () => SetCubeLightsOn();
            _wallCubeLightsLever.OnLeverTurnedOff += () => SetCubeLightsOff();
        }

        private void Update()
        {
            if (Time.time - _lastInstructionChangeTime > _instructionChangeInterval)
            {
                ChangeToNextInstruction();
            }
        }

        private void ChangeToNextInstruction()
        {
            int newInstructionIndex = (_currentInstructionIndex + 1) % _instructionTexts.Count;
            ChangeToInstruction(newInstructionIndex);
        }

        private void ChangeToInstruction(int newInstructionIndex, bool immediate = false)
        {
            if (_instructionTexts == null || _instructionTexts.Count == 0 || _instructionsText == null)
            {
                Debug.LogError("GameManager ChangeToInstruction: Instructions not setup");
                return;
            }

            _currentInstructionIndex = newInstructionIndex;
            _lastInstructionChangeTime = Time.time;

            if (!immediate)
            {
                // Do a fade-in - fade-out sequence when changing
                Sequence changeSequence = DOTween.Sequence();

                changeSequence.Append(_instructionsText.DOFade(0.0f, 0.5f));
                changeSequence.AppendCallback(() =>
                {
                    _instructionsText.text = _instructionTexts[_currentInstructionIndex];
                });
                changeSequence.Append(_instructionsText.DOFade(1.0f, 0.5f));
                changeSequence.Play();
            }
            else
            {
                _instructionsText.text = _instructionTexts[_currentInstructionIndex];
            }
        }

        private void SetCubeLightsOn(bool immediate = false)
        {
            if (immediate)
            {
                _wallCubeEmissiveOutlineMaterial.SetColor("_EmissionColor", _wallCubeOutlineOnEmissionColor);
            }
            else
            {
                _wallCubeEmissiveOutlineMaterial.DOColor(_wallCubeOutlineOnEmissionColor, "_EmissionColor", 0.7f);
            }
        }

        private void SetCubeLightsOff(bool immediate = false)
        {
            if (immediate)
            {
                _wallCubeEmissiveOutlineMaterial.SetColor("_EmissionColor", _wallCubeOutlineOffEmissionColor);
            }
            else
            {
                _wallCubeEmissiveOutlineMaterial.DOColor(_wallCubeOutlineOffEmissionColor, "_EmissionColor", 0.7f);
            }
        }

        public void ChangeCreatedInteractableObjectType(int t)
        {
            InteractableObjectType type = (InteractableObjectType)t;
            Debug.LogFormat("GameManager ChangeCreatedInteractableObjectType: Changing object type to {0}", type);
            ObjectManager.Instance.CurrentCreatableObjectType = type;
        }

        public void ToggleGravity()
        {
            Debug.LogFormat("GameManager ToggleGravity: Changing gravity to: {0}", !IsGravityOn);
            IsGravityOn = !IsGravityOn;
            ObjectManager.Instance.ToggleGravityForInteractableObjects(IsGravityOn);
        }

        public void ResetGame()
        {
            Debug.Log("GameManager ResetGame: Resetting the game");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void BasketScore()
        {
            BasketballScore++;
            if (BasketballScore > 99)
                BasketballScore = 0; //TODO: fix this later if you really want to
            int ones = BasketballScore % 10;
            int tens = BasketballScore / 10;
            _scoreboardTextOnes.text = ones.ToString();
            _scoreboardTextTens.text = tens.ToString();
            AudioManager.Instance.PlayAudioClip(GameAudioClipType.BASKETBALL_SCORE);
        }
    }

}