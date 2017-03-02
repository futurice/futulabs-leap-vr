using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraOffset : MonoBehaviour
{
    [SerializeField]
    protected Transform _headtracker;

    
    private void Update()
    {
        var localPos = transform.localPosition;
        localPos *= -1;
        _headtracker.localPosition = localPos;
    }
}
