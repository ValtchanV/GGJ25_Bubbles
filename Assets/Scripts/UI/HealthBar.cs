using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Sprite[] Images = new Sprite[0];
    [SerializeField] int hitPoints = 0;

    public int HitPoints {
        get => hitPoints;
        set
        {
            hitPoints = value;
            _image.sprite = Images.ElementAtOrDefault(value);
        }
    }

    private Image _image;

    void Awake()
    {
        _image = transform.GetComponent<Image>();
        HitPoints = hitPoints;
    }
}
