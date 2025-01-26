using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    HealthBar _healthBar;
    PlayerBody _playerBody;

    [SerializeField] int _playerHitPoints = 4;
    public int PlayerHitPoints
    {
        get => _playerHitPoints;
        set => _playerHitPoints = Math.Clamp(value, 0, 4);
    }

    void Awake()
    {
        _healthBar = transform.Find("UI/Canvas/HealthBar").GetComponent<HealthBar>();
        _playerBody = GetPlayerObject().transform.Find("Model").GetComponent<PlayerBody>();
    }

    void LateUpdate()
    {
        if (PlayerHitPoints == 0)
        {
            PlayerHitPoints = 4;
            _playerBody.SetPosition(new Vector2(0, 0));
        }
        _healthBar.HasFartUpdraft = _playerBody.HasFartUpdraft;
        _healthBar.HasPizzaForce = _playerBody.HasPizzaForce;
        _healthBar.HitPoints = _playerHitPoints;
    }

    public static GameObject GetSystemObject()
        => SceneManager.GetActiveScene().GetRootGameObjects().First(i => i.name == "System");

    public static GameObject GetPlayerObject()
        => SceneManager.GetActiveScene().GetRootGameObjects().First(i => i.name == "Player");

    public static GameManager GetGameManager()
        => GetSystemObject().GetComponent<GameManager>();

    public void OnItemPickup(string name)
    {
        Debug.Log($"OnItemPickup : {name}");
        if (name == "Corn") PlayerHitPoints++;
        if (name == "Villi") PlayerHitPoints--;
        if (name == "FartBubble") _playerBody.HasFartUpdraft = true;
        if (name == "Pizza") _playerBody.HasPizzaForce = true;
    }
}
