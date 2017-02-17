using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
                rightHalf.transform.SetParent(ObjectManager.Instance.ObjectContainer);
                rightHalf.transform.localScale = Vector3.Scale(interactableObject.transform.parent.localScale, interactableObject.transform.localScale);
                rightHalf.tag = "InteractableObject";

                GameObject[] solidPieces = CutMesh(
                    interactableObject.SolidMeshGameObject,
                    anchorPoint,
                    interactableObject.transform,
                    rightHalf.transform,
                    _capMaterial);

                GameObject[] outlinePieces = CutMesh(
                    interactableObject.OutlineMeshGameObject,
                    anchorPoint,
                    solidPieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE].transform,
                    solidPieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].transform,
                    _capMaterial);

                // Rebuild the names
                //solidPieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE].transform.name = "Solid";
                //outlinePieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE].transform.name = "Outline";
                //solidPieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].transform.name = "Solid";
                //outlinePieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE].transform.name = "Outline";

                // Rebuild the references for the new InteractableObjectController
                rightHalfInteractableObject.RebuildReferences();

                _lastCutTime = Time.time;
            }
        }

        private GameObject[] CutMesh(GameObject target, Vector3 anchorPoint, Transform leftParent, Transform rightParent, Material capMaterial)
        {
            GameObject[] pieces = MeshCut.Cut(target, anchorPoint, transform.up, capMaterial);
            GameObject leftSideGameObject = pieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE];
            GameObject rightSideGameObject = pieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE];

            leftSideGameObject.tag = target.tag;
            rightSideGameObject.tag = target.tag;

            leftSideGameObject.transform.SetParent(leftParent, true);
            rightSideGameObject.transform.SetParent(rightParent, true);
            rightSideGameObject.transform.localScale = leftSideGameObject.transform.localScale;

            return pieces;
        }
    }

}