using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private SegmentManager segmentManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private MirrorSegmentController mirrorSegmentManager;
    [SerializeField] private CharacterController[] characterControllers;

    public static GameController Instance { get; private set; }

    private InGame inGame;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        CharacterController.OnPlayerHitGate += OnHitGate;
    }

    void OnDisable()
    {
        CharacterController.OnPlayerHitGate -= OnHitGate;
    }

    void Start()
    {
        UIManager.OnLoadingComplete += StartGame;
        InGame.OnTimeUp += OnEndGame;
        inGame = FindFirstObjectByType<InGame>();
        if (inGame == null)
        {
            Debug.LogError("❌ InGame not found.");
            return;
        }
    }

    void OnDestroy()
    {
        UIManager.OnLoadingComplete -= StartGame;
        InGame.OnTimeUp -= OnEndGame;
    }


    [ContextMenu("🔄 Start Game Now")]
    public void StartGame()
    {
        levelManager.InitializeLevel();
        StartCoroutine(segmentManager.InitializeSegmentsCoroutine());
        StartCoroutine(mirrorSegmentManager.AutoFindSegmentsCoroutine());
    }

    public void ReloadGameLevel()
    {
        levelManager.InitializeLevel();

        StartCoroutine(segmentManager.InitializeSegmentsCoroutine());
        StartCoroutine(mirrorSegmentManager.AutoFindSegmentsCoroutine());
    }

    public void OnEndGame()
    {
        foreach (var character in characterControllers)
        {
            character.WinGame();
        }
        segmentManager.WinGame();
        mirrorSegmentManager.ForceGateEndNow();
    }

    public void OnHitGate()
    {
        segmentManager.StopAllSegments();
        inGame.OnLevelCompleted(true);
    }
}
