using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/*
[System.Serializable]
public enum SpawnShape
{
    Box,
    Icosphere,
    Star
}*/

public class MultiToggle : MonoBehaviour
{
    public Color Selected;
    public Color NotSelected;
    public List<Button> Buttons = new List<Button>();

    public void ChangeButton(int shape)
    {
        foreach(var b in Buttons)
        {
            b.GetComponent<Outline>().effectColor = NotSelected;
        }

        Buttons[shape].GetComponent<Outline>().effectColor = Selected;
        //TODO: Change the selected shape here
    }
}
