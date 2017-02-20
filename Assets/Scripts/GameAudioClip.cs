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
    INTERACTABLE_OBJECT_STICK = 6,
    PLASMA_CUTTER_ACTIVATE = 7,
    PLASMA_CUTTER_DEACTIVATE = 8,
    PLASMA_CUTTER_LOOP = 9,
    PLASMA_CUTTER_SWING0 = 10,
    PLASMA_CUTTER_SWING1 = 11,
    PLASMA_CUTTER_SWING2 = 12,
    PLASMA_CUTTER_SWING3 = 13
}

[System.Serializable]
public class GameAudioClip
{
    public GameAudioClipType audioClipType;
    public AudioClip audioClip;
}

}