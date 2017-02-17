using UnityEngine;
using System.Collections;
using UniRx;
using System.Collections.Generic;
using Leap.Unity.Interaction;

namespace Futulabs
{
    public class BladeCutter : MonoBehaviour
    {
        [SerializeField]
        private Material _capMaterial;
        [SerializeField]
        private float _cutCooldown = 0.5f;

        private float _lastCutTime;

        private void OnCollisionEnter(Collision collision)
        {
            if (enabled && 
                collision.gameObject.CompareTag("InteractableObject") &&
                Time.time > _lastCutTime + _cutCooldown)
            {
                Vector3 anchorPoint = collision.contacts[0].point;
                InteractableObjectControllerBase interactableObject = collision.gameObject.GetComponentInParent<InteractableObjectControllerBase>();               
                GameObject rightHalf = new GameObject(interactableObject.transform.name);
                InteractableObjectControllerBase rightHalfInteractableObject = rightHalf.AddComponent(interactableObject.GetType()) as InteractableObjectControllerBase;
                rightHalf.transform.SetParent(ObjectManager.Instance.ObjectContainer, true);
                rightHalf.transform.localScale = Vector3.Scale(interactableObject.transform.parent.localScale, interactableObject.transform.localScale);
                rightHalf.tag = "InteractableObject";

				CutMesh(
                    interactableObject.SolidMeshGameObject,
                    anchorPoint,
                    interactableObject.transform,
                    rightHalf.transform,
                    _capMaterial
				)
				.SelectMany(solidPieces => {
                    // Add interaction behaviour to the solid piece
                    InteractionBehaviour rightSideInteractionBehaviour = solidPieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].gameObject.AddComponent<InteractionBehaviour>();

                    return CutMesh(
	                    interactableObject.OutlineMeshGameObject,
	                    anchorPoint,
	                    solidPieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE].transform,
	                    solidPieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].transform,
						_capMaterial
					);
				})
				.Subscribe (outlinePieces => {
                    // Rebuild the references for the new InteractableObjectController
                    outlinePieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].transform.localPosition = Vector3.zero;
                    outlinePieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].transform.localRotation = Quaternion.identity;
                    rightHalfInteractableObject.RebuildReferences();
				});

                _lastCutTime = Time.time;
            }
        }

        private IObservable<GameObject[]> CutMesh(GameObject target, Vector3 anchorPoint, Transform leftParent, Transform rightParent, Material capMaterial)
        {
            return MeshCut.Cut(target, anchorPoint, transform.up, capMaterial)
				.Select(pieces => {
					GameObject leftSideGameObject = pieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE];
					GameObject rightSideGameObject = pieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE];

					leftSideGameObject.tag = target.tag;
					rightSideGameObject.tag = target.tag;

					leftSideGameObject.transform.SetParent(leftParent, true);
					rightSideGameObject.transform.SetParent(rightParent, true);
					rightSideGameObject.transform.localScale = leftSideGameObject.transform.localScale;
                    
					return pieces;
				});
        }
    }
}