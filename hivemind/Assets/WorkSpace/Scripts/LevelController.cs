using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Scene = UnityEngine.SceneManagement.Scene;

public class LevelController : MonoBehaviour
{
    public List<MapData> mapData;
    public MapData currentMapdata;
    public int levelMap;
    public LevelPanel levelPanel;
    public Tilemap tileMapBaseOnMapData;
    public static LevelController Instance;

    public event Action<LevelDetail> OnSpawnEnd;

    /*------------- bảng cấu hình camera -------------*/
    // Z‑pos theo level
    private readonly Dictionary<int, float> levelToZ = new Dictionary<int, float>
    {
        { 4,  -38f },
        { 5,  -38f },
        {10,-38f},
        { 11, -38f },
        { 12, -38f }
    };

    // FOV theo level (chỉ khai những level cần khác mặc định)
    private readonly Dictionary<int, float> levelToFOV = new Dictionary<int, float>
    {
        { 12, 94f }  ,
        { 11, 94f }  ,
        { 10, 94f}        // level 12 dùng FOV 97
    };
    /*-------------------------------------------------*/

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        levelPanel = FindAnyObjectByType<LevelPanel>();
        levelPanel.OnStartGame += idx => levelMap = idx;
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MakeUI") return;
        StartCoroutine(LoadMapData(levelMap));
    }

    public IEnumerator LoadMapData(int levelIndex)
    {
        currentMapdata = mapData.FirstOrDefault(md => md.levelDefine.Contains(levelIndex));
        if (currentMapdata == null)
        {
            Debug.LogError($"Không tìm thấy MapData chứa level {levelIndex}");
            yield break;
        }

        GameObject instantiatedMap = Instantiate(currentMapdata.mapPrefabs);

        LevelDetail? detail = currentMapdata
            .listMapLevel
            .FirstOrDefault(ld => ld.level == levelIndex);

        if (detail == null)
        {
            Debug.LogError($"Không tìm thấy LevelDetail cho level {levelIndex}");
            yield break;
        }

        Tilemap[] tms = instantiatedMap.GetComponentsInChildren<Tilemap>(true)
            .Where(tm => tm.CompareTag("TileMapLevel"))
            .ToArray();

        int idxInArray = currentMapdata.levelDefine.IndexOf(levelIndex);
        if (idxInArray >= 0 && idxInArray < tms.Length)
        {
            tileMapBaseOnMapData = tms[idxInArray];

            yield return new WaitForSeconds(0.2f);
            OnSpawnEnd?.Invoke(detail.Value);

            /*--- cập nhật camera ---*/
            AdjustCameraByLevel(levelIndex);
        }
        else
        {
            Debug.LogError($"Không tìm thấy tilemap cho level {levelIndex}");
        }
    }

    private void AdjustCameraByLevel(int levelIndex)
    {
        if (CameraController.Instance == null) return;

        // Z‑position
        float z = levelToZ.TryGetValue(levelIndex, out float zVal) ? zVal : -36f;
        var camTransform = CameraController.Instance.transform;
        camTransform.position = new Vector3(camTransform.position.x, camTransform.position.y, z);

        // Field of View
        if (levelToFOV.TryGetValue(levelIndex, out float fov))
        {
            CameraController.Instance.GetComponent<Camera>().fieldOfView = fov;
        }
        else
        {
            // về mặc định (60) nếu cần
            CameraController.Instance.GetComponent<Camera>().fieldOfView = 80f;
        }
    }

    public void TestManualMap()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
}
