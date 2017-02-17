using UnityEngine;
using System.Collections;
using UniRx;

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
		GameObject target = collision.collider.gameObject;
		MeshCut.Cut(target, transform.position, transform.right, _capMaterial).Subscribe (objects => {
			
		});
				
	}
}

}