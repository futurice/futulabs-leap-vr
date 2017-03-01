using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhatDoICollideWith : MonoBehaviour
{

    private HashSet<Collider> colliders = new HashSet<Collider>();
    public List<string> colliderNames = new List<string>();

    public void Update()
    {
        colliderNames.Clear();
        foreach (var collider in colliders)
        {
            colliderNames.Add(collider.gameObject.name);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        colliders.Add(collision.collider);
    }

    private void OnCollisionExit(Collision collision)
    {
        colliders.Remove(collision.collider);
    }
}
