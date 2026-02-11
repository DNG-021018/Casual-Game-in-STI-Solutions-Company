using UnityEngine;

namespace Wja8YNiR_PrismPath
{
    [CreateAssetMenu(menuName = "Level Data/Level List", fileName = "Level Data")]
    public class Wja8YNiR_LevelData : ScriptableObject
    {
        [SerializeField] public Wja8YNiR_Level[] levels;
    }
}
