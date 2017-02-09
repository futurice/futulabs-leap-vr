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

        private int _currentInstructionIndex = 0;
        private float _lastInstructionChangeTime = 0.0f;
        private bool _isGravityOn = true;

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

        private void Awake()
        {
            // Change to first instruction
            ChangeToInstruction(0, true);
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
            int newInstructionIndex = (_currentInstructionIndex+1) % _instructionTexts.Count;
            ChangeToInstruction(newInstructionIndex);
        }

        private void ChangeToInstruction(int newInstructionIndex, bool immediate =false)
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
    }

}