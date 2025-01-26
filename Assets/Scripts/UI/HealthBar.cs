using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Sprite[] Images = new Sprite[0];
    [SerializeField] int hitPoints = 0;
    [SerializeField] bool hasFartUpdraft = false;
    [SerializeField] bool hasPizzaForce = false;

    public int HitPoints
    {
        get => hitPoints;
        set
        {
            if (hitPoints == value) return;
            hitPoints = value;
            _image.sprite = Images.ElementAtOrDefault(value);
        }
    }

    public bool HasFartUpdraft
    {
        get => hasFartUpdraft;
        set
        {
            if (hasFartUpdraft == value) return;
            hasFartUpdraft = value;
            _hasFartUpdraftImage.enabled = hasFartUpdraft;
        }
    }

    public bool HasPizzaForce
    {
        get => hasPizzaForce;
        set
        {
            if (hasPizzaForce == value) return;
            hasPizzaForce = value;
            _hasPizzaForceImage.enabled = hasPizzaForce;
        }
    }

    private Image _image;
    private Image _hasFartUpdraftImage;
    private Image _hasPizzaForceImage;

    void Awake()
    {
        _image = transform.GetComponent<Image>();
        _image.sprite = Images.ElementAtOrDefault(hitPoints);

        _hasFartUpdraftImage = transform.Find("HasFartUpdraft").GetComponent<Image>();
        _hasFartUpdraftImage.enabled = hasFartUpdraft;

        _hasPizzaForceImage = transform.Find("HasPizzaForce").GetComponent<Image>();
        _hasPizzaForceImage.enabled = hasPizzaForce;
    }
}
