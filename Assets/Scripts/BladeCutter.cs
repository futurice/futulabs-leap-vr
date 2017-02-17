using UnityEngine;
using System.Collections;

namespace Futulabs
{
	
[RequireComponent (typeof(Rigidbody))]
public class BladeCutter : MonoBehaviour
{
	[SerializeField]
	protected Material 					_capMaterial;
	[SerializeField]
	protected AudioSource 				_audioSource;
	[SerializeField]
	protected AudioClip 				_cutAudioClip;
	[SerializeField]
	protected AudioClip 				_swordCollisionAudioClip;
	[SerializeField]
	public bool							_hapticFeedback;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag ("Sword"))
		{
			_audioSource.clip = _swordCollisionAudioClip;
			_audioSource.Play ();
		}
		else if (collision.gameObject.CompareTag ("Fruit"))
		{
			_audioSource.clip = _cutAudioClip;	
			_audioSource.Play ();

			GameObject target = collision.collider.gameObject;
			GameObject[] pieces = MeshCut.Cut(target, transform.position, transform.right, _capMaterial);
			GameObject leftSideGameObject = pieces[(int)MeshCut.MeshCutPieces.LEFT_SIDE];
			GameObject rightSideGameObject = pieces[(int)MeshCut.MeshCutPieces.RIGHT_SIDE];

			leftSideGameObject.tag = target.tag;
			rightSideGameObject.tag = target.tag;

			leftSideGameObject.transform.SetParent(target.transform.parent, true);
			rightSideGameObject.transform.SetParent(target.transform.parent, true);
		}
	}
}

}