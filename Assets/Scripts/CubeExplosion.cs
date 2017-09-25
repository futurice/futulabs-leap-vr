using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
namespace Futulabs
{
	public class CubeExplosion : MonoBehaviour 
	{
		// Use this for initialization
		void Start () 
		{
			//Observable.Timer(System.TimeSpan.FromSeconds(3)).Subscribe(_ =>
			//{
				Explode();
			//});
		}

		private Vector3 RandomUnit()
		{
			var x = Random.Range(-100,100);
			var y = Random.Range(-100,100);
			var z = Random.Range(-100,100);
			return new Vector3(x,y,z).normalized;
		}

		public void Explode()
		{
			int divisions = 4;
			Vector3 size = Vector3.one/divisions;
			var explosionPool = GameObject.FindGameObjectWithTag("ExplosionPool").GetComponent<ObjectPool>();

			for(int x = -divisions/2; x <= divisions/2; x++)
			{
				if(x != 0)
				{
					for(int y = -divisions/2; y <= divisions/2; y++)
					{
						if(y != 0)
						{
							for(int z = -divisions/2; z <= divisions/2; z++)
							{
								if(z != 0)
								{
									var instance = explosionPool.Get(typeof(ExplosionCube));
									instance.transform.SetParent(transform);
									instance.transform.localScale = size;
									var xPos = x*size.x - Mathf.Sign(x)*size.x/2;
									var yPos = y*size.y - Mathf.Sign(y)*size.y/2;
									var zPos = z*size.z - Mathf.Sign(z)*size.z/2;
									var rb = instance.GetComponent<Rigidbody>();
									rb.AddForce(RandomUnit()*Random.Range(100, 500));
									instance.transform.localPosition = new Vector3(xPos, yPos,zPos)/50;
									instance.transform.SetParent(null);
								} 
							}
						}
					}
				}
			}
			Destroy(gameObject);
		}

	}
}