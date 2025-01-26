using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameObject GetSystemObject()
        => SceneManager.GetActiveScene().GetRootGameObjects().First(i => i.name == "System");

    public static GameManager GetGameManager() 
        => GetSystemObject().GetComponent<GameManager>();
    
    public void OnItemPickup(string name)
    {
        Debug.Log($"OnItemPickup : {name}");
    }
}
