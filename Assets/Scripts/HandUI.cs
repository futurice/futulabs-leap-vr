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

    private bool MenuShown = false;
	
	// Update is called once per frame
	void Update () {
        RotateToShowMenu(Palm);
	}

    private void RotateToShowMenu(Transform t)
    {
        if (t.localRotation.eulerAngles.z >= RotationThreshold && !MenuShown)
        {
            ShowMenu();
            MenuShown = true;
            return;
        }
        else if (t.localRotation.eulerAngles.z < RotationThreshold && MenuShown)
        {
            HideMenu();
            MenuShown = false;
        }
    }

    private void ShowMenu()
    {
        Menu.SetActive(true);
        Debug.Log("Setting active");
    }

    private void HideMenu()
    {
        Menu.SetActive(false);
    }
}
