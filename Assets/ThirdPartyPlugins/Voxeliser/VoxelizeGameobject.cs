using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Futulabs
{
	public class VoxelizeGameobject : MonoBehaviour 
	{
		private Voxeliser _voxelizer;
		private Collider _collider;
		private ObjectPool _explosionPool;

		private Vector3 RandomUnit()
		{
			var x = Random.Range(-100,100);
			var y = Random.Range(50,100);
			var z = Random.Range(-100,100);
			return new Vector3(x,y,z).normalized;
		}

		public void Voxelize(int density)
		{
			_explosionPool = GameObject.FindGameObjectWithTag("ExplosionPool").GetComponent<ObjectPool>();
			_collider = GetComponent<Collider>();
			var bounds = _collider.bounds;
			_voxelizer = new Voxeliser(bounds, density, density, density);
			_voxelizer.Voxelize(transform);
			var gridCubeSize = new Vector3(
				bounds.size.x / density,
				bounds.size.y / density,
				bounds.size.z / density);
			var worldCentre = bounds.min + gridCubeSize / 2;
			var voxelRoot = new GameObject("Voxel Root");
			var size = _collider.bounds.size;
			var rootTransform = voxelRoot.transform;

			for(int x = 0; x < density; x++)
			{
				for(int y = 0; y < density; y++)
				{
					for(int z = 0; z < density; z++)
					{
						if (_voxelizer.VoxelMap[x][y][z])
						{
							var go = _explosionPool.Get(typeof(ExplosionCube)) as ExplosionCube;
							go.FadeOut();
							var rb = go.GetComponent<Rigidbody>();
							rb.AddForce(RandomUnit()*Random.Range(400, 800)*size.magnitude);
							go.transform.position = new Vector3(
								x * gridCubeSize.x,
								y * gridCubeSize.y,
								z * gridCubeSize.z) + worldCentre;
							go.transform.rotation = Quaternion.identity;
							go.transform.localScale = gridCubeSize;
							go.transform.SetParent(rootTransform, true);
						}
					}
				}
			}
			Destroy(gameObject);
		}
	}
}