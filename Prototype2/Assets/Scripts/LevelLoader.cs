using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// William de Beer
/// </summary>
public class LevelLoader : MonoBehaviour
{
    #region Singleton

    private static LevelLoader _instance = null;
    public static LevelLoader instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject loader = new GameObject();
                _instance = loader.AddComponent<LevelLoader>();
                return loader.GetComponent<LevelLoader>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        transitionPrefab = Resources.Load<GameObject>("Prefabs/Transitions/TransitionCanvas");

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
            Debug.LogWarning("Second Instance of LevelLoader was created, this instance was destroyed.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    #endregion

    public static bool cheatsEnabled = false;
    public static bool loadingNextArea = false;

    public GameObject CompleteLoadUI;

    public static GameObject transitionPrefab;
    public static Animator transition;

    public bool isTransitioning = false;
    public float transitionTime = 1.0f;

    private void InitialiseFunc()
    {
        //SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        loadingNextArea = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().name));
    }
    public void LoadNextLevel()
    {
        loadingNextArea = true;
        if (SceneManager.sceneCountInBuildSettings <= SceneManager.GetActiveScene().buildIndex + 1) // Check if index exceeds scene count
        {
            StartCoroutine(LoadLevel(SceneManager.GetSceneByBuildIndex(0).name)); // Load menu
        }
        else
        {
            StartCoroutine(LoadLevel(SceneManager.GetSceneByBuildIndex(SceneManager.GetActiveScene().buildIndex + 1).name)); // Loade next scene
        }
    }
    public void LoadNewLevel(string _name)
    {
        if (!isTransitioning)
            StartCoroutine(LoadLevel(_name));
    }
    public void ResetScene()
    {
        loadingNextArea = true;
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().name));
    }

    public void LoadLevelAsync(int levelIndex, float maxTime)
    {
        //StartCoroutine(OperationLoadLevelAsync(levelIndex, maxTime));
    }


    IEnumerator LoadLevel(string _name)
    {
        isTransitioning = true;

        transition = Instantiate(transitionPrefab, transform).GetComponent<Animator>();

        if (transition != null)
        {
            // Wait to let animation finish playing
            yield return new WaitForSeconds(transitionTime);
        }
        // Load Scene
        SceneManager.LoadScene(_name);
        yield return new WaitForSeconds(transitionTime);

        if (transition != null)
        {
            Destroy(transition.gameObject);
            transition = null;
        }
        isTransitioning = false;
        
    }

    //public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    //{
    //    //if(GameManager.HasInstance())
    //    //{
    //    //    var objects = GameManager.instance.m_saveSlot.GetSceneData(scene.buildIndex);

    //    //    if (objects == null || objects.Length == 0)
    //    //        return;

    //    //    foreach (var item in objects)
    //    //    {
    //    //        if (item != null)
    //    //        {
    //    //            int id = item.m_itemID;
    //    //            GameObject prefab = Resources.Load<GameObject>(GameManager.instance.m_items.list[id].placePrefabName);

    //    //            GameObject inWorld = Instantiate(prefab, new Vector3(item.x, item.y, item.z), Quaternion.Euler(item.rx, item.ry, item.rz));
    //    //            inWorld.GetComponent<SerializedObject>().UpdateTo(item);
    //    //        }
    //    //    }

    //    //    GameManager.instance.m_saveSlot.InstansiateNPCs(scene.buildIndex);
    //    //}
    //}

    //public void SaveSceneToSlot(SaveSlot slot)
    //{
    //    slot.SaveObjects(GameObject.FindGameObjectsWithTag("SerializedObject"));
    //    foreach (var item in GameObject.FindGameObjectsWithTag("NPC"))
    //    {
    //        slot.AddNPC(item.GetComponent<NPCScript>());
    //    }
    //}
    //IEnumerator OperationLoadLevelAsync(int levelIndex, float maxTime)
    //{
    //    AsyncOperation gameLoad = SceneManager.LoadSceneAsync(levelIndex);
    //    gameLoad.allowSceneActivation = false;
    //    float time = 0.0f;

    //    while (!gameLoad.isDone)
    //    {
    //        time += Time.deltaTime;
    //        if (gameLoad.progress >= 0.9f)
    //        {
    //            CompleteLoadUI.SetActive(true);

    //            if (InputManager.GetInstance().GetKeyDown(InputManager.ButtonType.BUTTON_SOUTH, 0))
    //            {
    //                gameLoad.allowSceneActivation = true;
    //            }
    //            if (InputManager.GetInstance().GetKeyDown(InputManager.ButtonType.BUTTON_SOUTH, 1))
    //            {
    //                gameLoad.allowSceneActivation = true;
    //            }
    //            if (time >= maxTime)
    //            {
    //                gameLoad.allowSceneActivation = true;
    //            }
    //        }
    //        yield return new WaitForEndOfFrame();
    //    }

    //    CompleteLoadUI.SetActive(false);
    //    yield return null;
    //}
}
