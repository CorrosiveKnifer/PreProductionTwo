using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Application_Singleton

    private static GameManager _instance = null;
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject loader = new GameObject();
                _instance = loader.AddComponent<GameManager>();
                return loader.GetComponent<GameManager>();
            }
            return _instance;
        }
    }

    public static bool HasInstance()
    {
        return _instance != null;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (_instance == this)
        {
            InitialiseFunc();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Second Instance of GameManager was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    #endregion

    public static Vector2 m_sensitivity = new Vector2(-400.0f, 100.0f);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float currentTime = Time.timeScale;
        currentTime += Time.deltaTime;
        currentTime = Mathf.Clamp(currentTime, 0.0f, 1.0f);
        Time.timeScale = currentTime;
    }

    private void InitialiseFunc()
    {
        gameObject.name = "Game Manager";
    }
    private void TimeUpdate()
    {

    }
    public void SlowTime(float _percentage)
    {
        Time.timeScale = 1 - _percentage;
    }
}
