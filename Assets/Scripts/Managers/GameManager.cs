using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using System;

namespace Futulabs
{

    public class GameManager : Singleton<GameManager>
    {
        [Header("GameManager")]
        [Header("Instructions")]
        [SerializeField]
        private Text _instructionsText = null;

        [Header("Wall Cube Outline Colors")]
        [SerializeField]
        private LeverController _wallCubeLightsLever;
        [SerializeField]
        private Material _wallCubeEmissiveOutlineMaterial;

        [Header("Score Board")]
        [SerializeField]
        private Text _scoreboardTextOnes;
        [SerializeField]
        private Text _scoreboardTextTens;

        [Header("Timer")]
        [SerializeField]
        private Text _timerTextOnes;
        [SerializeField]
        private Text _timerTextTens;

        private int _currentInstructionIndex = 0;
        private float _lastInstructionChangeTime = 0.0f;
        private bool _isGravityOn = true;
        private int _basketballScore = 0;
        public int CurrentBasketScore 
        {
            get 
            {
                return _basketballScore;
            }
        }

        private bool _stickyCubes = false;
        public bool StickyCubes { get { return _stickyCubes; } }

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

            //Tween sticky outline
            SettingsManager.Instance.StickyOutlineMaterial.SetFloat("_EmissionGain", SettingsManager.Instance.StickyMaterialMaxEmissionGain);
            SettingsManager.Instance.StickyOutlineMaterial.DOFloat(SettingsManager.Instance.StickyMaterialMinEmissionGain, "_EmissionGain", 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        private void Update()
        {
            if (Time.time - _lastInstructionChangeTime > SettingsManager.Instance.InstructionChangeInterval)
            {
                ChangeToNextInstruction();
            }
        }

        private void ChangeToNextInstruction()
        {
            int newInstructionIndex = (_currentInstructionIndex + 1) % SettingsManager.Instance.InstructionTexts.Count;
            ChangeToInstruction(newInstructionIndex);
        }

        private void ChangeToInstruction(int newInstructionIndex, bool immediate = false)
        {
            if (SettingsManager.Instance == null || SettingsManager.Instance.InstructionTexts == null || SettingsManager.Instance.InstructionTexts.Count == 0 || _instructionsText == null)
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
                    _instructionsText.text = SettingsManager.Instance.InstructionTexts[_currentInstructionIndex];
                });
                changeSequence.Append(_instructionsText.DOFade(1.0f, 0.5f));
                changeSequence.Play();
            }
            else
            {
                _instructionsText.text = SettingsManager.Instance.InstructionTexts[_currentInstructionIndex];
            }
        }

        private void SetCubeLightsOn(bool immediate = false)
        {
            if (immediate)
            {
                _wallCubeEmissiveOutlineMaterial.SetColor("_EmissionColor", SettingsManager.Instance.WallCubeOutlineOnEmissionColor);
            }
            else
            {
                _wallCubeEmissiveOutlineMaterial.DOColor(SettingsManager.Instance.WallCubeOutlineOnEmissionColor, "_EmissionColor", 0.7f);
            }
        }

        private void SetCubeLightsOff(bool immediate = false)
        {
            if (immediate)
            {
                _wallCubeEmissiveOutlineMaterial.SetColor("_EmissionColor", SettingsManager.Instance.WallCubeOutlineOffEmissionColor);
            }
            else
            {
                _wallCubeEmissiveOutlineMaterial.DOColor(SettingsManager.Instance.WallCubeOutlineOffEmissionColor, "_EmissionColor", 0.7f);
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

        void OnApplicationQuit()
        {
            HighscoreManager.SaveHighscores();
        }

        public void AddToBasketScore()
        {
            _basketballScore = (_basketballScore + 1) % 100;

            int ones = _basketballScore % 10;
            int tens = _basketballScore / 10;

            _scoreboardTextOnes.text = ones.ToString();
            _scoreboardTextTens.text = tens.ToString();

            AudioManager.Instance.PlayAudioClip(GameAudioClipType.BASKETBALL_SCORE);
        }

        public void ResetBasketScore() 
        {
            _basketballScore = 0;
            _scoreboardTextOnes.text = "0";
            _scoreboardTextTens.text = "0";
        }

        public void SetTimer(int time)
        {
            int ones = time % 10;
            int tens = time / 10;
            _timerTextOnes.text = ones.ToString();
            _timerTextTens.text = tens.ToString();
        }

        public void ToggleStickyCubes()
        {
            _stickyCubes = !_stickyCubes;
        }
    }

}