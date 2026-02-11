using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LevelGame", menuName = "ScriptableObjects/LevelGame", order = 1)]
public class LevelGame : ScriptableObject
{
    public string SceneName;
    public int LevelIndex;
}