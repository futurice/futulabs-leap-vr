using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    /// <summary>
    /// Object used to figure out when to introduce UI
    /// </summary>
    public Transform Palm;
    /// <summary>
    /// Menu to show/hide
    /// </summary>
    public GameObject Menu; 
    public float RotationThreshold = 200f;
    public float TimeThresHold = 1f;
    private float TimeDT = 0;

    private bool MenuShown = false;
	
	// Update is called once per frame
	void Update () {
        RotateToShowMenu(Palm);
	}

    private void RotateToShowMenu(Transform t)
    {
        Vector3 rot = t.localRotation.eulerAngles;
        if (rot.z >= RotationThreshold && !MenuShown && rot.z >= 0)
        {
            TimeDT += Time.deltaTime;
            if (TimeDT >= TimeThresHold)
            {
                ShowMenu();
                MenuShown = true;
                TimeDT = 0;
            }
        }
        if (rot.z < RotationThreshold && MenuShown)
        {
            TimeDT += Time.deltaTime;
            if (TimeDT >= TimeThresHold)
            {
                HideMenu();
                MenuShown = false;
                TimeDT = 0;
            }
        }
    }

    private void ShowMenu()
    {
        Menu.SetActive(true);
    }

    private void HideMenu()
    {
        Menu.SetActive(false);
    }
}
