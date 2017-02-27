using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;
using RootMotion.FinalIK;

namespace Futulabs
{ 
    public class IKLeapHandController : LeapHandController
    {
        [SerializeField]
        protected IK _ikController;
        protected IKSolver _ikSolver;

        [SerializeField]
        protected HandModel _leftHand;
        [SerializeField]
        protected HandModel _rightHand;

        protected virtual void Awake()
        {
            _ikSolver = _ikController.GetIKSolver();
        }

        override protected void OnUpdateFrame(Frame frame)
        {
            if (frame != null && graphicsEnabled)
            {
                UpdateHandRepresentations(graphicsReps, ModelType.Graphics, frame);

                if (!_ikSolver.initiated) InitiateSolver();
                if (!_ikSolver.initiated) return;

                Vector3 leftPos = _leftHand.transform.position;
                Vector3 rightPos = _rightHand.transform.position;

                _ikSolver.Update();
            }
        }

        override protected void OnFixedFrame(Frame frame)
        {
            if (frame != null && physicsEnabled)
            {
                UpdateHandRepresentations(physicsReps, ModelType.Physics, frame);
            }
        }

        private void InitiateSolver()
        {
            if (_ikSolver.initiated) return;
            _ikSolver.Initiate(_ikController.transform);
        }
    }
}