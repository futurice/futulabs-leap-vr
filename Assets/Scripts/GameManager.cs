using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Futulabs
{

public class GameManager : Singleton<GameManager>
{
    private bool _isGravityOn = true;

    /// <summary>
    /// Returns the object that will be created by the ObjectManager.
    /// </summary>
    public InteractableObjectType CurrentlySelectedObject
    {
        get
        {
            return ObjectManager.Instance.CurrentCreatableObjectType;
        }
    }

    /// <summary>
    /// Returns whether the gravity is on
    /// </summary>
    public bool IsGravityOn
    {
        get
        {
            return _isGravityOn;
        }

        private set
        {
            _isGravityOn = value;
        }
    }
    
    public void ChangeCreatedInteractableObjectType (InteractableObjectType type)
    {
        Debug.LogFormat("GameManager ChangeCreatedInteractableObjectType: Changing object type to {0}", type);
        ObjectManager.Instance.CurrentCreatableObjectType = type;
    }

    public void ToggleGravity()
    {
        Debug.LogFormat("GameManager ToggleGravity: Changing gravity to: {0}", !IsGravityOn);
        IsGravityOn = !IsGravityOn;
        ObjectManager.Instance.ToggleGravityForInteractableObjects(IsGravityOn);
    }

    public void ResetGame()
    {
        Debug.Log("GameManager ResetGame: Resetting the game");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

}