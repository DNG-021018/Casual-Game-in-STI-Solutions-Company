using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "LevelGame", menuName = "Game/LevelGame", order = 1)]
public class LevelGame : ScriptableObject
{
    public static LevelGame Instance { get; private set; }
    [SerializeField] private string sceneName;
    [SerializeField] private int levelIndex;
    [SerializeField] private int requiredScore = 10;
    [SerializeField] private int timeLimit = 60; // Default time limit in seconds
    [SerializeField] private int chickenSpawn = 0;
    [SerializeField] private int hoeSpawn = 0;

    public void Initialize(string sceneName, int levelIndex, int requiredScore = 10, int timeLimit = 60, int chickenSpawn = 0, int hoeSpawn = 0)
    {
        this.sceneName = sceneName;
        this.levelIndex = levelIndex;
        this.requiredScore = requiredScore;
        this.timeLimit = timeLimit;
        this.chickenSpawn = chickenSpawn;
        this.hoeSpawn = hoeSpawn;
    }

    public string SceneName => sceneName;
    public int LevelIndex => levelIndex;
    public int RequiredScore => requiredScore;
    public int TimeLimit => timeLimit;
    public int ChickenSpawn => chickenSpawn;
    public int HoeSpawn => hoeSpawn;
}
