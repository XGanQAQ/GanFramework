using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "SO/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public string playerName;
    public int level;
    public float health;

    public override string ToString()
    {
        return $"PlayerConfig(Name={playerName}, Level={level}, Health={health})";
    }
}
