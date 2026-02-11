using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "EndlessRunner/SpawnPatternGroupList")]
public class SpawnPatternGroupList : ScriptableObject
{
    public List<SpawnPointGroup> allCombinations = new List<SpawnPointGroup>();
}
