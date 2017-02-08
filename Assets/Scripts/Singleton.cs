using UnityEngine;

namespace Futulabs
{

/// <summary>
/// Generic Singleton utility class.
/// 
/// Be aware this will not prevent a non singleton constructor
/// such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance = null;
    protected static object _lock = new object();
    protected static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning(string.Format("[Singleton] Instance {0} already destroyed on application quit. Won't create again - returning null.", typeof(T)));
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError(string.Format("[Singleton] Multiple instances of {0} detected. Something is really wrong.", typeof(T)));
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(singleton);

                        Debug.Log(string.Format("[Singleton] An instance of {0} is needed in the scene, so {1} was created with DontDestroyOnLoad.", typeof(T), singleton));
                    }
                    else
                    {
                        Debug.Log(string.Format("[Singleton] Using instance already created: {0}", _instance.gameObject.name));
                    }
                }

                return _instance;
            }
        }
    }

    // Guarantee that this is a singleton - can't use the constructor
    protected Singleton()
    {
    }

    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    /// it will create a buggy ghost object that will stay on the Editor scene
    /// even after stopping playing the Application, which is really bad.
    /// This was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    private void OnDestroy()
    {
        // If the app has been running for less than a second we assume we are not quitting
        // but rather getting destroyed for some other reason
        if (Time.time < 1.0f)
        {
            _applicationIsQuitting = false;
        }
        else
        {
            _applicationIsQuitting = true;
        }
    }

    private void Start()
    {
        _applicationIsQuitting = false;
    }
}

}