using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    HealthBar _healthBar;

    void Awake()
    {
        _healthBar = transform.Find("UI/Canvas/HealthBar").GetComponent<HealthBar>();
    }

    public static GameObject GetSystemObject()
        => SceneManager.GetActiveScene().GetRootGameObjects().First(i => i.name == "System");

    public static GameManager GetGameManager() 
        => GetSystemObject().GetComponent<GameManager>();
    
    public void OnItemPickup(string name)
    {
        Debug.Log($"OnItemPickup : {name}");
        if (name == "Corn") _healthBar.HitPoints = Math.Min(_healthBar.HitPoints + 1, 4);
    }
}
