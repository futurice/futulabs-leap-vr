﻿using UnityEngine;

namespace Futulabs
{

[System.Serializable]
public enum GameAudioClipType
{
    INTERACTABLE_OBJECT_CREATING = 0,
    INTERACTABLE_OBJECT_COLLISION = 1,
    INTERACTABLE_OBJECT_MATERIALIZATION = 2
}

[System.Serializable]
public class GameAudioClip
{
    public GameAudioClipType audioClipType;
    public AudioClip audioClip;
}

}