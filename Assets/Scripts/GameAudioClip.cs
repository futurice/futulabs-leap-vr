using UnityEngine;

namespace Futulabs
{

[System.Serializable]
public enum GameAudioClipType
{
    INTERACTABLE_OBJECT_CREATING = 0,
    INTERACTABLE_OBJECT_COLLISION = 1,
    INTERACTABLE_OBJECT_MATERIALIZATION = 2,
    UI_CLICK_DOWN = 3,
    UI_CLICK_UP = 4,
    BASKETBALL_SCORE = 5,
    INTERACTABLE_OBJECT_STICK = 6
}

[System.Serializable]
public class GameAudioClip
{
    public GameAudioClipType audioClipType;
    public AudioClip audioClip;
}

}