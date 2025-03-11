using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterDatabase", menuName = "Scriptable Objects/MonsterDatabase")]
public class MonsterDatabase : ScriptableObject
{
    public List<MonsterData> Items = new List<MonsterData>();
}
