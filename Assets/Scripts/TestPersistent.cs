using System;
using System.Collections;
using GanFramework.Core.Data.Persistent;
using UnityEngine;

public class TestPersistent : MonoBehaviour
{
    public Player player = new Player();
    void Start()
    {
        player.Hp += 100;
        if (JsonSaveManager.Exists<Player>())
        {
            Debug.Log("Exists");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            JsonSaveManager.Save(player);
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            player = JsonSaveManager.Load<Player>();
        }
        
        if (Input.GetKeyDown(KeyCode.D))
        {
            JsonSaveManager.Delete<Player>();
        }
    }
}

[Serializable]
[SaveKey("player")]
public class Player
{
    public string Name;
    public int Hp = 100;
    public float Speed;
}
