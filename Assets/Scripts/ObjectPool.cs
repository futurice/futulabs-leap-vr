using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Futulabs
{
	[ExecuteInEditMode]
	public class ObjectPool : MonoBehaviour 
	{
		[SerializeField] private GameObject _poolableObject;
		private List<GameObject> _inactivePool = new List<GameObject>();
		private List<GameObject> _activePool = new List<GameObject>();
		public int PoolSize = 64;

		void OnEnable()
		{
			foreach(Transform child in transform)
			{
				DestroyImmediate(child.gameObject);
			}
			_inactivePool.Clear();
			_activePool.Clear();
			if(_poolableObject != null)
			{
				for(int i = 0; i < PoolSize; i++)
				{
					var obj = Instantiate(_poolableObject);
					Return(obj);
				}
			}
		}

		public void Return(GameObject obj)
		{
			obj.transform.SetParent(transform);
			obj.SetActive(false);
			_inactivePool.Add(obj);
			_activePool.Remove(obj);
		}

		public Component Get(Type t)
		{
			var obj = _inactivePool.FirstOrDefault();
			_inactivePool.Remove(obj);
			_activePool.Add(obj);
			obj.SetActive(true);
			return obj.GetComponent(t);
		}

	}
}