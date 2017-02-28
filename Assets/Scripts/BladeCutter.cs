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

        [SerializeField]
        private BladeCutterEffects _effects;

        [Tooltip("Minimum object size for cutting")]
        [SerializeField]
        private float _minimumCutSize;

        private void OnTriggerEnter(Collider collision)
        {
            if (enabled && 
                collision.gameObject.CompareTag("InteractableObject") &&
                Time.time > (_lastCutTime + _cutCooldown))
            {
                Vector3 anchorPoint = transform.position;
                InteractableObjectControllerBase interactableObject = collision.gameObject.GetComponentInParent<InteractableObjectControllerBase>();
                if (interactableObject.Colliders[0] == null)
                    return;
                Vector3 size = interactableObject.Colliders[0].bounds.size;
                if (!IsBigEnough(size))
                    return;

                GameObject rightHalf = new GameObject(interactableObject.transform.name);
                InteractableObjectControllerBase rightHalfInteractableObject = rightHalf.AddComponent(interactableObject.GetType()) as InteractableObjectControllerBase;

                Vector3 initialVelocity = interactableObject.Rigidbody.velocity;
                Vector3 initialAngular = interactableObject.Rigidbody.angularVelocity;
                interactableObject.Rigidbody.isKinematic = true;

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
                    UnSpazz(interactableObject, rightHalfInteractableObject, initialVelocity, initialAngular);
                    CheckIfGib(interactableObject);
                    CheckIfGib(rightHalfInteractableObject);
                });
                _lastCutTime = Time.time;
                _effects.PlaceCutEffect(collision.transform.position);
            }
        }

        private bool IsBigEnough(Vector3 size)
        {
            return size.magnitude >= _minimumCutSize;
        }

        /// <summary>
        /// Makes sure the left and right half don't "explode" when cutting through an object.
        /// </summary>
        private void UnSpazz(InteractableObjectControllerBase left, InteractableObjectControllerBase right, Vector3 initialVelocity, Vector3 initialAngular)
        {
            MeshCollider leftCollider = left.Colliders[0] as MeshCollider;
            MeshCollider rightCollider = right.Colliders[0] as MeshCollider;
            leftCollider.sharedMesh = left.SolidMeshGameObject.GetComponent<MeshFilter>().sharedMesh;
            rightCollider.sharedMesh = right.SolidMeshGameObject.GetComponent<MeshFilter>().sharedMesh;
            left.SolidMeshGameObject.transform.position += -transform.up * 0.01f;
            right.SolidMeshGameObject.transform.position += transform.up * 0.01f;
            left.Rigidbody.isKinematic = false;
            left.Rigidbody.velocity = initialVelocity;
            right.Rigidbody.velocity = initialVelocity;
            left.Rigidbody.angularVelocity = initialAngular;
            right.Rigidbody.angularVelocity = initialAngular;
        }

        /// <summary>
        /// Check if the object is so small that we would want it to disappear after some time
        /// </summary>
        private void CheckIfGib(InteractableObjectControllerBase side)
        {
            Vector3 size = side.Colliders[0].bounds.size;
            if(size.magnitude <= _minimumCutSize)
            {
                var gib = side.gameObject.AddComponent<Gib>() as Gib;
                gib.disappearanceTime = 5f;
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