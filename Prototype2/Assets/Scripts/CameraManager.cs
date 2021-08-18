using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

//Michael Jordan
public class CameraManager : MonoBehaviour
{
    #region Scene_Singleton

    public static CameraManager instance = null;

    public static bool HasInstance()
    {
        return instance != null;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance == this)
        {
            InitialiseFunc();
        }
        else
        {
            Debug.LogWarning("Second Instance of CameraManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    #endregion

    private Dictionary<string, PlayableDirector> m_directors;

    public void InitialiseFunc()
    {
        gameObject.name = $"Camera Manager ({gameObject.name})";

        m_directors = new Dictionary<string, PlayableDirector>();

        foreach (var item in GetComponents<PlayableDirector>())
        {
            m_directors.Add(item.playableAsset.name, item);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool PlayDirector(string directorName)
    {
        PlayableDirector director = null;
        if(m_directors.TryGetValue(directorName, out director) && !IsADirectorPlaying())
        {
            director.Play();
            return true;
        }
        return false;
    }

    public bool StopDirector(string directorName)
    {
        PlayableDirector director = null;
        if (m_directors.TryGetValue(directorName, out director))
        {
            director.Stop();
            return true;
        }
        return false;
    }

    public bool IsDirectorPlaying(string directorName)
    {
        PlayableDirector director = null;
        if (m_directors.TryGetValue(directorName, out director))
        {
            return director.state == PlayState.Playing;
        }
        return false;
    }

    public bool IsADirectorPlaying()
    {
        foreach (var item in m_directors)
        {
            if(item.Value.state == PlayState.Playing)
            {
                return true;
            }
        }
        return false;
    }
}
